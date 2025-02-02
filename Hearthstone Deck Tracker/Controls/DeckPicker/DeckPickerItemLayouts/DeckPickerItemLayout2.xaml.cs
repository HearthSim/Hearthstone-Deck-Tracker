#region

using System;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts
{
	/// <summary>
	/// Interaction logic for DeckPickerItemLayout2.xaml
	/// </summary>
	public partial class DeckPickerItemLayout2
	{
		public DeckPickerItemLayout2()
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
