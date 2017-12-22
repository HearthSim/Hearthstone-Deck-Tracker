#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.HotKeys;
using Hearthstone_Deck_Tracker.Utility.Logging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ModifierKeys = Hearthstone_Deck_Tracker.Utility.HotKeys.ModifierKeys;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerHotKeys.xaml
	/// </summary>
	public partial class TrackerHotKeys : INotifyPropertyChanged
	{
		private string _errorText;

		public TrackerHotKeys()
		{
			InitializeComponent();
		}

		public HotKey SelectedHotKey
		{
			get
			{
				if(string.IsNullOrEmpty(TextBoxKey.Text))
					return null;
				Keys key;
				if(!Enum.TryParse(TextBoxKey.Text, out key))
					return null;
				if(key == Keys.None)
					return null;
				return new HotKey((ModifierKeys)ComboBoxMod.SelectedValue, key);
			}
		}

		public bool SelectedHotKeyIsValue
		{
			get { return SelectedHotKey != null && !HotKeyManager.RegisteredHotKeysInfo.Any(x => Equals(x.Key, SelectedHotKey)); }
		}

		public string ErrorText
		{
			get { return _errorText; }
			set
			{
				_errorText = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ButtonAddNew_OnClick(object sender, RoutedEventArgs e)
		{
			var success = HotKeyManager.AddPredefinedHotkey(SelectedHotKey,
			                                                ((PredefinedHotKeyActionInfo)ComboBoxActions.SelectedItem).MethodName);
			if(!success)
				ErrorText = "Could not register hotkey.";
		}

		private void ComboBoxMod_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(SelectedHotKeyIsValue));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
		{
			if(DataGridHotKeys.SelectedItem == null)
				return;
			try
			{
				var hotkey = ((KeyValuePair<HotKey, string>)DataGridHotKeys.SelectedItem).Key;
				HotKeyManager.RemovePredefinedHotkey(hotkey);
			}
			catch(Exception ex)
			{
				Log.Error("Error deleting hotkey: " + ex);
			}
		}

		private void TextBoxKey_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			ErrorText = "";
			TextBoxKey.Text = e.Key == Key.System ? "None" : e.Key.ToString();
			e.Handled = true;
			OnPropertyChanged(nameof(SelectedHotKeyIsValue));
		}
	}
}
