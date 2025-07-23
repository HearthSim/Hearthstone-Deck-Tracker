namespace HearthWatcher.EventArgs
{
	public class UIEventArgs : System.EventArgs
	{
		public bool IsShopOpen { get; }
		public bool IsJournalOpen { get; }
		public bool IsPopupShowing { get; }
		public bool IsFriendsListVisible { get; }
		public bool IsBlurActive { get; }

		public UIEventArgs(
			bool isShopOpen, bool isJournalOpen, bool isPopupShowing, bool isFriendsListVisible, bool isBlurActive
		)
		{
			IsShopOpen = isShopOpen;
			IsJournalOpen = isJournalOpen;
			IsPopupShowing = isPopupShowing;
			IsFriendsListVisible = isFriendsListVisible;
			IsBlurActive = isBlurActive;
		}

		public override bool Equals(object obj) => obj is UIEventArgs args
			&& IsShopOpen == args.IsShopOpen
			&& IsJournalOpen == args.IsJournalOpen
			&& IsPopupShowing == args.IsPopupShowing
			&& IsFriendsListVisible == args.IsFriendsListVisible
			&& IsBlurActive == args.IsBlurActive;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + IsShopOpen.GetHashCode();
			hashCode = hashCode * -1521134295 + IsJournalOpen.GetHashCode();
			hashCode = hashCode * -1521134295 + IsPopupShowing.GetHashCode();
			hashCode = hashCode * -1521134295 + IsFriendsListVisible.GetHashCode();
			hashCode = hashCode * -1521134295 + IsBlurActive.GetHashCode();
			return hashCode;
		}
	}
}
