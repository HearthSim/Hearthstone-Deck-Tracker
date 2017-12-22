using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class RewardsEventArgs : System.EventArgs
	{
		public ArenaInfo Info { get; set; }

		public RewardsEventArgs(ArenaInfo info)
		{
			Info = info;
		}
	}
}
