using System;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class CollectionUploadedToast
	{
		public CollectionUploadedToast()
		{
			InitializeComponent();
		}

		private void CollectionUploadedToast_OnClicked(object sender, EventArgs e)
		{
			ToastManager.ForceCloseToast(this);
			var dust = CollectionHelper.TryGetCollection(out var collection) ? collection.Dust : 0;
			Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("decks", "collection_uploaded_toast", "maxDustCost=" + dust));
		}
	}
}
