namespace HearthWatcher.EventArgs
{
	public class BattlegroundsLeaderboardArgs : System.EventArgs
	{
		public int? HoveredEntityId { get; }

		public BattlegroundsLeaderboardArgs(
			int? hoveredEntityId
		)
		{
			HoveredEntityId = hoveredEntityId;
		}

		public override bool Equals(object obj)
		{
			return obj is BattlegroundsLeaderboardArgs args
			       && HoveredEntityId == args.HoveredEntityId;
		}
	}
}
