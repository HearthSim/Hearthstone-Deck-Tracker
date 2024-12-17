using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public partial class GuidesTabs : UserControl
{
	public GuidesTabs()
	{
		InitializeComponent();
	}


	private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is TabItem tabItem)
		{
			var tabControl = ItemsControl.ItemsControlFromItemContainer(tabItem) as TabControl;
			if (tabControl != null)
			{
				// Check if the click is on the TabItem header
				var header = GetHeader(tabItem);
				if (header != null && header.IsMouseOver)
				{
					bool isSelected = tabItem.IsSelected;
					tabItem.IsSelected = !isSelected;
					if (isSelected)
					{
						tabControl.SelectedItem = null;
					}
					else if((string)tabItem.Header == "Comps")
					{
						Core.Game.Metrics.BattlegroundsCompsTabClicks++;
					}
					e.Handled = true;
					Core.Game.Metrics.BattlegroundsTopTabClicks++;
				}
			}
		}
	}

	private FrameworkElement? GetHeader(TabItem tabItem)
	{
		var contentPresenter = FindChild<ContentPresenter>(tabItem);
		return contentPresenter?.Parent as FrameworkElement;
	}

	private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
	{
		if (parent == null) return null;

		int childCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childCount; i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i);
			if (child is T foundChild)
			{
				return foundChild;
			}

			var result = FindChild<T>(child);
			if (result != null)
			{
				return result;
			}
		}
		return null;
	}
}

