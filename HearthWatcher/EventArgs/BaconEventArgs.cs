using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class BaconEventArgs : System.EventArgs
	{
		public SelectedBattlegroundsGameMode SelectedBattlegroundsGameMode { get; }

		public BaconEventArgs(
			SelectedBattlegroundsGameMode selectedBattlegroundsGameMode
		)
		{
			SelectedBattlegroundsGameMode = selectedBattlegroundsGameMode;
		}

		public override bool Equals(object obj) => obj is BaconEventArgs args
			&& SelectedBattlegroundsGameMode == args.SelectedBattlegroundsGameMode;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + SelectedBattlegroundsGameMode.GetHashCode();
			return hashCode;
		}
	}
}
