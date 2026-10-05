using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Shaman;

// "Battlecry: Your next spell of each school casts twice."
public class TherazaneTheEarthmotherEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.TherazanetheEarthmother_SpellsOfASchoolsCastTwiceEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Shaman.TherazaneTheEarthmother;

	public TherazaneTheEarthmotherEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
