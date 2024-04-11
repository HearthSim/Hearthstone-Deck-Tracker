using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone;

internal class BattlegroundsHeroPickState
{
	public Entity? PickedHero { get; private set; }

	private GameV2 _game;

	public BattlegroundsHeroPickState(GameV2 game)
	{
		_game = game;
	}

	public Entity? SnapshotPickedHero()
	{
		PickedHero = _game.Player.Hero;
		return PickedHero;
	}
}
