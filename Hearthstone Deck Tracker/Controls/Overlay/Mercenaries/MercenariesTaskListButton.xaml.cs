using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries
{
	public partial class MercenariesTaskListButton : UserControl
	{
		public MercenariesTaskListButton()
		{
			InitializeComponent();
		}

		private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			(DataContext as MercenariesTaskListViewModel)?.OnMouseEnter();
		}

		private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			(DataContext as MercenariesTaskListViewModel)?.OnMouseLeave();
		}

		private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			(DataContext as MercenariesTaskListViewModel)?.OnMouseEnter();
		}
	}
}
