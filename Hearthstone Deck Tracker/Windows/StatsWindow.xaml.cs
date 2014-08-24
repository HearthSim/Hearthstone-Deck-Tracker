using System.ComponentModel;
using System.Windows;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for StatsWindow.xaml
	/// </summary>
	public partial class StatsWindow
	{
		private bool _appIsClosing;

		public StatsWindow()
		{
			InitializeComponent();

			Height = Config.Instance.StatsWindowHeight;
			Width = Config.Instance.StatsWindowWidth;
			if(Config.Instance.StatsWindowLeft.HasValue)
				Left = Config.Instance.StatsWindowLeft.Value;
			if(Config.Instance.StatsWindowTop.HasValue)
				Top = Config.Instance.StatsWindowTop.Value;
		}


		protected override void OnClosing(CancelEventArgs e)
		{
			if(_appIsClosing) return;
			e.Cancel = true;
			Hide();
		}

		internal void Shutdown()
		{
			_appIsClosing = true;
			Close();
		}

		private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FlyoutGameDetails.Width = Width;
		}
	}
}