﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;

public class BattlegroundsCompGuideViewModel : ViewModel
{
	public BattlegroundsCompGuide CompGuide { get; }

	private LinearGradientBrush CreateLinearGradientBrush(Color color1, Color color2)
	{
		var brush = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1)
		};
		brush.GradientStops.Add(new GradientStop(color1, 0.0));
		brush.GradientStops.Add(new GradientStop(color2, 1.0));

		return brush;
	}

	private static string GetTierText(int tier) => tier switch
	{
		1 => "S",
		2 => "A",
		3 => "B",
		4 => "C",
		5 => "D",
		_ => "?"
	};

	private LinearGradientBrush GetTierColor(int tier) => tier switch
	{
		1 => CreateLinearGradientBrush(Color.FromRgb(64, 138, 191), Color.FromRgb(56, 95, 122)),
		2 => CreateLinearGradientBrush(Color.FromRgb(107, 160, 54), Color.FromRgb(88, 121, 55)),
		3 => CreateLinearGradientBrush(Color.FromRgb(146, 160, 54), Color.FromRgb(104, 121, 55)),
		4 => CreateLinearGradientBrush(Color.FromRgb(160, 124, 54), Color.FromRgb(121, 95, 55)),
		5 => CreateLinearGradientBrush(Color.FromRgb(160, 72, 54), Color.FromRgb(121, 66, 55)),
		_ => CreateLinearGradientBrush(Color.FromRgb(112, 112, 112), Color.FromRgb(64, 64, 64))
	};

	private readonly bool _isPreLobby;

	public BattlegroundsCompGuideViewModel(BattlegroundsCompGuide compGuide, bool isPreLobby)
	{
		CompGuide = compGuide;
		_isPreLobby = isPreLobby;

		CoreCardId = CompGuide.CoreCards.FirstOrDefault();
		CardToShowInUi = Database.GetCardFromId(CompGuide.RepresentativeCard);
		CardAsset = new(CardToShowInUi, Utility.Assets.CardAssetType.Portrait);
		DifficultyText = CompGuide.Difficulty switch
		{
			1 => "Hard",
			2 => "Medium",
			3 => "Easy",
			_ => "Unknown"
		};
		DifficultyColor = CompGuide.Difficulty switch
		{
			1 => "#7f303e",
			2 => "#917b43",
			3 => "#49634b",
			_ => "#404040"
		};

		var tier = CompGuide.AdjustedTier ?? CompGuide.Tier;
		IsTierAdjusted = tier != CompGuide.Tier;

		TierText = GetTierText(tier);
		TierColor = GetTierColor(tier);
		BaseTierText = GetTierText(CompGuide.Tier);
		BaseTierColor = GetTierColor(CompGuide.Tier);

		PrimaryTribe = CompGuide.PrimaryTribe;
		CommonEnablerTags = ReferencedCardRun.ParseCardsFromText(CompGuide.CommonEnablers);
		WhenToCommitTags = ReferencedCardRun.ParseCardsFromText(CompGuide.WhenToCommit);
		HowToPlay = ReferencedCardRun.ParseCardsFromText(CompGuide.HowToPlay).FirstOrDefault();
		Core.Overlay.BattlegroundsMinionPinningViewModel.PinsChanged += OnGlobalPinsChanged;
		OnGlobalPinsChanged(this, EventArgs.Empty);
	}

	private void OnGlobalPinsChanged(object? sender, EventArgs e)
	{
		if(!Application.Current.Dispatcher.CheckAccess())
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() => OnGlobalPinsChanged(sender, e)));
			return;
		}
		OnPropertyChanged(nameof(AreAllCompCardsPinned));
	}

	public bool AreAllCompCardsPinned
	{
		get
		{
			var ids = GetCardIdsToPin();
			return ids.Count > 0 && ids.All(id => Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(id));
		}
	}

	private List<string> GetCardIdsToPin()
	{
		var ids = new List<string>();
		void Add(int dbfId)
		{
			var card = Database.GetCardFromDbfId(dbfId, false);
			if(card?.Id is string id && !string.IsNullOrEmpty(id) && card.IsKnownCard && !ids.Contains(id))
				ids.Add(id);
		}

		// premium guides come with a list filtered by the server, free ones have to be collected from the guide
		if(CompGuide.MinionsToPin != null)
		{
			foreach(var dbfId in CompGuide.MinionsToPin)
				Add(dbfId);
			return ids;
		}

		var available = GetAvailableCardIds();
		var inlineDbfIds = WhenToCommitTags.Concat(CommonEnablerTags)
			.Append(HowToPlay ?? Enumerable.Empty<Inline>())
			.SelectMany(line => line)
			.OfType<ReferencedCardRun>()
			.Select(r => r.Card?.DbfId)
			.OfType<int>();
		foreach(var dbfId in CompGuide.CoreCards.Concat(CompGuide.AddonCards).Concat(inlineDbfIds))
		{
			if(available == null || available.Contains(dbfId))
				Add(dbfId);
		}
		return ids;
	}

	[LocalizedProp]
	public string LastUpdatedFormatted => LocUtil.GetAge(CompGuide.LastUpdated);

	public int PrimaryTribe { get; set; }
	public int CoreCardId { get; }

	public  IEnumerable<Inline>[] CommonEnablerTags { get; }
	public IEnumerable<Inline>[] WhenToCommitTags { get; }

	public string TierText { get; }
	public LinearGradientBrush TierColor { get; }
	public string BaseTierText { get; }
	public LinearGradientBrush BaseTierColor { get; }
	// 1 when promoted, -1 when demoted (tier 1 is the best)
	public bool IsTierAdjusted { get; }

	// the base tier badge sits where {0} is in the localized string
	private static string[] TierAdjustedFromParts => LocUtil.Get("Battlegrounds_CompGuide_TierAdjustedFrom").Split(new[] { "{0}" }, 2, StringSplitOptions.None);

	[LocalizedProp]
	public string TierAdjustedFromPrefix => TierAdjustedFromParts[0].Trim();

	[LocalizedProp]
	public string TierAdjustedFromSuffix => TierAdjustedFromParts.ElementAtOrDefault(1)?.Trim() ?? "";
	public Hearthstone.Card? CardToShowInUi { get; }

	public string DifficultyText { get; }
	public string DifficultyColor { get; }

	public CardAssetViewModel CardAsset { get; }

	private HashSet<int>? _availableCardIds;
	private BattlegroundsDb? _availableCardIdsDb;

	// null means every card is available, which is the case before a match, where there is no minion pool yet
	private HashSet<int>? GetAvailableCardIds()
	{
		if(_isPreLobby)
			return null;

		var db = BattlegroundsDbSingleton.Current;
		if (_availableCardIds == null || _availableCardIdsDb != db)
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces();
			if(availableRaces == null)
				return null;
			var currentRaces = new HashSet<Race>(availableRaces.Concat(new[] { Race.ALL, Race.INVALID }));
			var availableCards = db.GetCardsByRaces(currentRaces, Core.Game.IsBattlegroundsDuosMatch)
				.Concat(db.GetSpells(Core.Game.IsBattlegroundsDuosMatch));
			_availableCardIds = new HashSet<int>(availableCards.Select(card => card.DbfId));
			_availableCardIdsDb = db;
		}
		return _availableCardIds;
	}

	public void OnMinionPoolChanged()
	{
		OnPropertyChanged(nameof(CoreCards));
		OnPropertyChanged(nameof(AddonCards));
	}

	private IEnumerable<BattlegroundsMinionViewModel> GetBattlegroundsMinions(IEnumerable<int> cardIds, bool checkAvailability = true)
	{
		var availableCardIds = checkAvailability ? GetAvailableCardIds() : null;

		return cardIds.Select(cardId =>
		{
			var card = Database.GetCardFromDbfId(cardId, false);
			if (card == null)
				return null;

			card.BaconCard = true;
			return card;
		}).WhereNotNull().Where(x => x.IsKnownCard).Select(card =>
			new BattlegroundsMinionViewModel
			{
				Attack = card.Attack,
				Health = card.Health,
				Tier = card.TechLevel,
				Card = card,
				IsAvailable = availableCardIds?.Contains(card.DbfId) ?? true
			});
	}

	public IEnumerable<BattlegroundsMinionViewModel> CoreCards => GetBattlegroundsMinions(CompGuide.CoreCards, IsTier7Enabled);

	public IEnumerable<BattlegroundsMinionViewModel> AddonCards => GetBattlegroundsMinions(CompGuide.AddonCards, IsTier7Enabled);

	public ICommand ShowExampleBoardsCommand => new Command(() =>
	{
		var availableCardIds = GetAvailableCardIds();
		var cards = CompGuide.CoreCards.Select(cardId =>
		{
			var card = Database.GetCardFromDbfId(cardId, false);
			if(card == null || availableCardIds?.Contains(card.DbfId) == false)
				return null;

			card.BaconCard = true;
			return card;
		}).WhereNotNull().ToArray();
		Core.Overlay.BattlegroundsInspirationViewModel.SetKeyMinion(CompGuide.Name, cards);
		Core.Overlay.ShowBgsInspiration();
		Core.Game.Metrics.BattlegroundsCompGuidesInspirationClicks++;
	});

	public ICommand PinAllCompCardsCommand => new Command(() =>
	{
		var ids = GetCardIdsToPin();

		if(ids.Count == 0)
			return;

		var allPinned = ids.All(id => Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(id));
		if(allPinned)
		{
			foreach(var id in ids)
				Core.Overlay.BattlegroundsMinionPinningViewModel.UnpinCard(id);
		}
		else
		{
			Core.Game.Metrics.TavernMarkersPinnedFromCompGuide = true;
			foreach(var id in ids)
				Core.Overlay.BattlegroundsMinionPinningViewModel.PinCard(id);
		}
	});

	private static bool IsTier7Enabled => (HSReplayNetOAuth.AccountData?.IsTier7 ?? false)
	                                       || Tier7Trial.IsTrialForCurrentGameActive(Core.Game.MetaData.ServerInfo
		                                       ?.GameHandle);

	public bool ExampleBoardsButtonEnabled => IsTier7Enabled;

	// the example boards are filtered by the current minion pool, which only exists during a match
	public Visibility ExampleBoardsButtonVisibility => _isPreLobby ? Visibility.Collapsed : Visibility.Visible;

	public IEnumerable<Inline>? HowToPlay { get; }
}
