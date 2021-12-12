using System;

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
		}
	}
}
