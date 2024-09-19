using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using HSReplay.Requests;

namespace Hearthstone_Deck_Tracker.Hearthstone;

internal class BattlegroundsTrinketPickState
{
	public int ChoiceId { get; private set; }
	public BattlegroundsTrinketPickParams Params { get; private set; }
	public int? ChosenTrinketDbfId { get; private set; }

	public BattlegroundsTrinketPickState(int choiceId, BattlegroundsTrinketPickParams parameters)
	{
		ChoiceId = choiceId;
		Params = parameters;
	}

	public void PickTrinket(Entity trinket)
	{
		ChosenTrinketDbfId = trinket.Card.DbfId;
	}
}
