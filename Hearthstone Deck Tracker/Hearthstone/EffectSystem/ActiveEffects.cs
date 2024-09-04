using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Factory;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem;

public class ActiveEffects
{
	private readonly List<EntityBasedEffect> _playerEffects = new();
	private readonly List<EntityBasedEffect> _opponentEffects = new();
	private readonly EffectFactory _effectFactory = new();

	private List<EntityBasedEffect> GetTargetEffectsList(EntityBasedEffect effect, bool controlledByPlayer) =>
		(controlledByPlayer && effect.EffectTarget == EffectTarget.Self) || (!controlledByPlayer && effect.EffectTarget == EffectTarget.Enemy) ? _playerEffects : _opponentEffects;

	public void TryAddEffect(Entity sourceEntity, bool controlledByPlayer)
	{
		var effect = _effectFactory.CreateFromEntity(sourceEntity, controlledByPlayer);

		if(effect == null) return;

		// TODO: Make sure only 1 enchant is added for both players
		if(effect.EffectTarget == EffectTarget.Both)
		{
			_playerEffects.Add(effect);
			_opponentEffects.Add(effect);
			NotifyEffectsChanged();
			return;
		}

		var effects = GetTargetEffectsList(effect, controlledByPlayer);

		effects.Add(effect);

		NotifyEffectsChanged();
	}

	public void TryRemoveEffect(Entity sourceEntity,  bool controlledByPlayer)
	{
		var sampleEffect = _effectFactory.CreateFromEntity(sourceEntity, controlledByPlayer);

		if (sampleEffect == null) return;

		// TODO: Make sure only 1 enchant is added for both players
		if(sampleEffect.EffectTarget == EffectTarget.Both)
		{
			_playerEffects.RemoveAll(e => e.EntityId == sourceEntity.Id);
			_opponentEffects.RemoveAll(e => e.EntityId == sourceEntity.Id);
			NotifyEffectsChanged();
			return;
		}

		var effects = GetTargetEffectsList(sampleEffect, controlledByPlayer);

		var effect = effects.FirstOrDefault(e => e.EntityId == sourceEntity.Id);

		if(effect == null) return;

		effects.Remove(effect);
		NotifyEffectsChanged();
	}

	public List<EntityBasedEffect> GetVisibleEffects(bool controlledByPlayer)
	{
		var effects = controlledByPlayer ? _playerEffects : _opponentEffects;

		return effects.Where(e => e.EffectDuration != EffectDuration.MultipleTurns).ToList();
	}

	public void Reset()
	{
		_playerEffects.Clear();
		_opponentEffects.Clear();
		NotifyEffectsChanged();
	}

	public event EventHandler? EffectsChanged;

	private void NotifyEffectsChanged()
	{
		EffectsChanged?.Invoke(this, EventArgs.Empty);
	}
}
