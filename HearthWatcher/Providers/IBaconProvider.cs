namespace HearthWatcher.Providers
{
	public interface IBaconProvider
	{
		bool? IsShopOpen { get; }
		bool? IsJournalOpen { get; }
		bool? IsPopupShowing { get; }
		bool? IsFriendslistOpen { get; }
	}
}
