namespace Hearthstone_Deck_Tracker.FlyoutControls.Options;

/// <summary>
/// Marks an options page that manages its own scrolling, typically because it hosts a long
/// virtualized list. <see cref="OptionsMain"/> normally wraps every page in a ScrollViewer, which
/// gives the page unbounded height and so defeats virtualization (and would add a second scrollbar
/// next to the page's own). For these pages it turns that ScrollViewer off, so the page is laid out
/// at the height of the visible area and its own list is the only thing that scrolls.
/// </summary>
public interface IOptionsPageWithOwnScrolling
{
}
