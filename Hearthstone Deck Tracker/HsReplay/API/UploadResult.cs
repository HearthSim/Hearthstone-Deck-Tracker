namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class UploadResult
	{
		public static UploadResult Failed => new UploadResult();
		public static UploadResult Successful(string replayId) => new UploadResult(replayId);

		private UploadResult()
		{
			Success = false;
		}

		private UploadResult(string replayId)
		{
			ReplayId = replayId;
			Success = true;
		}

		public bool Success { get; }
		public string ReplayId { get; }
	}
}