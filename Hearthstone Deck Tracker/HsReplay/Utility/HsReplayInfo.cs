namespace Hearthstone_Deck_Tracker.HsReplay.Utility
{
	public class HsReplayInfo
	{
		public HsReplayInfo()
		{
			
		}

		public HsReplayInfo(string uploadId)
		{
			UploadId = uploadId;
		}

		public string UploadId { get; set; }

		public int UploadTries { get; set; }

		public bool Unsupported { get; set; }

		public bool Uploaded => !string.IsNullOrEmpty(UploadId);

		public string Url => $"https://hsreplay.net/uploads/upload/{UploadId}";

		public void UploadTry() => UploadTries++;
	}
}
