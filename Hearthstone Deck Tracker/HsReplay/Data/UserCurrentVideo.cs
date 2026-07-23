
namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	public class UserCurrentVideo
	{
		public UserCurrentVideo(string url, string language)
		{
			Url = url;
			Language = language;
		}

		public string Language { get; set; }

		public string Url { get; set; }
	}
}
