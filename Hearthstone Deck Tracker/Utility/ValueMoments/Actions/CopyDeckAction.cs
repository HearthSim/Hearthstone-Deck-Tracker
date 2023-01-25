using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class CopyDeckAction : VMAction
	{
		public enum Action
		{
			[JsonProperty("Copy All")]
			CopyAll,
			[JsonProperty("Copy Code")]
			CopyCode,
			[JsonProperty("Copy Ids to Clipboard")]
			CopyIds,
			[JsonProperty("Copy Names to Clipboard")]
			CopyNames,
			[JsonProperty("Save as XML")]
			SaveAsXML,
		}

		public CopyDeckAction(Franchise franchise, Action actionName) : base(
			franchise, null, 10, true
		)
		{
			ActionName = actionName;
		}

		public override string Name => "Copy Deck HDT";
		public override ActionSource Source => ActionSource.MainWindow;
		public override string Type => "Copy Deck";

		[JsonProperty("action_name")]
		[JsonConverter(typeof(EnumJsonConverter))]
		public Action ActionName { get; }
	}
}
