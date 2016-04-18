#region

using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	/// <summary>
	/// Interaction logic for ReplayToast.xaml
	/// </summary>
	public partial class ReplayToast : UserControl
	{
		private readonly GameStats _game;

		public ReplayToast([NotNull] GameStats game)
		{
			InitializeComponent();
			_game = game;
		}

		private void BorderReplay_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			HsReplayManager.ShowReplay(_game, true).Forget();
			ToastManager.ForceCloseToast(this);
		}

		private void BorderReplay_OnMouseEnter(object sender, MouseEventArgs e)
		{
			if(Cursor != Cursors.Wait)
				Cursor = Cursors.Hand;
		}

		private void BorderReplay_OnMouseLeave(object sender, MouseEventArgs e)
		{
			if(Cursor != Cursors.Wait)
				Cursor = Cursors.Arrow;
		}
	}
}