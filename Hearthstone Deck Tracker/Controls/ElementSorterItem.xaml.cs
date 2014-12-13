﻿using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for ElementSorterItem.xaml
	/// </summary>
	public partial class ElementSorterItem
	{
		public readonly string ItemName;
		private readonly bool _initialized;
		private readonly bool _isPlayerList;
		private readonly Action<bool> _setConfigValue;

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
				Helper.MainWindow.Options.ElementSorterPlayer.MoveItem(this, SortDirection.Up);
				Helper.MainWindow.Overlay.UpdatePlayerLayout();
				Helper.MainWindow.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Helper.MainWindow.Options.ElementSorterOpponent.MoveItem(this, SortDirection.Up);
				Helper.MainWindow.Overlay.UpdateOpponentLayout();
				Helper.MainWindow.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void ButtonDown_OnClick(object sender, RoutedEventArgs e)
		{
			if(_isPlayerList)
			{
				Helper.MainWindow.Options.ElementSorterPlayer.MoveItem(this, SortDirection.Down);
				Helper.MainWindow.Overlay.UpdatePlayerLayout();
				Helper.MainWindow.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				Helper.MainWindow.Options.ElementSorterOpponent.MoveItem(this, SortDirection.Down);
				Helper.MainWindow.Overlay.UpdateOpponentLayout();
				Helper.MainWindow.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			_setConfigValue(true);
			Config.Save();
			Helper.MainWindow.Overlay.Update(false);
			if(_isPlayerList)
				Helper.MainWindow.PlayerWindow.Update();
			else
				Helper.MainWindow.OpponentWindow.Update();
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			_setConfigValue(false);
			Config.Save();
			Helper.MainWindow.Overlay.Update(false);
			if(_isPlayerList)
				Helper.MainWindow.PlayerWindow.Update();
			else
				Helper.MainWindow.OpponentWindow.Update();
		}
	}
}