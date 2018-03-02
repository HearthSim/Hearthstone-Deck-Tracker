using System;

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
			Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("decks", "collection_uploaded_toast"));
		}
	}
}
