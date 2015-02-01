namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class LoginResult
	{
		public LoginResult(bool success, string message = "")
		{
			Success = success;
			Message = message;
		}

		public bool Success { get; private set; }
		public string Message { get; private set; }
	}
}