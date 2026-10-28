using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public partial class OverlayRelatedCards : UserControl, IOptionsPageWithOwnScrolling, IOptionsPageWithFilter
{
	private readonly RelatedCardSettingsViewModel _viewModel = new();

	public OverlayRelatedCards()
	{
		InitializeComponent();
		DataContext = _viewModel;
	}

	/// <summary>
	/// Deliberately called only when the page is selected, never from OptionsMain.Load(): building
	/// the catalog instantiates every related card, which has no business happening on the startup
	/// path for the majority of users who never open this page.
	/// </summary>
	public void Load() => _viewModel.Load();

	public string Filter
	{
		get => _viewModel.Filter;
		set
		{
			// the search matched regardless of customization, so show everything it found
			if(value.Length > 0)
				_viewModel.CustomizedOnly = false;
			_viewModel.Filter = value;
		}
	}

	public bool HasFilterMatches(string filter) => _viewModel.HasMatches(filter);
}
