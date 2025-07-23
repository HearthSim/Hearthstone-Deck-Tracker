namespace HearthWatcher.Providers
{
	public interface IUiProvider
	{
		bool? IsShopOpen { get; }
		bool? IsJournalOpen { get; }
		bool? IsPopupShowing { get; }
		bool? IsFriendsListVisible { get; }
		bool? IsBlurActive { get; }
	}
}
