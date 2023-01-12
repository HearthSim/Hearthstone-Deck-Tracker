using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class FirstHSCollectionUploadAction : VMAction
	{
		public FirstHSCollectionUploadAction(int collectionSize) : base(
			Franchise.HSConstructed, null, null
		)
		{
			CollectionSize = collectionSize;
		}

		[JsonProperty("collection_size")]
		public int CollectionSize { get; }

		public override string Name => "Upload First Hearthstone Collection";
		public override ActionSource Source => ActionSource.App;
		public override string Type => "First Collection Upload";
	}
}
