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
		private string _arenaEditButtonText = LocUtil.Get(LocEdit);
		private bool _arenaTemplateEditable;
		private string _dungeonEditButtonText = LocUtil.Get(LocEdit);
		private bool _dungeonTemplateEditable;
		private const string LocEdit = "Options_Tracker_Importing_Button_Edit";
		private const string LocSave = "Options_Tracker_Importing_Button_Save";

		public TrackerImporting()
		{
			InitializeComponent();
		}

		public string ArenaEditButtonText
		{
			get => _arenaEditButtonText;
			set
			{
				_arenaEditButtonText = value;
				OnPropertyChanged();
			}
		}

		public bool ArenaTemplateEditable
		{
			get => _arenaTemplateEditable;
			set
			{
				_arenaTemplateEditable = value;
				OnPropertyChanged();
			}
		}

		public string DungeonEditButtonText
		{
			get => _dungeonEditButtonText;
			set
			{
				_dungeonEditButtonText = value; 
				OnPropertyChanged();
			}
		}

		public bool DungeonTemplateEditable
		{
			get => _dungeonTemplateEditable;
			set
			{
				_dungeonTemplateEditable = value; 
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
			CheckboxAutoSaveOnImport.IsChecked = Config.Instance.AutoSaveOnImport;
			TextBoxArenaTemplate.Text = Config.Instance.ArenaDeckNameTemplate;
			CheckBoxConstructedImportNew.IsChecked = Config.Instance.ConstructedAutoImportNew;
			CheckBoxConstrucedUpdate.IsChecked = Config.Instance.ConstructedAutoUpdate;
			TextBoxDungeonTemplate.Text = Config.Instance.DungeonRunDeckNameTemplate;
			CheckBoxDungeonImport.IsChecked = Config.Instance.DungeonAutoImport;
			CheckBoxDungeonIncludePassives.IsChecked = Config.Instance.DungeonRunIncludePassiveCards;
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

		private void BtnEditTemplate_ClickDungeon(object sender, RoutedEventArgs e)
		{
			if(DungeonTemplateEditable)
			{
				Config.Instance.DungeonRunDeckNameTemplate = TextBoxDungeonTemplate.Text;
				Config.Save();
			}
			DungeonEditButtonText = LocUtil.Get(DungeonTemplateEditable ? LocEdit : LocSave);
			DungeonTemplateEditable = !DungeonTemplateEditable;
		}

		private void TextBoxDungeonTemplate_OnTextChanged(object sender, TextChangedEventArgs e) 
			=> TextBlockNamePreviewDungeon.Text = Helper.ParseDeckNameTemplate(TextBoxDungeonTemplate.Text, new Deck() {Class = "ClassName"});

		private void BtnEditTemplate_ClickArena(object sender, RoutedEventArgs e)
		{
			if(ArenaTemplateEditable)
			{
				Config.Instance.ArenaDeckNameTemplate = TextBoxArenaTemplate.Text;
				Config.Save();
			}
			ArenaEditButtonText = LocUtil.Get(ArenaTemplateEditable ? LocEdit : LocSave);
			ArenaTemplateEditable = !ArenaTemplateEditable;
		}

		private void TextBoxArenaTemplate_OnTextChanged(object sender, TextChangedEventArgs e) 
			=> TextBlockNamePreviewArena.Text = Helper.ParseDeckNameTemplate(TextBoxArenaTemplate.Text, new Deck() {Class = "ClassName"});

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

		private void CheckBoxDungeonImport_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DungeonAutoImport = true;
			Config.Save();
		}

		private void CheckBoxDungeonImport_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DungeonAutoImport = false;
			Config.Save();
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

		private void CheckBoxDungeonIncludePassives_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DungeonRunIncludePassiveCards = true;
			Config.Save();
		}

		private void CheckBoxDungeonIncludePassives_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DungeonRunIncludePassiveCards = false;
			Config.Save();
		}
	}
}
