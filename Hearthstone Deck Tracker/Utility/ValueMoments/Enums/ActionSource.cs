using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum ActionSource
	{
		[MixpanelProperty("app")]
		App,

		[MixpanelProperty("mainWindow")]
		MainWindow,

		[MixpanelProperty("overlay")]
		Overlay,
	}
}
