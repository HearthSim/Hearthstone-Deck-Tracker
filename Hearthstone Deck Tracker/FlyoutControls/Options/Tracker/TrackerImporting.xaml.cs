#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	public partial class TrackerImporting : INotifyPropertyChanged
	{
		private bool _initialized;
		private string _editButtonText = LocUtil.Get(LocEdit);
		private bool _templateEditable;
		private const string LocEdit = "Options_Tracker_Importing_Button_Edit";
		private const string LocSave = "Options_Tracker_Importing_Button_Save";

		public TrackerImporting()
		{
			InitializeComponent();
		}

		public string EditButtonText
		{
			get => _editButtonText;
			set
			{
				_editButtonText = value;
				OnPropertyChanged();
			}
		}

		public bool TemplateEditable
		{
			get => _templateEditable;
			set
			{
				_templateEditable = value;
				OnPropertyChanged();
			}
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);

		public void Load()
		{
			ComboboxArenaImportingBehaviour.ItemsSource = Enum.GetValues(typeof(ArenaImportingBehaviour));
			if(Config.Instance.SelectedArenaImportingBehaviour.HasValue)
				ComboboxArenaImportingBehaviour.SelectedItem = Config.Instance.SelectedArenaImportingBehaviour.Value;
			ComboboxPasteImporting.ItemsSource = Enum.GetValues(typeof(ImportingChoice));
			ComboboxPasteImporting.SelectedItem = Config.Instance.PasteImportingChoice;
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
			if(TemplateEditable)
			{
				Config.Instance.ArenaDeckNameTemplate = TextBoxArenaTemplate.Text;
				Config.Save();
			}
			EditButtonText = LocUtil.Get(TemplateEditable ? LocEdit : LocSave);
			TemplateEditable = !TemplateEditable;
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

		private void ComboboxPasteImporting_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PasteImportingChoice = (ImportingChoice)ComboboxPasteImporting.SelectedItem;
			Config.Save();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
