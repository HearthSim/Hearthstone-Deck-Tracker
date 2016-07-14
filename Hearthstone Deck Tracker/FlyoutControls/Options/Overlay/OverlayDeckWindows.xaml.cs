#region

using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using SystemColors = System.Windows.SystemColors;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for DeckWindows.xaml
	/// </summary>
	public partial class OverlayDeckWindows
	{
		private GameV2 _game;
		private bool _initialized;

		public OverlayDeckWindows()
		{
			InitializeComponent();
		}

		public void Load(GameV2 game)
		{
			_game = game;
			CheckboxWindowsTopmost.IsChecked = Config.Instance.WindowsTopmost;
			CheckboxPlayerWindowOpenAutomatically.IsChecked = Config.Instance.PlayerWindowOnStart;
			CheckboxOpponentWindowOpenAutomatically.IsChecked = Config.Instance.OpponentWindowOnStart;
			CheckboxTimerTopmost.IsChecked = Config.Instance.TimerWindowTopmost;
			CheckboxTimerWindow.IsChecked = Config.Instance.TimerWindowOnStartup;
			CheckboxTimerTopmostHsForeground.IsChecked = Config.Instance.TimerWindowTopmostIfHsForeground;
			CheckboxTimerTopmostHsForeground.IsEnabled = Config.Instance.TimerWindowTopmost;
			CheckboxWinTopmostHsForeground.IsChecked = Config.Instance.WindowsTopmostIfHsForeground;
			CheckboxWinTopmostHsForeground.IsEnabled = Config.Instance.WindowsTopmost;
			ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
				                               ? "#696969" : Config.Instance.WindowsBackgroundHex;
			UpdateAdditionalWindowsBackground();
			CheckboxWindowCardToolTips.IsChecked = Config.Instance.WindowCardToolTips;
			_initialized = true;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
		}

		private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmost = true;
			Core.Windows.PlayerWindow.Topmost = true;
			Core.Windows.OpponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmost = false;
			Core.Windows.PlayerWindow.Topmost = false;
			Core.Windows.OpponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			Core.Windows.PlayerWindow.Topmost = false;
			Core.Windows.OpponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if(Config.Instance.WindowsTopmost)
			{
				Core.Windows.PlayerWindow.Topmost = true;
				Core.Windows.OpponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmost = true;
			Core.Windows.TimerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmost = false;
			Core.Windows.TimerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.TimerWindow.Show();
			Core.Windows.TimerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.TimerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			Core.Windows.TimerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if(Config.Instance.TimerWindowTopmost)
				Core.Windows.TimerWindow.Topmost = true;
			SaveConfig(false);
		}

		private void ComboboxWindowBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			TextboxCustomBackground.IsEnabled = ComboboxWindowBackground.SelectedItem.ToString() == "Custom";
			Config.Instance.SelectedWindowBackground = ComboboxWindowBackground.SelectedItem.ToString();
			UpdateAdditionalWindowsBackground();
		}

		private void TextboxCustomBackground_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized || ComboboxWindowBackground.SelectedItem.ToString() != "Custom")
				return;
			var background = Helper.BrushFromHex(TextboxCustomBackground.Text);
			if(background != null)
			{
				UpdateAdditionalWindowsBackground(background);
				Config.Instance.WindowsBackgroundHex = TextboxCustomBackground.Text;
				SaveConfig(false);
			}
		}

		private void CheckboxWindowCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowCardToolTips = true;
			SaveConfig(false);
		}

		private void CheckboxWindowCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowCardToolTips = false;
			SaveConfig(false);
		}

		private void CheckboxPlayerWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.PlayerWindow.Show();
			Core.Windows.PlayerWindow.Activate();
			Core.Windows.PlayerWindow.SetCardCount(_game.Player.HandCount, _game.IsInMenu ? 30 : _game.Player.DeckCount);
			Config.Instance.PlayerWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxPlayerWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.PlayerWindow.Hide();
			Config.Instance.PlayerWindowOnStart = false;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.OpponentWindow.Show();
			Core.Windows.OpponentWindow.Activate();
			Core.Windows.OpponentWindow.SetOpponentCardCount(_game.Opponent.HandCount, _game.IsInMenu ? 30 : _game.Opponent.DeckCount, _game.Opponent.HasCoin);
			Config.Instance.OpponentWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.OpponentWindow.Hide();
			Config.Instance.OpponentWindowOnStart = false;
			Config.Save();
		}

		internal void UpdateAdditionalWindowsBackground(Brush brush = null)
		{
			var background = brush;

			switch(ComboboxWindowBackground.SelectedItem.ToString())
			{
				case "Theme":
					background = Background;
					break;
				case "Light":
					background = SystemColors.ControlLightBrush;
					break;
				case "Dark":
					background = SystemColors.ControlDarkDarkBrush;
					break;
			}
			if(background == null)
			{
				var hexBackground = Helper.BrushFromHex(TextboxCustomBackground.Text);
				if(hexBackground != null)
				{
					Core.Windows.PlayerWindow.Background = hexBackground;
					Core.Windows.OpponentWindow.Background = hexBackground;
					Core.Windows.TimerWindow.Background = hexBackground;
				}
			}
			else
			{
				Core.Windows.PlayerWindow.Background = background;
				Core.Windows.OpponentWindow.Background = background;
				Core.Windows.TimerWindow.Background = background;
			}
		}
	}
}