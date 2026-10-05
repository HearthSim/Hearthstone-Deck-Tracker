using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

// "Tome Translator (deciphered): Battlecry: The next spell you cast this turn costs (4) less."
public class TomeTranslatorEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Druid.TomeTranslator_YoggScriptureEnchantmentToken;
	protected override string CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.TomeTranslator_TomeTranslatorToken2;

	public TomeTranslatorEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
