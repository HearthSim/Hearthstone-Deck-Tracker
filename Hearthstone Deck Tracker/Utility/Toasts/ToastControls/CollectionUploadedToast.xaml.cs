using System;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

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
			HSReplayNetClientAnalytics.TryTrackToastClick(Franchise.HSConstructed, ToastAction.Toast.ConstructedCollectionUploaded);
		}
	}
}
