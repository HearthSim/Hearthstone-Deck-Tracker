#region

using System;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts
{
	/// <summary>
	/// Interaction logic for DeckPickerItemLayoutLegacy.xaml
	/// </summary>
	public partial class DeckPickerItemLayoutLegacy : UserControl
	{
		public DeckPickerItemLayoutLegacy()
		{
			InitializeComponent();
		}

		private DateTime _mouseDown = DateTime.MinValue;
		private void UseButton_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			_mouseDown = DateTime.Now;
		}

		private void UseButton_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if(DateTime.Now.Subtract(_mouseDown).TotalMilliseconds >= 1000)
				return;
			if(DataContext is not DeckPickerItemViewModel { Deck: Deck deck })
				return;
			DeckList.Instance.ActiveDeck = deck;
		}
	}
}
