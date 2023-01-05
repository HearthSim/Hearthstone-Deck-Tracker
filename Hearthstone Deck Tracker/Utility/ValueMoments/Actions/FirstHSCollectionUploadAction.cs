using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class FirstHSCollectionUploadAction : VMAction
	{
		public const string Name = "Upload First Hearthstone Collection";

		public FirstHSCollectionUploadAction(int collectionSize) : base(
			Name, ActionSource.App, "First Collection Upload", Franchise.HSConstructed, null, null
		)
		{
			CollectionSize = collectionSize;
		}

		[JsonProperty("collection_size")]
		public int CollectionSize { get; }
	}
}
