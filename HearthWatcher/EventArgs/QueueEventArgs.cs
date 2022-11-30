using HearthMirror.Enums;

namespace HearthWatcher.EventArgs
{
	public class QueueEventArgs : System.EventArgs
	{
		public bool IsInQueue { get; set; }
		public FindGameState? Current { get; set; }
		public FindGameState? Previous { get; set; }

		public QueueEventArgs(bool isInQueue, FindGameState? current, FindGameState? previous)
		{
			IsInQueue = isInQueue;
			Current = current;
			Previous = previous;
		}
	}
}
