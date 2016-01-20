namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class PostResult
	{
		public bool Retry;
		public bool Success;

		private PostResult(bool success, bool retry)
		{
			Success = success;
			Retry = retry;
		}

		public static PostResult WasSuccess => new PostResult(true, false);

		public static PostResult Failed => new PostResult(false, false);

		public static PostResult CanRetry => new PostResult(false, true);
	}
}