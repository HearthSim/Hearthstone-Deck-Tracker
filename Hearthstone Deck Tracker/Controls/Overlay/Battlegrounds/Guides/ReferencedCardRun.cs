using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public class ReferencedCardRun : Run, ICardTooltip
{
	public Hearthstone.Card? Card { get; }
	public CardAssetType AssetType { get; }

	private static Dictionary<int, HearthDb.Card> _cardsByDbfId = new();

	static ReferencedCardRun()
	{
		UpdateCards();
		CardDefsManager.CardsChanged += UpdateCards;
	}

	private static void UpdateCards()
	{
		var heroes = Cards.All.Values
			.Where(x => x.Type == CardType.BATTLEGROUND_SPELL || x.Type is CardType.HERO && x.Entity.Tags.Any(t => t.EnumId == (int)GameTag.BACON_HERO_CAN_BE_DRAFTED));
		_cardsByDbfId = Cards.BaconPoolMinions.Values.Concat(heroes).ToDictionary(x => x.DbfId, x => x);
	}

	static string ResolveCardNameOrFallback(int? dbfId, string fallback)
	{
		if(dbfId is int theDbfId && _cardsByDbfId.TryGetValue(theDbfId, out var card))
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

		if(dbfId is int theDbfId && _cardsByDbfId.TryGetValue(theDbfId, out var card))
		{
			Card = new Hearthstone.Card(card)
			{
				BaconCard = true
			};
			AssetType = Card.TypeEnum == CardType.HERO ? CardAssetType.Hero : CardAssetType.FullImage;
		}
	}

	public static IEnumerable<Inline>[] ParseCardsFromText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return Array.Empty<IEnumerable<Inline>>();
		}

		var result = new List<IEnumerable<Inline>>();
		var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		foreach (var line in lines)
		{
			var inlines = new List<Inline>();
			var index = 0;
			while (index < line.Length)
			{
				var nextIndex = line.IndexOf("[[", index);
				if (nextIndex == -1)
				{
					inlines.Add(new Run(line.Substring(index)));
					break;
				}

				if (nextIndex > index)
				{
					inlines.Add(new Run(line.Substring(index, nextIndex - index)));
				}

				index = nextIndex + 2;
				var endIndex = line.IndexOf("]]", index);
				if (endIndex == -1)
				{
					inlines.Add(new Run(line.Substring(nextIndex)));
					break;
				}

				var separatorIndex = line.IndexOf("||", index);
				if (separatorIndex > endIndex)
				{
					separatorIndex = -1;
				}

				var loaded = separatorIndex != -1 ? line.Substring(separatorIndex + 2, endIndex - separatorIndex - 2) : "";
				var dbfId = separatorIndex != -1
					? int.TryParse(loaded, out var theDbfId)
						? theDbfId
						: null
					: (int?)null;

				var cardNameTerminator = separatorIndex != -1 ? separatorIndex : endIndex;
				var length = cardNameTerminator - index;
				inlines.Add(new ReferencedCardRun(dbfId, line.Substring(index, length)));

				index = endIndex + 2;
			}
			result.Add(inlines);
		}

		return result.ToArray();
	}

	public void UpdateTooltip(CardTooltipViewModel viewModel)
	{
		viewModel.Card = Card;
		viewModel.CardAssetType = AssetType;
		viewModel.ShowTriple = true;
	}
}
