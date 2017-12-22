using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	public class ReleaseUrls
	{
		private const string FieldDev = "dev";
		private const string FieldLive = "live";
		private const string FieldLiveChina = "live-china";

		[JsonProperty(FieldDev)]
		public string Dev { get; set; }

		[JsonProperty(FieldLive)]
		public string Live { get; set; }

		[JsonProperty(FieldLiveChina)]
		public string LiveChina { get; set; }

		public string GetReleaseUrl(string release)
		{
			switch(release)
			{
				case FieldDev:
					return Dev;
				case FieldLive:
					return Live;
				case FieldLiveChina:
					return !string.IsNullOrEmpty(LiveChina) ? LiveChina : Live;
				default:
					return Live;
			}
		}
	}
}
