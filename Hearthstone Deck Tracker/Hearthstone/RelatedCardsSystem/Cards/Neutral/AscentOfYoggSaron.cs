using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Transform spells in your hand into ones that cost (3) more. (They keep their original Cost.)"
// Mastermind Yogg-Saron's ability.
public class AscentOfYoggSaron : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_AscentOfYoggSaronToken;
	protected override int CostOffset => 3;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";
}
