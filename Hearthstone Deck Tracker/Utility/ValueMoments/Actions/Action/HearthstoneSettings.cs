using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class HearthstoneSettings
	{

		[JsonProperty("hide_decks")]
		public bool HideDecks { get => Config.Instance.HideDecksInOverlay; }

		[JsonProperty("hide_timers")]
		public bool HideTimers { get => Config.Instance.HideTimers; }
	}
}
