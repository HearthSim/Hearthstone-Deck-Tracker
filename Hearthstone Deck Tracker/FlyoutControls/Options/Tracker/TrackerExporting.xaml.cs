#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherExporting.xaml
	/// </summary>
	public partial class TrackerExporting
	{
		private bool _initialized;

		public TrackerExporting()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxExportName.IsChecked = Config.Instance.ExportSetDeckName;
			CheckboxPrioGolden.IsChecked = Config.Instance.PrioritizeGolden;
			CheckboxExportPasteClipboard.IsChecked = Config.Instance.ExportPasteClipboard;
			CheckboxAutoClear.IsChecked = Config.Instance.AutoClearDeck;
			CheckboxAutoClearFilters.IsChecked = Config.Instance.EnableExportAutoFilter;
			TextboxExportDelay.Text = Config.Instance.ExportStartDelay.ToString();
			CheckboxShowDialog.IsChecked = Config.Instance.ShowExportingDialog;
			CheckboxExportAddVersion.IsChecked = Config.Instance.ExportAddDeckVersionToName;
			CheckboxForceClear.IsChecked = Config.Instance.ExportForceClear;

			var delay = Config.Instance.DeckExportDelay;
			ComboboxExportSpeed.SelectedIndex = delay < 40 ? 0 : delay < 60 ? 1 : delay < 100 ? 2 : delay < 150 ? 3 : 4;
			_initialized = true;
		}

		private void CheckboxExportName_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportSetDeckName = true;
			Config.Save();
		}

		private void CheckboxExportName_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportSetDeckName = false;
			Config.Save();
		}

		private void CheckboxPrioGolden_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PrioritizeGolden = true;
			Config.Save();
		}

		private void CheckboxPrioGolden_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PrioritizeGolden = false;
			Config.Save();
		}

		private void ComboboxExportSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var selected = ComboboxExportSpeed.SelectedValue.ToString();

			switch(selected)
			{
				case "Very Fast (20ms)":
					Config.Instance.DeckExportDelay = 20;
					break;
				case "Fast (40ms)":
					Config.Instance.DeckExportDelay = 40;
					break;
				case "Normal (60ms)":
					Config.Instance.DeckExportDelay = 60;
					break;
				case "Slow (100ms)":
					Config.Instance.DeckExportDelay = 100;
					break;
				case "Very Slow (150ms)":
					Config.Instance.DeckExportDelay = 150;
					break;
				default:
					return;
			}
			Config.Save();
		}

		private void CheckboxExportPasteClipboard_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportPasteClipboard = true;
			Config.Save();
		}

		private void CheckboxExportPasteClipboard_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportPasteClipboard = false;
			Config.Save();
		}

		private void CheckboxAutoClear_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoClearDeck = true;
			Config.Save();
		}

		private void CheckboxAutoClear_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoClearDeck = false;
			Config.Save();
		}

		private void CheckboxAutoClearFilters_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableExportAutoFilter = true;
			Config.Save();
		}

		private void CheckboxAutoClearFilters_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableExportAutoFilter = false;
			Config.Save();
		}

		private void TextboxExportDelay_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized)
				return;
			int exportStartDelay;
			if(int.TryParse(TextboxExportDelay.Text, out exportStartDelay))
			{
				if(exportStartDelay < 0)
				{
					TextboxExportDelay.Text = "0";
					exportStartDelay = 0;
				}

				if(exportStartDelay > 60)
				{
					TextboxExportDelay.Text = "60";
					exportStartDelay = 60;
				}

				Config.Instance.ExportStartDelay = exportStartDelay;
				Config.Save();
			}
		}

		private void TextboxExportDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void CheckboxShowDialog_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowExportingDialog = true;
			Config.Save();
		}

		private void CheckboxShowDialog_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowExportingDialog = false;
			Config.Save();
		}

		private void CheckboxExportAddVersion_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportAddDeckVersionToName = true;
			Config.Save();
		}

		private void CheckboxExportAddVersion_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportAddDeckVersionToName = false;
			Config.Save();
		}

		private void CheckboxForceClear_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportForceClear = true;
			Config.Save();
		}

		private void CheckboxForceClear_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportForceClear = false;
			Config.Save();
		}
	}
}