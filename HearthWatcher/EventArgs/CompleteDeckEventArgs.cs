using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class CompleteDeckEventArgs : System.EventArgs
	{
		public ArenaInfo Info { get; set; }

		public CompleteDeckEventArgs(ArenaInfo info)
		{
			Info = info;
		}
	}
}
