using System;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using static Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.VMActions.ToastAction;

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
			HSReplayNetClientAnalytics.TryTrackToastClick(Franchise.HSConstructed, ToastName.ConstructedCollectionUploaded);
		}
	}
}
