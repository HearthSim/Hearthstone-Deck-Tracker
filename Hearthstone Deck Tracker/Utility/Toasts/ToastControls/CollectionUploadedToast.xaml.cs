using System;
using Hearthstone_Deck_Tracker.HsReplay;

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
			HSReplayNetHelper.OpenDecksUrlWithCollection("collection_uploaded_toast");
		}
	}
}
