#region

using System;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class ReplayToast
	{
		private readonly GameStats _game;

		public ReplayToast([NotNull] GameStats game)
		{
			InitializeComponent();
			_game = game;
		}

		private async void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			await Task.Delay(500);
			ReplayLauncher.ShowReplay(_game, true).Forget();
		}
	}
}
