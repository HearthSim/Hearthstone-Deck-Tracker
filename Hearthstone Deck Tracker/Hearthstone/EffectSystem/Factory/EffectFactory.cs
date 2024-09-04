using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Factory;

public class EffectFactory : DynamicFactory<EntityBasedEffect>
{
	public EntityBasedEffect? CreateFromEntity(Entity entity, bool controlledByPlayer)
	{
		if(entity.CardId != null && Constructors.TryGetValue(entity.CardId, out var ctor))
			return ctor(entity.Id, controlledByPlayer);

		return null;
	}
}
