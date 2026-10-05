using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

// "Draw a card and reduce its Cost by (2). (The next Shadow spell you play repeats this!)"
public class ElderWordForesightEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Priest.ElderWordForesight_ForesightEssenceEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.ElderWordForesight;

	public ElderWordForesightEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
