using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

// "Battlecry: If 5 friendly minions died this turn, your Rush minions have Charge this game."
public class BroodqueenShakarEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Demonhunter.BroodqueenShakar_ShakarsRageEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.BroodqueenShakar;

	public BroodqueenShakarEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
