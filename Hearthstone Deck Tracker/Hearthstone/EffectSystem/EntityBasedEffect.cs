using System;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Microsoft.Build.Framework;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem;

public abstract class EntityBasedEffect : IShowInGlobalEffectList
{
	public int EntityId { get; }

	public bool IsControlledByPlayer { get; }
	public virtual string? CardId { get; }

	protected virtual string? CardIdToShowInUI { get; }
	public Card? CardToShowInUI => Database.GetCardFromId(CardIdToShowInUI ?? CardId);

	public CardAssetViewModel CardAsset => new(CardToShowInUI, Utility.Assets.CardAssetType.Portrait);

	public virtual bool ShowNumberInPlay => true;
	public virtual EffectTarget EffectTarget => EffectTarget.Self;

	[Required]
	public abstract EffectDuration EffectDuration { get; }
	[Required]
	public abstract EffectTag EffectTag { get; }

	protected EntityBasedEffect(int entityId, bool isControlledByPlayer)
	{
		EntityId = entityId;
		IsControlledByPlayer = isControlledByPlayer;
	}
}
