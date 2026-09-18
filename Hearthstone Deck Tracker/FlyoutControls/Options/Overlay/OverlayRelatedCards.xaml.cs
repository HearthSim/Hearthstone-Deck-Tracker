using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public partial class OverlayRelatedCards : UserControl
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
}
