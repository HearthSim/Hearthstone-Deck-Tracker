using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class CamouflagedDirigibleEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Rogue.CamouflagedDirigible_CamouflagedEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.CamouflagedDirigible;

	public CamouflagedDirigibleEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
