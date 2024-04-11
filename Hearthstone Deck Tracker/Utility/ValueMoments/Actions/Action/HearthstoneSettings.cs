using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class HearthstoneSettings
	{

		[JsonProperty("hide_decks")]
		public bool HideDecks { get => Config.Instance.HideDecksInOverlay; }

		[JsonProperty("hide_timers")]
		public bool HideTimers { get => Config.Instance.HideTimers; }

		[JsonProperty("mulligan_guide_overlay")]
		public bool MulliganGuideOverlay { get => Config.Instance.EnableMulliganGuide; }

		[JsonProperty("mulligan_guide_overlay_auto_expand")]
		public bool MulliganGuideOverlayAutoExpand { get => Config.Instance.AutoShowMulliganGuide; }

		[JsonProperty("mulligan_guide_toast")]
		public bool MulliganGuideToast { get => Config.Instance.ShowMulliganToast; }
	}
}
