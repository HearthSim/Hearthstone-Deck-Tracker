using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class TaskExtensions
	{
		public static void Forget(this Task task)
		{
		}

		public static bool IsCompletedSuccessfully(this Task task) => task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
	}
}
