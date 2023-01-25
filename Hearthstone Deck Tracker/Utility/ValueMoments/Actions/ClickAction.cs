using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class ClickAction : VMAction
	{
		public enum Action
		{
			[JsonProperty("screenshot: Copy to Clipboard")]
			ScreenshotCopyToClipboard,
			[JsonProperty("screenshot: Save To Disk")]
			ScreenshotSaveToDisk,
			[JsonProperty("screenshot: Upload to Imgur")]
			ScreenshotUploadToImgur,

			[JsonProperty("stats: Arena")]
			StatsArena,
			[JsonProperty("stats: Constructed")]
			StatsConstructed,
		}

		public ClickAction(Franchise franchise, Action actionName) : this(franchise, actionName, null){ }

		public ClickAction(Franchise franchise, Action actionName, SubFranchise[]? subFranchise) : base(
			franchise, subFranchise, 10, true
		)
		{
			ActionName = actionName;
		}

		public override string Name => "Click Action HDT";
		public override ActionSource Source => ActionSource.MainWindow;
		public override string Type => "Click Action";

		[JsonProperty("action_name")]
		[JsonConverter(typeof(EnumJsonConverter))]
		public Action ActionName { get ; }
	}
}
