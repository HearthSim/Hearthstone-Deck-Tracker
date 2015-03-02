#region

using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Decks
{
	/// <summary>
	/// Interaction logic for General.xaml
	/// </summary>
	public partial class DecksGeneral
	{
		private bool _initialized;

		public DecksGeneral()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			_initialized = true;
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseSameScaling = true;
			Config.Save();
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseSameScaling = false;
			Config.Save();
		}
	}
}