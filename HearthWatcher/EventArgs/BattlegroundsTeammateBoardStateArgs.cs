using System.Collections.Generic;
using System.Linq;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class BattlegroundsTeammateBoardStateArgs : System.EventArgs
	{
		public bool IsViewingTeammate { get; }

		public List<string> MulliganHeroes { get; }

		public List<BattlegroundsTeammateBoardStateEntity> Entities { get; }

		public BattlegroundsTeammateBoardStateArgs(
			bool isViewingTeammate, List<string> mulliganHeroes, List<BattlegroundsTeammateBoardStateEntity> entities
		)
		{
			IsViewingTeammate = isViewingTeammate;
			MulliganHeroes = mulliganHeroes;
			Entities = entities;
		}

		public override bool Equals(object obj)
		{
			return obj is BattlegroundsTeammateBoardStateArgs args
			       && IsViewingTeammate == args.IsViewingTeammate
			       && MulliganHeroes.SequenceEqual(args.MulliganHeroes)
			       && Entities.SequenceEqual(args.Entities);
		}
	}
}
