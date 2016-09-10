#region

using System;
using System.Windows;
using static Hearthstone_Deck_Tracker.Enums.SortDirection;

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
			switch (name)
			{
				case "Deck Title":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_DeckTitle");
					break;
				case "Cards":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_Cards");
					break;
				case "Card Counter":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_CardCount");
					break;
				case "Fatigue Counter":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_FatigueCount");
					break;
				case "Draw Chances":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_DrawChances");
					break;
				case "Wins":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_Wins");
					break;
				case "Win Rate":
					CheckBox.Content = Utility.LocUtil.Get("Options_Overlay_SortedElement_WinRate");
					break;
				default:
					CheckBox.Content = name;
					break;
			}
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
				Core.MainWindow.Options.OptionsOverlayPlayer.ElementSorterPlayer.MoveItem(this, Up);
				Core.Overlay.UpdatePlayerLayout();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Core.MainWindow.Options.OptionsOverlayOpponent.ElementSorterOpponent.MoveItem(this, Up);
				Core.Overlay.UpdateOpponentLayout();
				Core.Windows.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void ButtonDown_OnClick(object sender, RoutedEventArgs e)
		{
			if(_isPlayerList)
			{
				Core.MainWindow.Options.OptionsOverlayPlayer.ElementSorterPlayer.MoveItem(this, Down);
				Core.Overlay.UpdatePlayerLayout();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Core.MainWindow.Options.OptionsOverlayOpponent.ElementSorterOpponent.MoveItem(this, Down);
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
			{
				Core.Windows.PlayerWindow.Update();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Core.Windows.OpponentWindow.Update();
				Core.Windows.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_setConfigValue(false);
			Config.Save();
			Core.Overlay.Update(false);
			if(_isPlayerList)
			{
				Core.Windows.PlayerWindow.Update();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Core.Windows.OpponentWindow.Update();
				Core.Windows.OpponentWindow.UpdateOpponentLayout();
			}
		}
	}
}