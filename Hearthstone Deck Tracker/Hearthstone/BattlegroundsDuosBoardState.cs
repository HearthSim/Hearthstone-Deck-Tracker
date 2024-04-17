using System.Collections.Generic;
using HearthMirror.Objects;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public class BattlegroundsDuosBoardState
{
	public bool IsViewingTeammate { get;  }

	public List<string> MulliganHeroes { get; }

	public List<BattlegroundsTeammateBoardStateEntity> Entities { get; }

	public BattlegroundsDuosBoardState(
		bool isViewingTeammate,
		List<string> mulliganHeroes,
		List<BattlegroundsTeammateBoardStateEntity> entities
	)
	{
		IsViewingTeammate = isViewingTeammate;
		MulliganHeroes = mulliganHeroes;
		Entities = entities;
	}
}
