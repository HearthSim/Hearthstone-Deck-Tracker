using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Utility
{
	class ComboBoxHelper
	{
		private static readonly List<ComboBox> ComboBoxes = new List<ComboBox>();
		private static bool _updating;

		public static readonly DependencyProperty SelectionChangedProperty =
			DependencyProperty.RegisterAttached("SelectionChanged", typeof(SelectionChangedEventHandler), typeof(ComboBoxHelper),
				new FrameworkPropertyMetadata(OnChanged));

		private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is ComboBox comboBox))
				throw new ArgumentException("Target is not a Combobox");
			if (e.NewValue != null)
			{
				comboBox.SelectionChanged += HandleSelectionChanged;
				ComboBoxes.Add(comboBox);
			}
			else
			{
				comboBox.SelectionChanged -= HandleSelectionChanged;
				ComboBoxes.Remove(comboBox);
			}
		}

		private static void HandleSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			if (!_updating && sender is UIElement element)
				GetSelectionChanged(element).Invoke(sender, args);
		}

		public static void Update()
		{
			_updating = true;
			foreach (var comboBox in ComboBoxes)
			{
				var index = comboBox.SelectedIndex;
				comboBox.SelectedIndex = -1;
				comboBox.Items.Refresh();
				comboBox.SelectedIndex = index;
			}
			_updating = false;
		}

		public static SelectionChangedEventHandler GetSelectionChanged(UIElement element)
		{
			return (SelectionChangedEventHandler)element.GetValue(SelectionChangedProperty);
		}

		public static void SetSelectionChanged(UIElement element, SelectionChangedEventHandler value)
		{
			element.SetValue(SelectionChangedProperty, value);
		}
	}
}
