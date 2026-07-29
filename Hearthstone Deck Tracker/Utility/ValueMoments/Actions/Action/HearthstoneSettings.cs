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

		[JsonProperty("mulligan_gv2_overlay")]
		public bool MulliganGV2Overlay { get => Config.Instance.EnableMulliganGV2; }

		[JsonProperty("mulligan_guide_overlay_auto_expand")]
		public bool MulliganGuideOverlayAutoExpand { get => Config.Instance.AutoShowMulliganGuide; }

		[JsonProperty("mulligan_guide_toast")]
		public bool MulliganGuideToast { get => Config.Instance.ShowMulliganToast; }

		[JsonProperty("outfinder")]
		public bool Outfinder { get => Config.Instance.OutfinderEnabled; }

		[JsonProperty("outfinder_in_deck")]
		public bool OutfinderInDeck { get => Config.Instance.OutfinderInDeck; }

		[JsonProperty("outfinder_in_hand")]
		public bool OutfinderInHand { get => Config.Instance.OutfinderInHand; }

		[JsonProperty("outfinder_use_percentages")]
		public bool OutfinderUsePercentages { get => Config.Instance.OutfinderUsePercentages; }

		[JsonProperty("outfinder_use_card_tiles")]
		public bool OutfinderUseCardTiles { get => Config.Instance.OutfinderUseCardTiles; }

		[JsonProperty("board_entry_order")]
		public bool BoardEntryOrder { get => Config.Instance.ShowBoardEntryOrder; }
	}
}
