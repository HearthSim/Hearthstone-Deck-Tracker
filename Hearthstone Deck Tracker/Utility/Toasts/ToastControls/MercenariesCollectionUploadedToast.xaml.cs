using System;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using static Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.VMActions.ToastAction;

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class MercenariesCollectionUploadedToast
	{
		public MercenariesCollectionUploadedToast()
		{
			InitializeComponent();
		}

		private void CollectionUploadedToast_OnClicked(object sender, EventArgs e)
		{
			ToastManager.ForceCloseToast(this);
			Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("/mercenaries/collection/mine/", "collection_uploaded_toast"));
			HSReplayNetClientAnalytics.TryTrackToastClick(Franchise.Mercenaries, ToastName.MercenariesCollectionUploaded);
		}
	}
}
