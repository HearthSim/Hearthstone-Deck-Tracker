#region

using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Animations;
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
		private readonly Action<ElementSorterItem, SortDirection> _moveItem;
		private readonly Action<bool> _setConfigValue;
		public DeckPanel Panel { get; }

		public ElementSorterItem(DeckPanel panel, bool isChecked, Action<bool> setConfigValue, bool isPlayerList, Action<ElementSorterItem, SortDirection> moveItem)
		{
			InitializeComponent();
			Panel = panel;
			CheckBox.IsChecked = isChecked;
			_setConfigValue = setConfigValue;
			_isPlayerList = isPlayerList;
			_moveItem = moveItem;
			_initialized = true;
		}

		private void ButtonUp_OnClick(object sender, RoutedEventArgs e)
		{
			if(_isPlayerList)
			{
				_moveItem(this, Up);
				Core.Overlay.UpdatePlayerLayout();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				_moveItem(this, Up);
				Core.Overlay.UpdateOpponentLayout();
				Core.Windows.OpponentWindow.UpdateOpponentLayout();
			}
		}

		private void ButtonDown_OnClick(object sender, RoutedEventArgs e)
		{
			if(_isPlayerList)
			{
				_moveItem(this, Down);
				Core.Overlay.UpdatePlayerLayout();
				Core.Windows.PlayerWindow.UpdatePlayerLayout();
			}
			else
			{
				_moveItem(this, Down);
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
