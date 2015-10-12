#region

using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for ElementSorterItem.xaml
	/// </summary>
	public partial class ElementSorterItem
	{
		private readonly bool _initialized;
		private readonly bool _isPlayerList;
		private readonly Action<bool> _setConfigValue;
		public readonly string ItemName;

		public ElementSorterItem(string name, bool isChecked, Action<bool> setConfigValue, bool isPlayerList)
		{
			InitializeComponent();
			CheckBox.Content = name;
			ItemName = name;
			CheckBox.IsChecked = isChecked;
			_setConfigValue = setConfigValue;
			_isPlayerList = isPlayerList;
			_initialized = true;
		}

		private void ButtonUp_OnClick(object sender, RoutedEventArgs e)
		{
			if(_isPlayerList)
			{
				Core.MainWindow.Options.OptionsOverlayPlayer.ElementSorterPlayer.MoveItem(this, SortDirection.Up);
				Core.Overlay.UpdatePlayerLayout();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Core.MainWindow.Options.OptionsOverlayOpponent.ElementSorterOpponent.MoveItem(this, SortDirection.Up);
				Core.Overlay.UpdateOpponentLayout();
				Core.Windows.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void ButtonDown_OnClick(object sender, RoutedEventArgs e)
		{
			if(_isPlayerList)
			{
				Core.MainWindow.Options.OptionsOverlayPlayer.ElementSorterPlayer.MoveItem(this, SortDirection.Down);
				Core.Overlay.UpdatePlayerLayout();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Core.MainWindow.Options.OptionsOverlayOpponent.ElementSorterOpponent.MoveItem(this, SortDirection.Down);
				Core.Overlay.UpdateOpponentLayout();
				Core.Windows.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_setConfigValue(true);
			Config.Save();
			Core.Overlay.Update(false);
			if(_isPlayerList)
				Core.Windows.PlayerWindow.Update();
			else
				Core.Windows.OpponentWindow.Update();
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_setConfigValue(false);
			Config.Save();
			Core.Overlay.Update(false);
			if(_isPlayerList)
				Core.Windows.PlayerWindow.Update();
			else
				Core.Windows.OpponentWindow.Update();
		}
	}
}