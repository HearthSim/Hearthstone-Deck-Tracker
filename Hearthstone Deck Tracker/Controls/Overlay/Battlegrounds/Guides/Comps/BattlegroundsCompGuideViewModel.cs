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

public class ReferencedCardRun : Run
{
	public Hearthstone.Card? Card { get; }
	public CardAssetType AssetType { get; }

	private static Dictionary<int, HearthDb.Card> CardsByDbfId { get; }

	static ReferencedCardRun()
	{
		var heroes = Cards.All.Values
			.Where(x => x.Type == CardType.BATTLEGROUND_SPELL || x.Type is CardType.HERO && x.Entity.Tags.Any(t => t.EnumId == (int)GameTag.BACON_HERO_CAN_BE_DRAFTED));
		CardsByDbfId = Cards.BaconPoolMinions.Values.Concat(heroes).ToDictionary(x => x.DbfId, x => x);
	}

	static string ResolveCardNameOrFallback(int? dbfId, string fallback)
	{
		if(dbfId is int theDbfId && CardsByDbfId.TryGetValue(theDbfId, out var card))
		{
			if(!Enum.TryParse(Helper.GetCardLanguage(), out Locale lang))
				lang = Locale.enUS;
			return card.GetLocName(lang);
		}

		return fallback;
	}

	public ReferencedCardRun(int? dbfId, string fallback) : base(ResolveCardNameOrFallback(dbfId, fallback))
	{
		DataContext = this; // required for tooltip to work

		if(dbfId is int theDbfId && CardsByDbfId.TryGetValue(theDbfId, out var card))
		{
			Card = new Hearthstone.Card(card)
			{
				BaconCard = true
			};
			AssetType = Card.TypeEnum == CardType.HERO ? CardAssetType.Hero : CardAssetType.FullImage;
		}
	}
}

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

	private IEnumerable<Inline> ParseCardsFromText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			yield break;
		}

		var index = 0;

		while (index < text.Length)
		{
			var nextIndex = text.IndexOf("[[", index);
			// If there is no opening tag anymore, return the remaining text
			if (nextIndex == -1)
			{
				yield return new Run(text.Substring(index));
				yield break;
			}

			// If there is text before the next tag, return it
			if (nextIndex > index)
			{
				yield return new Run(text.Substring(index, nextIndex - index));
			}

			index = nextIndex + 2;


			var endIndex = text.IndexOf("]]", index);

			// Edge case: if there is no closing tag, just return the remaining text
			if (endIndex == -1)
			{
				yield return new Run(text.Substring(nextIndex));
				yield break;
			}

			// Check if there's a dbf id before the end
			var separatorIndex = text.IndexOf("||", index);

			// Discard any separators past the end of this card
			if(separatorIndex > endIndex)
			{
				separatorIndex = -1;
			}

			// Parse the dbf id
			var loaded = separatorIndex != -1 ? text.Substring(separatorIndex + 2, endIndex - separatorIndex - 2) : "";
			var dbfId = separatorIndex != -1
				? int.TryParse(loaded, out var theDbfId)
					? theDbfId
					: null
				: (int?)null;

			var cardNameTerminator = separatorIndex != -1 ? separatorIndex : endIndex;
			var length = cardNameTerminator - index;
			yield return new ReferencedCardRun(dbfId, text.Substring(index, length));

			index = endIndex + 2;
		}
	}

	public BattlegroundsCompGuideViewModel(BattlegroundsCompGuide compGuide)
	{
		CompGuide = compGuide;

		CoreCardId = CompGuide.CoreCards.FirstOrDefault();
		CardToShowInUi = Database.GetCardFromDbfId(CoreCardId, false);
		CardAsset = new(CardToShowInUi, Utility.Assets.CardAssetType.Portrait);
		DifficultyText = CompGuide.Difficulty switch
		{
			1 => "Easy",
			2 => "Medium",
			3 => "Hard",
			_ => "Unknown"
		};
		DifficultyColor = CompGuide.Difficulty switch
		{
			1 => "#49634b",
			2 => "#917b43",
			3 => "#7f303e",
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

		CommonEnablerTags = compGuide.CommonEnablers
			.Replace("\r\n", "\n")
			.Replace("\r", "\n")
			.Split('\n')
			.Select(ParseCardsFromText)
			.ToArray();

		WhenToCommitTags = compGuide.WhenToCommit
			.Replace("\r\n", "\n")
			.Replace("\r", "\n")
			.Split('\n')
			.Select(ParseCardsFromText)
			.ToArray();

		HowToPlay = ParseCardsFromText(compGuide.HowToPlay);
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

	public IEnumerable<Inline> HowToPlay { get; }
}
