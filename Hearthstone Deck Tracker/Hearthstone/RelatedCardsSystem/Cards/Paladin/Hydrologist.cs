using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Battlecry: Discover and cast a Secret."
// The Secret enters play, so ICardGenerator
// lets SecretsManager narrow the opponent's possible Secrets.
public class Hydrologist : ClassOrNeutralSecretPool, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Hydrologist;

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
