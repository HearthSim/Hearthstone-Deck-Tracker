using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	public class ReleaseUrls
	{
		private const string FieldLive = "live";
		private const string FieldHsReplay = "hsreplay";

		[JsonProperty(FieldLive)]
		public string Live { get; set; }

		[JsonProperty(FieldHsReplay)]
		public string HsReplay { get; set; }

		public string GetReleaseUrl(string release)
		{
			switch(release)
			{
				case FieldHsReplay:
					return !string.IsNullOrEmpty(HsReplay) ? HsReplay : Live;
				case FieldLive:
					return Live;
				default:
					return Live;
			}
		}
	}
}