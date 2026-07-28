namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a spell. Reduce its Cost by (2)."
public class PrimordialGlyph : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.PrimordialGlyph;
}

public class PrimordialGlyphCorePlaceholder : PrimordialGlyph
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.PrimordialGlyphCorePlaceholder;
}
