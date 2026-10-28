using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public partial class OverlayCounters : UserControl, IOptionsPageWithOwnScrolling, IOptionsPageWithFilter
{
	private readonly CounterSettingsViewModel _viewModel = new();

	public OverlayCounters()
	{
		InitializeComponent();
		DataContext = _viewModel;
	}

	/// <summary>
	/// Deliberately called only when the page is selected, never from OptionsMain.Load(): building
	/// the catalog instantiates every counter, which has no business happening on the startup path
	/// for the majority of users who never open this page.
	/// </summary>
	public void Load() => _viewModel.Load();

	public string Filter
	{
		get => _viewModel.Filter;
		set => _viewModel.Filter = value;
	}

	public bool HasFilterMatches(string filter) => _viewModel.HasMatches(filter);
}
