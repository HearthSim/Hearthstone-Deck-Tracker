using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

// "Summon a 5/5 Voidwing with Lifesteal. (The next Shadow spell you play repeats this!)"
public class ElderWordManifestEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Priest.ElderWordManifest_ManifestEssenceEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.ElderWordManifest;

	public ElderWordManifestEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
