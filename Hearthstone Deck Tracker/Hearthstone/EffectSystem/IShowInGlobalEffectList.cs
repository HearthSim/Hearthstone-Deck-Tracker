using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem;

public interface IShowInGlobalEffectList
{
	public bool ShowNumberInPlay { get; }
	public EffectTarget EffectTarget { get; }
}
