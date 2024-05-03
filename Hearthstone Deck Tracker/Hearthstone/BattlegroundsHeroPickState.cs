using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone;

internal class BattlegroundsHeroPickState
{
	public int? PickedHeroDbfId { get; private set; }

	private GameV2 _game;

	public BattlegroundsHeroPickState(GameV2 game)
	{
		_game = game;
	}

	public int? SnapshotPickedHero()
	{
		PickedHeroDbfId = _game.Player.Hero?.Card.DbfId;
		return PickedHeroDbfId;
	}
}
