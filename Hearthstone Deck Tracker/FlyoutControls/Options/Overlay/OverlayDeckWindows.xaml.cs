#region

using System.Drawing;
using System.Linq;
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
				Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmost = true;
			Helper.MainWindow.PlayerWindow.Topmost = true;
			Helper.MainWindow.OpponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmost = false;
			Helper.MainWindow.PlayerWindow.Topmost = false;
			Helper.MainWindow.OpponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			Helper.MainWindow.PlayerWindow.Topmost = false;
			Helper.MainWindow.OpponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if(Config.Instance.WindowsTopmost)
			{
				Helper.MainWindow.PlayerWindow.Topmost = true;
				Helper.MainWindow.OpponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmost = true;
			Helper.MainWindow.TimerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmost = false;
			Helper.MainWindow.TimerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.MainWindow.TimerWindow.Show();
			Helper.MainWindow.TimerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.MainWindow.TimerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			Helper.MainWindow.TimerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if(Config.Instance.TimerWindowTopmost)
				Helper.MainWindow.TimerWindow.Topmost = true;
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
			var background = BackgroundFromHex();
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
			Helper.MainWindow.PlayerWindow.Show();
			Helper.MainWindow.PlayerWindow.Activate();
			Helper.MainWindow.PlayerWindow.SetCardCount(_game.PlayerHandCount,
			                                            30 - _game.PlayerDrawn.Where(c => !c.IsStolen).Sum(card => card.Count));
			Config.Instance.PlayerWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxPlayerWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.MainWindow.PlayerWindow.Hide();
			Config.Instance.PlayerWindowOnStart = false;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.MainWindow.OpponentWindow.Show();
			Helper.MainWindow.OpponentWindow.Activate();
			Helper.MainWindow.OpponentWindow.SetOpponentCardCount(_game.OpponentHandCount, _game.OpponentDeckCount, _game.OpponentHasCoin);
			Config.Instance.OpponentWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.MainWindow.OpponentWindow.Hide();
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
				var hexBackground = BackgroundFromHex();
				if(hexBackground != null)
				{
					Helper.MainWindow.PlayerWindow.Background = hexBackground;
					Helper.MainWindow.OpponentWindow.Background = hexBackground;
					Helper.MainWindow.TimerWindow.Background = hexBackground;
				}
			}
			else
			{
				Helper.MainWindow.PlayerWindow.Background = background;
				Helper.MainWindow.OpponentWindow.Background = background;
				Helper.MainWindow.TimerWindow.Background = background;
			}
		}

		private SolidColorBrush BackgroundFromHex()
		{
			SolidColorBrush brush = null;
			var hex = TextboxCustomBackground.Text;
			if(hex.StartsWith("#"))
				hex = hex.Remove(0, 1);
			if(!string.IsNullOrEmpty(hex) && hex.Length == 6 && Helper.IsHex(hex))
			{
				var color = ColorTranslator.FromHtml("#" + hex);
				brush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
			}
			return brush;
		}
	}
}