using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using WPFLocalizeExtension.Engine;

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
		CardToShowInUi = Database.GetCardFromDbfId(CoreCardId, false);
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

		CommonEnablerTags = ReferencedCardRun.ParseCardsFromText(compGuide.CommonEnablers);
		WhenToCommitTags = ReferencedCardRun.ParseCardsFromText(compGuide.WhenToCommit);
		HowToPlay = ReferencedCardRun.ParseCardsFromText(compGuide.HowToPlay).FirstOrDefault();
	}

	[LocalizedProp]
	public string LastUpdatedFormatted => LocUtil.GetAge(CompGuide.LastUpdated);

	public int CoreCardId { get; }

	public  IEnumerable<Inline>[] CommonEnablerTags { get; }
	public IEnumerable<Inline>[] WhenToCommitTags { get; }

	public string TierText { get; }
	public LinearGradientBrush TierColor { get; }
	public Hearthstone.Card? CardToShowInUi { get; }

	public string DifficultyText { get; }
	public string DifficultyColor { get; }

	public CardAssetViewModel CardAsset { get; }

	public IEnumerable<BattlegroundsMinionViewModel> CoreCards
	{
		get
		{
			return CompGuide.CoreCards.Select(cardId =>
				{
					var card = Database.GetCardFromDbfId(cardId, false);
					if (card == null)
						return null;

					card.BaconCard = true;
					return card;
				}
			).WhereNotNull().Where(x => x.Id != Database.UnknownCardId).Select(card =>
				new BattlegroundsMinionViewModel
				{
					Attack = card.Attack,
					Health = card.Health,
					Tier = card.TechLevel,
					Card = card
				});
		}
	}

	public ICommand ShowExampleBoardsCommand => new Command(() =>
	{
		var cards = CompGuide.CoreCards.Select(cardId =>
		{
			var card = Database.GetCardFromDbfId(cardId, false);
			if(card == null)
				return null;

			card.BaconCard = true;
			return card;
		}).WhereNotNull().ToArray();
		Core.Overlay.BattlegroundsInspirationViewModel.SetKeyMinion(CompGuide.Name, cards);
		Core.Overlay.ShowBgsInspiration();
		Core.Game.Metrics.BattlegroundsCompGuidesInspirationClicks++;
	});

	public bool ExampleBoardsButtonDisabled => (HSReplayNetOAuth.AccountData?.IsTier7 ?? false)
	                                           || Tier7Trial.IsTrialForCurrentGameActive(Core.Game.MetaData.ServerInfo
		                                           ?.GameHandle);

	public IEnumerable<BattlegroundsMinionViewModel> AddonCards
	{
		get
		{
			return CompGuide.AddonCards.Select(cardId =>
				{
					var card = Database.GetCardFromDbfId(cardId, false);
					if (card == null)
						return null;

					card.BaconCard = true;
					return card;
				}
			).WhereNotNull().Where(x => x.Id != Database.UnknownCardId).Select(card =>
				new BattlegroundsMinionViewModel
				{
					Attack = card.Attack,
					Health = card.Health,
					Tier = card.TechLevel,
					Card = card
				});
		}
	}

	public IEnumerable<Inline>? HowToPlay { get; }
}
