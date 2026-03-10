using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DeathKnight;

public class AlexandrosMograineEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Deathknight.AlexandrosMograine_MograinesMigraineEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Deathknight.AlexandrosMograine;

	public AlexandrosMograineEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
