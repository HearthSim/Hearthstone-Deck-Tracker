using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class CopyDeckAction : VMAction
	{
		public const string Name = "Copy Deck HDT";

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
			Name, ActionSource.MainWindow, "Copy Deck", franchise, null, 10, true
		)
		{
			ActionName = actionName;
		}

		[JsonProperty(ValueMomentsConstants.ActionNameProperty)]
		[JsonConverter(typeof(EnumJsonConverter))]
		public Action ActionName { get; }
	}
}
