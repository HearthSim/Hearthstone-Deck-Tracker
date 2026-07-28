using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a Secret. Put it into the battlefield."
// The Secret enters play, so ICardGenerator
// lets SecretsManager narrow the opponent's possible Secrets.
public class ArcaneKeysmith : ClassOrNeutralSecretPool, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ArcaneKeysmith;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.GetTag(GameTag.SECRET) > 0 &&
		       card.IsClass("Mage") &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
