using System.Windows.Forms;
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

		public string Url
		{
			get
			{
				if(string.IsNullOrEmpty(ReplayUrl))
					return Helper.BuildHsReplayNetUrl($"/uploads/upload/{UploadId}", "replay");
				return ReplayUrl + Helper.GetHsReplayNetUrlParams("replay");
			}
		}

		public string ReplayUrl { get; set; }

		public void UploadTry() => UploadTries++;
	}
}
