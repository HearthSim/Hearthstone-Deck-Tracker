using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using NuGet;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public class ArenaPickSingleCardOptionViewModel : ViewModel
{
	public ArenaCardPickApiResponse.CardStatsEntry? CardStats { get; }

	public List<string>? PickedDeck
	{
		get => GetProp<List<string>?>(null);
		set => SetProp(value);
	}

	public bool IsUnderground { get; }

	public bool HasInfo => CardStats != null &&
	                       !CardStats.Messages.Where(m => m.Type is
		                       MessageType.LowSynergy or
			                   MessageType.Highlander or
			                   MessageType.SoftHighlander or
			                   MessageType.HighlanderChances or
			                   MessageType.QuestHelps or
			                   MessageType.VeryRare
		                   ).IsEmpty();

	public ArenaPickSingleCardOptionViewModel(string cardId, ArenaCardPickApiResponse.CardStatsEntry data, List<string>? pickedDeck, bool isUnderground, bool apiError = false)
	{
		Card = new Hearthstone.Card(cardId);
		CardStats = data;
		PickedDeck = pickedDeck;
		IsUnderground = isUnderground;


		var score = "-";
		if(CardStats.ArenasmithDyn?.Score != null)
			score = FormatArenasmithScoreString(CardStats.ArenasmithDyn?.Score);
		else if(CardStats.Arenasmith?.Score != null)
			score = FormatArenasmithScoreString(CardStats.Arenasmith?.Score);

		var level = 0;
		if(CardStats.ArenasmithDyn?.Plaque != null)
			level = CardStats.ArenasmithDyn?.Plaque ?? 0;
		else if(CardStats.Arenasmith?.Plaque != null)
			level = CardStats.Arenasmith?.Plaque ?? 0;

		PlaqueViewModel = new ArenaPlaqueViewModel(score, level, cardId.GetHashCode(), isUnderground);

		if(score == "-")
		{
			Influx.OnArenasmithMissingScore(apiError, cardId);
		}
	}

	private static string FormatArenasmithScoreString(string? score)
	{
		if(string.IsNullOrWhiteSpace(score)) return "-";

		var intScore = score!.Split('.').First();
		var decimalScore = score!.Split('.').Last();

		return int.Parse(intScore) >= 10 || decimalScore == "0" ? intScore : score;
	}

	public ArenaPickSingleCardOptionViewModel(string cardId, bool isUnderground)
	{
		Card = new Hearthstone.Card(cardId);
		IsUnderground = isUnderground;
		PlaqueViewModel = new ArenaPlaqueViewModel("", 0, cardId.GetHashCode(), isUnderground);
	}

	public Hearthstone.Card Card { get; }
	public bool IsMultiTribe => Card.SecondaryRaceEnum != null && Card.SecondaryRaceEnum != Race.INVALID;

	private static readonly SolidColorBrush NormalBorderColor = Helper.BrushFromHex("#067F93")!;
	private static readonly SolidColorBrush UndergroundBorderColor = Helper.BrushFromHex("#932020")!;
	public SolidColorBrush BadgeBorderColor => IsUnderground ? UndergroundBorderColor : NormalBorderColor;

	private static readonly SolidColorBrush NormalForegroundColor = Helper.BrushFromHex("#168Fa3")!;

	// This is intentionally different from ArenaPickSingleHeroOption!
	// The icons here are more solid than the stats and text below the hero, and need a darker red.
	private static readonly SolidColorBrush UndergroundForegroundColor = Helper.BrushFromHex("#C72E2E")!;

	public SolidColorBrush BadgeForegroundColor => IsUnderground ? UndergroundForegroundColor : NormalForegroundColor;

	public bool HasRelatedCards => CardStats?.RelatedCards.GeneratedCardIds?.Generated.Any() ?? false;

	public List<string> EnabledCardsIds
	{
		get
		{
			var direct = CardStats?.RelatedCards.CardIdsEnabled?.Direct ?? new List<string>();
			var indirect = CardStats?.RelatedCards.CardIdsEnabled?.Indirect ?? new List<string>();
			return direct.Concat(indirect).ToList();
		}
	}

	public List<string> EnhancedByCardsIds
	{
		get
		{
			var direct = CardStats?.RelatedCards.EnhancedByCardIds?.Direct ?? new List<string>();
			var indirect = CardStats?.RelatedCards.EnhancedByCardIds?.Indirect ?? new List<string>();
			return direct.Concat(indirect).ToList();
		}
	}

	public bool ShowSynergy => SynergyCount > 0;

	public int SynergyCount => EnabledCardsIds.Count +
	                           EnhancedByCardsIds.Where(x => !(EnabledCardsIds?.Contains(x) ?? false)).Sum(x => PickedDeck?.Count(p => p == x) ?? 0);

	// @todo: add this back
	private string? GetIcon()
	{
		string? icon = null;

		if(CardStats?.ArenasmithDyn?.Caution != null)
		{
			icon = CardStats.ArenasmithDyn?.Caution switch
			{
				"Red" => "/Images/caution-red.png",
				"Yellow" => "/Images/caution-yellow.png",
				_ => null
			};
		}

		return icon;
	}

	public ArenaPlaqueViewModel PlaqueViewModel { get; }

	public bool HighlightImprovements
	{
		get => GetProp(false);
		set => SetProp(value);
	}

}
