using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum ClientSettings
	{
		[MixpanelProperty("is_authenticated")]
		IsAuthenticated,

		[MixpanelProperty("screen_height")]
		ScreenHeight,

		[MixpanelProperty("screen_width")]
		ScreenWidth,

		[MixpanelProperty("card_language")]
		CardLanguage,

		[MixpanelProperty("appearance_language")]
		AppearanceLanguage,

		[MixpanelProperty("hdt_plugins")]
		HDTPlugins,
	}
}
