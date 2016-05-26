#region

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherImporting.xaml
	/// </summary>
	public partial class TrackerImporting
	{
		private GameV2 _game;
		private bool _initialized;

		public TrackerImporting()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);

		public void Load(GameV2 game)
		{
			_game = game;
			ComboboxArenaImportingBehaviour.ItemsSource = Enum.GetValues(typeof(ArenaImportingBehaviour));
			if(Config.Instance.SelectedArenaImportingBehaviour.HasValue)
				ComboboxArenaImportingBehaviour.SelectedItem = Config.Instance.SelectedArenaImportingBehaviour.Value;
			CheckboxTagOnImport.IsChecked = Config.Instance.TagDecksOnImport;
			CheckboxImportNetDeck.IsChecked = Config.Instance.NetDeckClipboardCheck ?? false;
			CheckboxAutoSaveOnImport.IsChecked = Config.Instance.AutoSaveOnImport;
			TextBoxArenaTemplate.Text = Config.Instance.ArenaDeckNameTemplate;
			CheckBoxConstructedImportNew.IsChecked = Config.Instance.ConstructedAutoImportNew;
			CheckBoxConstrucedUpdate.IsChecked = Config.Instance.ConstructedAutoUpdate;
			_initialized = true;
		}

		private void CheckboxTagOnImport_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TagDecksOnImport = true;
			Config.Save();
		}

		private void CheckboxTagOnImport_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TagDecksOnImport = false;
			Config.Save();
		}

		private void CheckboxImportNetDeck_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NetDeckClipboardCheck = true;
			Config.Save();
		}

		private void CheckboxImportNetDeck_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NetDeckClipboardCheck = false;
			Config.Save();
		}

		private void CheckboxAutoSaveOnImport_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSaveOnImport = true;
			Config.Save();
		}

		private void CheckboxAutoSaveOnImport_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSaveOnImport = false;
			Config.Save();
		}

		private void BtnEditTemplate_Click(object sender, RoutedEventArgs e)
		{
			if(TextBoxArenaTemplate.IsEnabled)
			{
				BtnEditTemplate.Content = "EDIT";
				Config.Instance.ArenaDeckNameTemplate = TextBoxArenaTemplate.Text;
				Config.Save();
				TextBoxArenaTemplate.IsEnabled = false;
			}
			else
			{
				BtnEditTemplate.Content = "SAVE";
				TextBoxArenaTemplate.IsEnabled = true;
			}
		}

		private void TextBoxArenaTemplate_OnTextChanged(object sender, TextChangedEventArgs e) 
			=> TextBlockNamePreview.Text = Helper.ParseDeckNameTemplate(TextBoxArenaTemplate.Text, new Deck() {Class = "ClassName"});

		private void ComboboxArenaImportingBehaviour_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var selected = ComboboxArenaImportingBehaviour.SelectedItem as ArenaImportingBehaviour?;
			if(selected != null)
			{
				Config.Instance.SelectedArenaImportingBehaviour = selected;
				Config.Save();
			}
		}

		private void CheckBoxConstructedImportNew_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedAutoImportNew = true;
			Config.Save();
		}

		private void CheckBoxConstructedImportNew_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedAutoImportNew = false;
			Config.Save();
		}

		private void CheckBoxConstrucedUpdate_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedAutoUpdate= true;
			Config.Save();
		}

		private void CheckBoxConstrucedUpdate_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedAutoUpdate = false;
			Config.Save();
		}
	}
}