namespace Hearthstone_Deck_Tracker.FlyoutControls.Options;

/// <summary>
/// An options page listing entries (such as counters) that the options search should find,
/// prefiltering the page to what it found.
/// </summary>
public interface IOptionsPageWithFilter
{
	string Filter { get; set; }

	bool HasFilterMatches(string filter);
}
