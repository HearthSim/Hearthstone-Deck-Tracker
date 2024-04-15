using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class BaconEventArgs : System.EventArgs
	{
		public bool IsShopOpen { get; }
		public bool IsJournalOpen { get; }
		public bool IsPopupShowing { get; }
		public bool IsFriendslistOpen { get; }
		public bool IsBlurActive { get; }
		public SelectedBattlegroundsGameMode SelectedBattlegroundsGameMode { get; }

		public BaconEventArgs(
			bool isShopOpen, bool isJournalOpen, bool isPopupShowing, bool isFriendslistOpen, bool isBlurActive, SelectedBattlegroundsGameMode selectedBattlegroundsGameMode
		)
		{
			IsShopOpen = isShopOpen;
			IsJournalOpen = isJournalOpen;
			IsPopupShowing = isPopupShowing;
			IsFriendslistOpen = isFriendslistOpen;
			IsBlurActive = isBlurActive;
			SelectedBattlegroundsGameMode = selectedBattlegroundsGameMode;
		}

		public bool IsAnyOpen => IsShopOpen || IsJournalOpen || IsPopupShowing || IsFriendslistOpen || IsBlurActive;

		public override bool Equals(object obj) => obj is BaconEventArgs args
			&& IsShopOpen == args.IsShopOpen
			&& IsJournalOpen == args.IsJournalOpen
			&& IsPopupShowing == args.IsPopupShowing
			&& IsFriendslistOpen == args.IsFriendslistOpen
			&& IsBlurActive == args.IsBlurActive
			&& SelectedBattlegroundsGameMode == args.SelectedBattlegroundsGameMode;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + IsShopOpen.GetHashCode();
			hashCode = hashCode * -1521134295 + IsJournalOpen.GetHashCode();
			hashCode = hashCode * -1521134295 + IsPopupShowing.GetHashCode();
			hashCode = hashCode * -1521134295 + IsFriendslistOpen.GetHashCode();
			hashCode = hashCode * -1521134295 + IsBlurActive.GetHashCode();
			hashCode = hashCode * -1521134295 + SelectedBattlegroundsGameMode.GetHashCode();
			return hashCode;
		}
	}
}
