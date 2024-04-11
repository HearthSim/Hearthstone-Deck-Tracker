using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone;

internal class MulliganState
{
	public List<Entity>? OfferedCards { get; private set; }
	public List<Entity>? KeptCards { get; private set; }
	public List<Entity>? FinalCardsInHand { get; private set; }

	private GameV2 _game;

	public MulliganState(GameV2 game)
	{
		_game = game;
	}

	public List<Entity> SnapshotMulligan()
	{
		OfferedCards = _game.Player.PlayerEntities.Where(x => x.IsInHand && !x.Info.Created).OrderBy(x => x.ZonePosition).ToList();
		return OfferedCards;
	}

	public List<Entity> SnapshotMulliganChoices(Choice choice)
	{
		KeptCards = choice.ChosenEntities.ToList();
		return KeptCards;
	}

	public List<Entity> SnapshotOpeningHand()
	{
		FinalCardsInHand = _game.Player.PlayerEntities.Where(x => x.IsInHand && !x.Info.Created).OrderBy(x => x.ZonePosition).ToList();
		return FinalCardsInHand;
	}
}
