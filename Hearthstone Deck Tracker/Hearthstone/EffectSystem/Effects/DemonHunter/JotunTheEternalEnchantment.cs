using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class JotunTheEternalEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Demonhunter.JotuntheEternal_JotunsSwiftnessToken;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.JotunTheEternal;

	public JotunTheEternalEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
