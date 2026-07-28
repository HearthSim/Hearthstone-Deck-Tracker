using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Twinspell Cast a random Paladin Secret."
// The Secret enters play, so ICardGenerator lets SecretsManager narrow the opponent's
// possible Secrets.
public class DesperateMeasures : DiscoverPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.DesperateMeasures;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.IsClass("Paladin") && c.HasTag(GameTag.SECRET))
			.Select(c => new Card(c));
	}

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.GetTag(GameTag.SECRET) > 0 &&
		       card.IsClass("Paladin") &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}

// Twinspell copy - reproduces the same "Cast a random Paladin Secret" effect
public class DesperateMeasuresToken : DesperateMeasures
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Paladin.DesperateMeasures_DesperateMeasuresToken;
}
