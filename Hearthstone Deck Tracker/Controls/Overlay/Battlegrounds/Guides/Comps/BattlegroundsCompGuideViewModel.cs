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

	public BattlegroundsCompGuideViewModel(BattlegroundsCompGuide compGuide)
	{
		CompGuide = compGuide;

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

		TierText = CompGuide.Tier switch
		{
			1 => "S",
			2 => "A",
			3 => "B",
			4 => "C",
			5 => "D",
			_ => "?"
		};

		TierColor = CompGuide.Tier switch
		{
			1 => CreateLinearGradientBrush(Color.FromRgb(64, 138, 191), Color.FromRgb(56, 95, 122)),
			2 => CreateLinearGradientBrush(Color.FromRgb(107, 160, 54), Color.FromRgb(88, 121, 55)),
			3 => CreateLinearGradientBrush(Color.FromRgb(146, 160, 54), Color.FromRgb(104, 121, 55)),
			4 => CreateLinearGradientBrush(Color.FromRgb(160, 124, 54), Color.FromRgb(121, 95, 55)),
			5 => CreateLinearGradientBrush(Color.FromRgb(160, 72, 54), Color.FromRgb(121, 66, 55)),
			_ => CreateLinearGradientBrush(Color.FromRgb(112, 112, 112), Color.FromRgb(64, 64, 64))
		};

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

	private bool AreAllDbfIdsPinned(IEnumerable<int> dbfIds)
	{
		var available = GetAvailableCardIds();
		var any = false;
		foreach(var dbfId in dbfIds)
		{
			if(available != null && !available.Contains(dbfId))
				continue;
			var card = Database.GetCardFromDbfId(dbfId, false);
			if(card == null || !card.IsKnownCard)
				continue;
			any = true;
			if(!Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(card.Id))
				return false;
		}
		return any;
	}

	private bool AreAllInlinePinned(IEnumerable<Inline>[] tags)
	{
		var any = false;
		foreach(var line in tags)
		{
			foreach(var inline in line)
			{
				if(inline is ReferencedCardRun r && r.Card?.DbfId is int dbfId)
				{
					var card = Database.GetCardFromDbfId(dbfId, false);
					if(card == null || !card.IsKnownCard)
						continue;
					any = true;
					if(!Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(card.Id))
						return false;
				}
			}
		}
		return any;
	}

	private bool AreAllInlinePinned(IEnumerable<Inline>? inlines)
	{
		if(inlines == null)
			return false;
		var any = false;
		foreach(var inline in inlines)
		{
			if(inline is ReferencedCardRun r && r.Card?.DbfId is int dbfId)
			{
				var card = Database.GetCardFromDbfId(dbfId, false);
				if(card == null || !card.IsKnownCard)
					continue;
				any = true;
				if(!Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(card.Id))
					return false;
			}
		}
		return any;
	}

	public bool AreAllCompCardsPinned
	{
		get
		{
			var coreCardsPinned = AreAllDbfIdsPinned(CompGuide.CoreCards);
			var addonCardsPinned = AreAllDbfIdsPinned(CompGuide.AddonCards);
			var whenToCommitPinned = AreAllInlinePinned(WhenToCommitTags);
			var commonEnablersPinned = AreAllInlinePinned(CommonEnablerTags);
			var howToPlayPinned = AreAllInlinePinned(HowToPlay);

			var hasAnyCards = coreCardsPinned || addonCardsPinned || whenToCommitPinned || commonEnablersPinned || howToPlayPinned;
			if(!hasAnyCards)
				return false;

			return coreCardsPinned && addonCardsPinned && whenToCommitPinned && commonEnablersPinned && howToPlayPinned;
		}
	}


	[LocalizedProp]
	public string LastUpdatedFormatted => LocUtil.GetAge(CompGuide.LastUpdated);

	public int PrimaryTribe { get; set; }
	public int CoreCardId { get; }

	public  IEnumerable<Inline>[] CommonEnablerTags { get; }
	public IEnumerable<Inline>[] WhenToCommitTags { get; }

	public string TierText { get; }
	public LinearGradientBrush TierColor { get; }
	public Hearthstone.Card? CardToShowInUi { get; }

	public string DifficultyText { get; }
	public string DifficultyColor { get; }

	public CardAssetViewModel CardAsset { get; }

	private HashSet<int>? _availableCardIds;
	private HashSet<int> GetAvailableCardIds()
	{
		if (_availableCardIds == null)
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces();
			var currentRaces = new HashSet<Race>(availableRaces.Concat(new[] { Race.ALL, Race.INVALID }));
			var availableCards = BattlegroundsDbSingleton.Instance.GetCardsByRaces(currentRaces, Core.Game.IsBattlegroundsDuosMatch)
				.Concat(BattlegroundsDbSingleton.Instance.GetSpells(Core.Game.IsBattlegroundsDuosMatch));
			_availableCardIds = new HashSet<int>(availableCards.Select(card => card.DbfId));
		}
		return _availableCardIds;
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
				IsAvailable = !checkAvailability || availableCardIds!.Contains(card.DbfId)
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
			if(card == null || !availableCardIds.Contains(card.DbfId))
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
		var available = GetAvailableCardIds();
		var ids = new List<string>();

		foreach(var dbfId in CompGuide.CoreCards)
		{
			if(available != null && !available.Contains(dbfId))
				continue;
			var card = Database.GetCardFromDbfId(dbfId, false);
			if(card?.Id is string id && !string.IsNullOrEmpty(id) && card.IsKnownCard)
				ids.Add(id);
		}

		foreach(var dbfId in CompGuide.AddonCards)
		{
			if(available != null && !available.Contains(dbfId))
				continue;
			var card = Database.GetCardFromDbfId(dbfId, false);
			if(card?.Id is string id && !string.IsNullOrEmpty(id) && card.IsKnownCard)
				ids.Add(id);
		}

		foreach(var line in WhenToCommitTags)
		{
			foreach(var inline in line)
			{
				if(inline is ReferencedCardRun r && r.Card?.DbfId is int dbfId)
				{
					if(available != null && !available.Contains(dbfId))
						continue;
					var card = Database.GetCardFromDbfId(dbfId, false);
					if(card?.Id is string id && !string.IsNullOrEmpty(id) && card.IsKnownCard && !ids.Contains(id))
						ids.Add(id);
				}
			}
		}

		foreach(var line in CommonEnablerTags)
		{
			foreach(var inline in line)
			{
				if(inline is ReferencedCardRun r && r.Card?.DbfId is int dbfId)
				{
					if(available != null && !available.Contains(dbfId))
						continue;
					var card = Database.GetCardFromDbfId(dbfId, false);
					if(card?.Id is string id && !string.IsNullOrEmpty(id) && card.IsKnownCard && !ids.Contains(id))
						ids.Add(id);
				}
			}
		}

		if(HowToPlay != null)
		{
			foreach(var inline in HowToPlay)
			{
				if(inline is ReferencedCardRun r && r.Card?.DbfId is int dbfId)
				{
					if(available != null && !available.Contains(dbfId))
						continue;
					var card = Database.GetCardFromDbfId(dbfId, false);
					if(card?.Id is string id && !string.IsNullOrEmpty(id) && card.IsKnownCard && !ids.Contains(id))
						ids.Add(id);
				}
			}
		}

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

	public IEnumerable<Inline>? HowToPlay { get; }
}
