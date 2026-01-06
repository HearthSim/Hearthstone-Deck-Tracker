using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class ToastAction : VMAction
	{
		public const string ToastProperty = "toast";

		public enum Toast
		{
			[JsonProperty("mulligan")]
			Mulligan,
			[JsonProperty("constructed_collection_uploaded")]
			ConstructedCollectionUploaded,
			[JsonProperty("battlegrounds_hero_picker")]
			BattlegroundsHeroPicker,
			[JsonProperty("mercenaries_collection_uploaded")]
			MercenariesCollectionUploaded,
			[JsonProperty("battlegrounds_timewarp")]
			BattlegroundsTimewarp,
		}

		public ToastAction(Franchise franchise, Toast toastName) : base(
			franchise, null, null
		)
		{
			ToastName = toastName;
		}

		public override string Name => "Click HDT Toast";
		public override ActionSource Source => ActionSource.Overlay;
		public override string Type => "Toast Click";

		[JsonProperty(ToastProperty)]
		[JsonConverter(typeof(EnumJsonConverter))]
		public Toast ToastName { get; }
	}
}
