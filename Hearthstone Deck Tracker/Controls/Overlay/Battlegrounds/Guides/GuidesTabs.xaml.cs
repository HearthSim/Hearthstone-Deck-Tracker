using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public partial class GuidesTabs : UserControl
{
	// Background colors matching the original Button styling.
	private static readonly SolidColorBrush BgDefault = new(Color.FromRgb(0x14, 0x16, 0x17));
	private static readonly SolidColorBrush BgHover = new(Color.FromRgb(0x2C, 0x31, 0x35));
	private static readonly SolidColorBrush BgActive = new(Color.FromRgb(0x23, 0x27, 0x2A));

	public GuidesTabs()
	{
		InitializeComponent();
	}

	private void Tab_MouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Border border && !IsTabActive(border))
			border.Background = BgHover;
	}

	private void Tab_MouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Border border && !IsTabActive(border))
			border.Background = BgDefault;
	}

	private static bool IsTabActive(Border tab)
	{
		return tab.Background is SolidColorBrush brush
			&& Color.Equals(brush.Color, BgActive.Color);
	}

	private void TabMinions_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is BattlegroundsGuidesTabsViewModel vm)
			vm.ShowMinionsCommand?.Execute(null);
	}

	private void TabComps_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is BattlegroundsGuidesTabsViewModel vm)
			vm.ShowCompsCommand?.Execute(null);
	}

	private void TabHeroes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is BattlegroundsGuidesTabsViewModel vm)
			vm.ShowHeroesCommand?.Execute(null);
	}
}

