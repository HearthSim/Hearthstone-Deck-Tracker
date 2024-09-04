using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class IllidariStudiesEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.IllidariStudies_LonerEnchantmentDARKMOON_FAIRE2;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.IllidariStudies;

	public IllidariStudiesEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;

	public override EffectTag EffectTag => EffectTag.CostModification;
}
