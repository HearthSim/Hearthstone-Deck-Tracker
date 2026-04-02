using System.Linq;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class BattlegroundsLobbyInfoArgs : System.EventArgs
	{
		public BattlegroundsLobbyInfo? LobbyInfo { get; }

		public BattlegroundsLobbyInfoArgs(BattlegroundsLobbyInfo? lobbyInfo)
		{
			LobbyInfo = lobbyInfo;
		}

		public override bool Equals(object obj)
		{
			if(obj is not BattlegroundsLobbyInfoArgs args)
				return false;
			if(LobbyInfo == null && args.LobbyInfo == null)
				return true;
			if(LobbyInfo == null || args.LobbyInfo == null)
				return false;
			return LobbyInfo.GameUuid == args.LobbyInfo.GameUuid
				&& LobbyInfo.Players.Count == args.LobbyInfo.Players.Count
				&& LobbyInfo.Players.Select(p => p.HeroCardId).SequenceEqual(args.LobbyInfo.Players.Select(p => p.HeroCardId));
		}
	}
}
