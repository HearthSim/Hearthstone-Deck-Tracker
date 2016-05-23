using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Importing.Game.ImportOptions;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class DeckImportingControl : INotifyPropertyChanged
	{
		private const string StartTextConstructed = "Start Hearthstone and enter the 'Play' menu.";
		private const string StartTextConstructedGameRunning = "Enter the 'Play' menu.";
		private const string StartTextBrawl = "Start Hearthstone and enter the 'Tavern Brawl' menu.";
		private const string StartTextBrawlGameRunning = "Enter the 'Tavern Brawl' menu.";
		private const string NoDecksFoundText = "No new decks found.";
		private const string StartHearthstoneText = "START LAUNCHER / HEARTHSTONE";
		private const string StartHearthstoneWaitingText = "WAITING FOR HEARTHSTONE...";

		private string StartText => _brawl ? StartTextBrawl : StartTextConstructed;
		private string StartTextGameRunning => _brawl ? StartTextBrawlGameRunning : StartTextConstructedGameRunning;


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
			ButtonStartHearthstoneText = StartHearthstoneText;
		}

		public void SetDecks(List<ImportedDeck> decks)
		{
			Decks.Clear();
			foreach(var deck in decks)
				Decks.Add(deck);
			_ready = decks.Any();
			Text = NoDecksFoundText;
			UpdateContent();
		}

		private void UpdateContent()
		{
			OnPropertyChanged(nameof(TextVisibility));
			OnPropertyChanged(nameof(ContentVisibility));
			OnPropertyChanged(nameof(StartButtonVisibility));
		}

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
			foreach(var deck in Decks.Where(x => x.Import))
			{
				if(deck.SelectedImportOption is NewDeck)
				{
					Log.Info($"Saving {deck.Deck.Name} as new deck.");
					var newDeck = new Deck
					{
						Class = deck.Class,
						Name = deck.Deck.Name,
						HsId = deck.Deck.Id,
						Cards = new ObservableCollection<Hearthstone.Card>(deck.Deck.Cards.Select(x =>
						{
							var card = Database.GetCardFromId(x.Id);
							card.Count = x.Count;
							return card;
						})),
						IsArenaDeck = false
					};
					if(_brawl)
						newDeck.Tags.Add("Brawl");
					DeckList.Instance.Decks.Add(newDeck);
				}
				else
				{
					var existing = deck.SelectedImportOption as ExistingDeck;
					if(existing == null)
						return;
					var target = existing.Deck;
					target.HsId = deck.Deck.Id;
					if(_brawl && !target.Tags.Any(x => x.ToUpper().Contains("BRAWL")))
						target.Tags.Add("Brawl");
					if(existing.NewVersion.Major == 0)
						Log.Info($"Assinging id to existing deck: {deck.Deck.Name}.");
					else
					{
						Log.Info($"Saving {deck.Deck.Name} as {existing.NewVersion.ShortVersionString} (prev={target.Version.ShortVersionString}).");
						DeckList.Instance.Decks.Remove(target);
						var oldDeck = (Deck)target.Clone();
						oldDeck.Versions = new List<Deck>();
						target.Name = deck.Deck.Name;
						target.LastEdited = DateTime.Now;
						target.Versions.Add(oldDeck);
						target.Version = existing.NewVersion;
						target.SelectedVersion = existing.NewVersion;
						target.HearthStatsDeckVersionId = "";
						target.Cards.Clear();
						var cards = deck.Deck.Cards.Select(x =>
						{
							var card = Database.GetCardFromId(x.Id);
							card.Count = x.Count;
							return card;
						});
						foreach(var card in cards)
							target.Cards.Add(card);
						DeckList.Instance.Decks.Add((Deck)target.Clone());
					}
				}
			}
			DeckList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			Core.MainWindow.UpdateIntroLabelVisibility();
			Core.UpdatePlayerCards(true);
			Core.MainWindow.FlyoutDeckImporting.IsOpen = false;
		}

		private async void BtnStartHearthstone_Click(object sender, RoutedEventArgs e)
		{
			BtnStartHearthstone.IsEnabled = false;
			ButtonStartHearthstoneText = StartHearthstoneWaitingText;
			Helper.StartHearthstoneAsync().Forget();
			await Task.Delay(5000);
			BtnStartHearthstone.IsEnabled = true;
		}
	}
}