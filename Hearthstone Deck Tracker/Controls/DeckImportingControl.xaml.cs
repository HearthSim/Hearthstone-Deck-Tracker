using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using static System.Windows.Visibility;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class DeckImportingControl : INotifyPropertyChanged
	{
		private const string StartTextConstructed = "Importing_Constructed_Text_StartHearthstonePlay";
		private const string StartTextConstructedGameRunning = "Importing_Constructed_Text_EnterPlay";
		private const string StartTextBrawl = "Importing_Constructed_Text_StartHearthstoneBrawl";
		private const string StartTextBrawlGameRunning = "Importing_Constructed_Text_EnterBrawl";
		private const string NoDecksFoundText = "Importing_Constructed_Text_NoDecksFound";
		private const string StartHearthstoneText = "Importing_Constructed_Button_StartHearthstone";
		private const string StartHearthstoneWaitingText = "Importing_Constructed_Button_Waiting";

		private string StartText => LocUtil.Get(_brawl ? StartTextBrawl : StartTextConstructed);
		private string StartTextGameRunning => LocUtil.Get(_brawl ? StartTextBrawlGameRunning : StartTextConstructedGameRunning);


		private bool _brawl;
		private bool _ready;
		private string _text;

		public DeckImportingControl()
		{
			InitializeComponent();
			DataContext = this;
		}

		public Visibility TextVisibility => _ready ? Collapsed : Visible;
		public Visibility ContentVisibility => _ready ? Visible : Collapsed;
		public Visibility StartButtonVisibility => Core.Game.IsRunning ? Collapsed : Visible;
		public Visibility AutoImportingVisibility => _brawl || Config.Instance.ConstructedAutoImportNew ? Collapsed : Visible;

		public string Text
		{
			get { return _text; }
			set
			{
				if(_text == value)
					return;
				_text = value;
				OnPropertyChanged();
			}
		}

		public string ButtonStartHearthstoneText
		{
			get { return _text; }
			set
			{
				if(_text == value)
					return;
				_text = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<ImportedDeck> Decks { get; } = new ObservableCollection<ImportedDeck>();


		public event PropertyChangedEventHandler PropertyChanged;

		public void Reset(bool brawl)
		{
			_brawl = brawl;
			_ready = false;
			Text = Core.Game.IsRunning ? StartTextGameRunning : StartText;
			UpdateContent();
			ButtonImport.IsEnabled = true;
			ButtonStartHearthstoneText = LocUtil.Get(StartHearthstoneText, true);
		}

		public void SetDecks(List<ImportedDeck> decks)
		{
			Decks.Clear();
			var import = CheckBoxImportAll.IsChecked == true;
			foreach(var deck in decks)
			{
				deck.Import = import;
				Decks.Add(deck);
			}
			_ready = decks.Any();
			Text = LocUtil.Get(NoDecksFoundText);
			UpdateContent();
		}

		private void UpdateContent()
		{
			OnPropertyChanged(nameof(TextVisibility));
			OnPropertyChanged(nameof(ContentVisibility));
			OnPropertyChanged(nameof(StartButtonVisibility));
		}

		private CheckBox GetImportCheckbox(object item)
		{
			var c = (ContentPresenter)ItemsControl.ItemContainerGenerator.ContainerFromItem(item);
			return (CheckBox)c.ContentTemplate.FindName("CheckBoxImport", c);
		}

		private void CheckBoxImportAll_OnClicked(object sender, RoutedEventArgs e)
		{
			foreach(var item in ItemsControl.Items)
				GetImportCheckbox(item).IsChecked = CheckBoxImportAll.IsChecked.HasValue && CheckBoxImportAll.IsChecked.Value;
		}

		private void CheckBoxImport_OnChecked(object sender, RoutedEventArgs e)
		{
			var modifiedCount = 0;
			foreach(var item in ItemsControl.Items)
			{
				if(GetImportCheckbox(item).IsChecked == true)
					modifiedCount++;
			}

			if (modifiedCount == ItemsControl.Items.Count)
				CheckBoxImportAll.IsChecked = true;
		}

		private void CheckBoxImport_OnUnchecked(object sender, RoutedEventArgs e) => CheckBoxImportAll.IsChecked = false;

		public void StartedGame()
		{
			Text = StartTextGameRunning;
			UpdateContent();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
		{
			ButtonImport.IsEnabled = false;
			ImportDecks();
		}

		private void ImportDecks()
		{
			DeckManager.ImportDecks(Decks.Where(x => x.Import), _brawl);
			Core.MainWindow.FlyoutDeckImporting.IsOpen = false;
		}

		private async void BtnStartHearthstone_Click(object sender, RoutedEventArgs e)
		{
			BtnStartHearthstone.IsEnabled = false;
			ButtonStartHearthstoneText = LocUtil.Get(StartHearthstoneWaitingText, true);
			HearthstoneRunner.StartHearthstone().Forget();
			await Task.Delay(5000);
			BtnStartHearthstone.IsEnabled = true;
		}

		private void ButtonAutoImport_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.Options.TreeViewItemTrackerImporting.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
		}
	}
}
