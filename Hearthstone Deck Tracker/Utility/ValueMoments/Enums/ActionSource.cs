using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum ActionSource
	{
		[JsonProperty("app")]
		App,

		[JsonProperty("mainWindow")]
		MainWindow,

		[JsonProperty("overlay")]
		Overlay,
	}
}
