using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor
{
	public class DeckEditorViewModel : ViewModel
	{
		private string? _searchText;
		private readonly List<Card>? _allCards;
		private Deck _deck;
		private Deck? _originalDeck;
		private bool _includeWild;
		private bool _constructedCardLimits;
		private CostFilter _selectedCostFilter = CostFilter.All;
		private ClassFilter _selectedClassFilter = ClassFilter.All;
		private SetFilter _selectedSetFilter = SetFilter.ALL;
		private DeckEditorErrors _errors;
		private DeckEditorWarnings _warnings;
		private SaveOperation? _selectedSaveOperation;
		private SaveOperation[]? _saveOperations;
		private Visibility _saveOperationSelectionVisibility;
		private int _selectedDbIndex;
		private string? _selectedSearchText;

		public Action? DbInputFocusRequest { get; set; }

		public DeckEditorViewModel()
		{
			_allCards = Database.GetActualCards().OrderBy(c => c.Cost).ThenBy(c => c.LocalizedName).ToList();
			_deck = new Deck();
			_includeWild = Config.Instance.CardDbIncludeWildOnlyCards;
			_constructedCardLimits = !_deck.IsArenaDeck;
		}

		public string? SearchText
		{
			get => _searchText;
			set
			{
				if(value != _searchText)
				{
					_searchText = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(CardDatabase));
					SelectedDbIndex = 0;
				}
			}
		}

		public IEnumerable<Card> Cards => Deck.Cards;

		public void SetDeck(Deck deck, bool isNewDeck)
		{
			Deck = deck;
			var hasSaveOp = !isNewDeck && !deck.IsArenaDeck;
			SelectedSaveOperation = hasSaveOp ? SaveOperations?[1] : null;
			SaveOperationSelectionVisibility = hasSaveOp ? Visibility.Visible : Visibility.Collapsed;
		}

		public Deck Deck
		{
			get => _deck;
			set
			{
				_originalDeck = value;
				_deck = (Deck)value.Clone();
				SetCards(value.GetSelectedDeckVersion().Cards);
				OnPropertyChanged();
				OnPropertyChanged(nameof(DeckName));
				OnPropertyChanged(nameof(CardDatabase));
				OnPropertyChanged(nameof(ConstructedCardLimitsVisibility));
				UpdateDeckNameError();
				UpdateNameExistsWarning();
				SaveOperations = new[]
				{
					SaveOperation.Current(value),
					SaveOperation.MinorIncrement(value),
					SaveOperation.MajorIncrement(value)
				};
				SelectedDbIndex = 0;
				SearchText = string.Empty;
				Helper.SortCardCollection(Cards);
			}
		}

		public void SetCards(IEnumerable<Card> cards)
		{
			_deck.Cards.Clear();
			foreach(var card in cards)
				_deck.Cards.Add((Card)card.Clone());
			OnPropertyChanged(nameof(Cards));
			OnPropertyChanged(nameof(CardCount));
			UpdateCardCountWarning();
		}

		public IEnumerable<Card> CardDatabase
		{
			get
			{
				if(_allCards == null)
					return new List<Card>();

				var tourist = Cards.Select(x => x.GetTouristClass()).WhereNotNull().FirstOrDefault();
				var cards = _allCards.Where(x => x.IsClass(Deck.Class) || x.IsNeutral || (tourist != null && x.IsClass(tourist) && x.CanBeVisitedByTourist));
				if(!string.IsNullOrEmpty(SearchText))
				{
					var input = CleanString(SearchText!).ToLowerInvariant();
					cards = cards.Where(c => Matches(c, input) || FullTextSearch && FullMatch(c, input));
				}
				if(SelectedCostFilter != CostFilter.All)
					cards = cards.Where(c => Math.Min(c.Cost, 9) == (int)SelectedCostFilter);
				if(SelectedClassFilter != ClassFilter.All)
					cards = cards.Where(
						SelectedClassFilter == ClassFilter.TouristOnly
							? c => c.IsTourist || (tourist != null && c.IsClass(tourist))
							: c => c.IsClassCard == (SelectedClassFilter == ClassFilter.ClassOnly) && (tourist == null || c.IsClass(Deck.Class))
					);
				if(SelectedSetFilter != SetFilter.ALL)
					cards = cards.Where(c => c.CardSet.HasValue && (int)c.CardSet.Value == (int)SelectedSetFilter);
				if(!IncludeWild || Deck.IsArenaDeck)
					cards = cards.Where(c => !Helper.WildOnlySets.Contains(c.Set));

				return cards;
			}
		}

		private bool Matches(Card card, string searchStr)
		{
			if(card.LocalizedName != null && Helper.RemoveDiacritics(card.LocalizedName, true).ToLowerInvariant().Contains(searchStr))
				return true;
			return card.Race?.ToLowerInvariant().Contains(searchStr) ?? false;
		}

		private bool FullMatch(Card card, string searchStr)
		{
			var words = searchStr.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
			return words.Any(card.Text.Contains);
		}

		private string CleanString(string str) => string.IsNullOrEmpty(str) ? string.Empty : Helper.RemoveDiacritics(str, true).ToLowerInvariant();

		public ICommand AddCardCommand => new Command<Card>(AddCardToDeck);

		public ICommand RemoveCardCommand => new Command<Card>(RemoveCardFromDeck);

		// Todo: replace this with format dropdown
		public bool IncludeWild
		{
			get => _includeWild;
			set
			{
				_includeWild = value;
				Config.Instance.CardDbIncludeWildOnlyCards = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardDatabase));
			}
		}

		public bool ConstructedCardLimits
		{
			get => _constructedCardLimits; set
			{
				_constructedCardLimits = value;
				OnPropertyChanged();
			}
		}

		public string CardCount => $"{Deck.Cards.Sum(x => x.Count)} / {(Cards.Any(x => x.Id is HearthDb.CardIds.Collectible.Neutral.PrinceRenathal or HearthDb.CardIds.Collectible.Neutral.PrinceRenathalCorePlaceholder) ? 40 : 30)}";

		public CostFilter SelectedCostFilter
		{
			get => _selectedCostFilter; set
			{
				_selectedCostFilter = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardDatabase));
			}
		}

		public ClassFilter SelectedClassFilter
		{
			get => _selectedClassFilter;
			set
			{
				_selectedClassFilter = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardDatabase));
			}
		}

		public SetFilter SelectedSetFilter
		{
			get => _selectedSetFilter;
			set
			{
				_selectedSetFilter = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardDatabase));
			}
		}

		public SaveOperation? SelectedSaveOperation
		{
			get => _selectedSaveOperation;
			set
			{
				_selectedSaveOperation = value;
				OnPropertyChanged();
			}
		}

		public SaveOperation[]? SaveOperations
		{
			get => _saveOperations;
			set
			{
				_saveOperations = value;
				OnPropertyChanged();
			}
		}

		public Visibility SaveOperationSelectionVisibility
		{
			get => _saveOperationSelectionVisibility; set
			{
				_saveOperationSelectionVisibility = value;
				OnPropertyChanged();
			}
		}

		public string DeckName
		{
			get => Deck.Name;
			set
			{
				Deck.Name = value;
				OnPropertyChanged();
				UpdateDeckNameError();
				UpdateNameExistsWarning();
			}
		}

		public void UpdateNameExistsWarning()
		{
			if(!string.IsNullOrEmpty(DeckName) &&
				DeckList.Instance.Decks.Any(d => d.Name == DeckName && d.DeckId != Deck.DeckId))
				Warnings |= DeckEditorWarnings.NameAlreadyExists;
			else
				Warnings &= ~DeckEditorWarnings.NameAlreadyExists;
		}

		public Visibility ConstructedCardLimitsVisibility => Deck.IsArenaDeck ? Visibility.Collapsed : Visibility.Visible;

		public ICommand SaveCommand => new Command(SaveDeck);

		public bool CanSave => Errors == 0;

		private void SaveDeck()
		{
			if(SelectedSaveOperation != null)
			{
				if(!SelectedSaveOperation.IsCurrent && SelectedSaveOperation.Version != null)
					Deck.Version = SelectedSaveOperation.Version;
				if(_originalDeck != null)
					DeckManager.SaveDeck(_originalDeck, Deck, SelectedSaveOperation.IsCurrent);
			}
			else
				DeckManager.SaveDeck(Deck);

			// TODO: Find a better way to interact with the MainWindow
			Core.MainWindow.FlyoutDeckEditor.IsOpen = false;
		}

		public DeckEditorErrors Errors
		{
			get => _errors;
			set
			{
				if(value != _errors)
				{
					_errors = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(ErrorMessages));
					OnPropertyChanged(nameof(CanSave));
				}
			}
		}

		public DeckEditorWarnings Warnings
		{
			get => _warnings;
			set
			{
				if(value != _warnings)
				{
					_warnings = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(WarningMessages));
				}
			}
		}

		public IEnumerable<string> ErrorMessages
		{
			get
			{
				foreach(var error in Enum.GetValues(typeof(DeckEditorErrors)).OfType<DeckEditorErrors>())
				{
					if(((int)Errors & (int)error) == (int)error)
						yield return EnumDescriptionConverter.GetDescription(error);
				}
			}
		}

		public IEnumerable<string> WarningMessages
		{
			get
			{
				foreach(var warning in Enum.GetValues(typeof(DeckEditorWarnings)).OfType<DeckEditorWarnings>())
				{
					if(((int)Warnings & (int)warning) == (int)warning)
						yield return EnumDescriptionConverter.GetDescription(warning);
				}
			}
		}

		public bool FullTextSearch
		{
			get => Config.Instance.UseFullTextSearch;
			set
			{
				Config.Instance.UseFullTextSearch = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardDatabase));
			}
		}

		public ICommand AddCardByIndexCommand => new Command<object>(value =>
		{
			if(!int.TryParse(value.ToString(), out var index))
				return;
			var card = CardDatabase.ElementAtOrDefault(index);
			if(card != null)
				AddCardToDeck(card);
		});

		public int SelectedDbIndex
		{
			get => _selectedDbIndex; set
			{
				_selectedDbIndex = value;
				OnPropertyChanged();
			}
		}

		public ICommand MoveSelectionCommand => new Command<string>(value =>
		{
			if(!int.TryParse(value, out var numValue))
				return;
			if(SelectedDbIndex > 0 && numValue < 0 || numValue > 0)
				SelectedDbIndex += numValue;
		});

		public string? SelectedSearchText
		{
			get => _selectedSearchText;
			set
			{
				_selectedSearchText = value;
				OnPropertyChanged();
			}
		}

		private void RemoveCardFromDeck(Card card)
		{
			if(Deck == null || card == null)
				return;
			card.Count--;
			if(card.Count <= 0)
				Deck.Cards.Remove(card);
			OnPropertyChanged(nameof(Cards));
			OnPropertyChanged(nameof(Deck));
			OnPropertyChanged(nameof(CardCount));
			OnPropertyChanged(nameof(CardDatabase)); // recalculate for tourist
			UpdateCardCountWarning();
			UpdateTouristWarning();
		}

		private void AddCardToDeck(Card card)
		{
			if(Deck == null || card == null)
				return;
			var existing = Deck.Cards.FirstOrDefault(c => c.Id == card.Id);
			if(existing == null)
			{
				existing = (Card)card.Clone();
				existing.Count = 0;
				Deck.Cards.Add(existing);
			}
			else if(ConstructedCardLimits && !Deck.IsArenaDeck)
			{
				if(existing.Count > 1 || existing.Count > 0 && existing.Rarity == Rarity.LEGENDARY)
					return;
			}

			existing.Count++;
			OnPropertyChanged(nameof(Cards));
			OnPropertyChanged(nameof(Deck));
			OnPropertyChanged(nameof(CardCount));
			OnPropertyChanged(nameof(CardDatabase)); // recalculate for tourist
			UpdateCardCountWarning();
			UpdateTouristWarning();
			SelectedSearchText = SearchText;
			DbInputFocusRequest?.Invoke();
		}

		private void UpdateCardCountWarning()
		{
			var count = Cards.Sum(x => x.Count);
			if(Cards.Any(x => x.Id is HearthDb.CardIds.Collectible.Neutral.PrinceRenathal or HearthDb.CardIds.Collectible.Neutral.PrinceRenathalCorePlaceholder))
			{
				Warnings &= ~(DeckEditorWarnings.LessThan30Cards | DeckEditorWarnings.MoreThan30Cards);
				if(count == 40)
					Warnings &= ~(DeckEditorWarnings.LessThan40Cards | DeckEditorWarnings.MoreThan40Cards);
				else if(count > 40)
					Warnings |= DeckEditorWarnings.MoreThan40Cards;
				else
					Warnings |= DeckEditorWarnings.LessThan40Cards;
			}
			else
			{
				Warnings &= ~(DeckEditorWarnings.LessThan40Cards | DeckEditorWarnings.MoreThan40Cards);
				if(count == 30)
					Warnings &= ~(DeckEditorWarnings.LessThan30Cards | DeckEditorWarnings.MoreThan30Cards);
				else if(count > 30)
					Warnings |= DeckEditorWarnings.MoreThan30Cards;
				else
					Warnings |= DeckEditorWarnings.LessThan30Cards;
			}
		}

		private void UpdateTouristWarning()
		{
			Warnings &= ~DeckEditorWarnings.MissingTourist;
			var tourist = Cards.Select(x => x.GetTouristClass()).WhereNotNull().FirstOrDefault();
			if(tourist != null)
				return;
			if(Cards.Any(x => !x.IsClass(Deck.Class) && !x.IsNeutral && x.CanBeVisitedByTourist))
			{
				Warnings |= DeckEditorWarnings.MissingTourist;
			}
		}

		private void UpdateDeckNameError()
		{
			if(string.IsNullOrEmpty(Deck?.Name))
				Errors |= DeckEditorErrors.NameRequired;
			else
				Errors &= ~DeckEditorErrors.NameRequired;
		}
	}
}
