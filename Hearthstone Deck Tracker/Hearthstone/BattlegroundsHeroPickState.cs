using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public class BattlegroundsHeroPickState
{
	public int? PickedHeroDbfId { get; private set; }
	public int[]? OfferedHeroDbfIds { get; private set; }

	private GameV2 _game;

	public BattlegroundsHeroPickState(GameV2 game)
	{
		_game = game;
	}

	public int[] SnapshotOfferedHeroes(IEnumerable<Entity> heroes)
	{
		var offered = heroes.OrderBy(x => x.ZonePosition).Select(x => x.Card.DbfId).ToArray();
		OfferedHeroDbfIds = offered;
		return OfferedHeroDbfIds;
	}

	public int? SnapshotPickedHero()
	{
		PickedHeroDbfId = _game.Player.Hero?.Card.DbfId;
		return PickedHeroDbfId;
	}
}
