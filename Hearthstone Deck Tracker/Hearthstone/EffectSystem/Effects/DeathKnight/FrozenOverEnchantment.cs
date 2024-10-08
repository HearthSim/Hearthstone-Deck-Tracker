﻿using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DeathKnight;

public class FrozenOverEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Deathknight.FrozenOver_FrozenSolidEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Deathknight.FrozenOver;

	public FrozenOverEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectTarget EffectTarget => EffectTarget.Enemy;

	public override EffectDuration EffectDuration => EffectDuration.NextTurn;

	public override EffectTag EffectTag => EffectTag.CardLock;
}
