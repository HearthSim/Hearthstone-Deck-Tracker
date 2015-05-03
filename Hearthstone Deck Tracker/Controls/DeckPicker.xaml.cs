#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckPicker.xaml
	/// </summary>
	public partial class DeckPicker
	{
		private bool _wasClicked;
		private bool _wasDoubleClicked;

		public DeckPicker()
		{
			InitializeComponent();

			SelectedTags = new List<string>();

			SelectedDeck = null;
			_hsClasses = new List<HsClass>();
			foreach(var className in _classNames)
				_hsClasses.Add(new HsClass(className));
			_hsClasses.Add(new HsClass("Undefined"));

			ListboxPicker.Items.Add(new HsClass("All"));
			foreach(var hsClass in _hsClasses)
				ListboxPicker.Items.Add(hsClass);
			_inClassSelect = true;
			_initialized = true;
			ShowAll = false;
		}

		public void AddDeck(Deck deck)
		{
			if(deck == null)
				return;
			var hsClass = _hsClasses.FirstOrDefault(c => c.Name == deck.Class) ?? _hsClasses.First(c => c.Name == "Undefined");
			hsClass.Decks.Add(deck);
		}

		public void AddAndSelectDeck(Deck deck)
		{
			if(deck == null)
				return;
			AddDeck(deck);
			SelectDeck(deck);
		}

		public void SelectDeck(Deck deck)
		{
			if(deck == null)
				return;
			ChangedSelection = true;
			var hsClass = _hsClasses.FirstOrDefault(c => c.Name == deck.Class) ?? _hsClasses.First(c => c.Name == "Undefined");

			if(hsClass != null)
			{
				_selectedClass = hsClass;
				ListboxPicker.Items.Clear();
				ListboxPicker.Items.Add(new HsClass("Back"));
				if(ShowAll)
				{
					foreach(var d in _hsClasses.SelectMany(hsc => hsc.Decks))
					{
						if(DeckMatchesSelectedTags(d))
							ListboxPicker.Items.Add(d);
					}
				}
				else
				{
					foreach(var d in hsClass.Decks)
					{
						if(DeckMatchesSelectedTags(d))
							ListboxPicker.Items.Add(d);
					}
				}
				ListboxPicker.SelectedItem = deck;
				_inClassSelect = false;
				SortDecks();
			}

			SelectedDeck = deck;
			ChangedSelection = false;
		}

		private bool DeckMatchesSelectedTags(Deck deck)
		{
			return SelectedTags.Any(t => t == "All")
			       || (TagOperation == TagFilerOperation.Or
				           ? SelectedTags.Any(t => deck.Tags.Contains(t) || t == "None" && deck.Tags.Count == 0)
				           : SelectedTags.All(t => deck.Tags.Contains(t) || t == "None" && deck.Tags.Count == 0));
		}

		public void RemoveDeck(Deck deck)
		{
			if(deck == null)
				return;
			var hsClass = _hsClasses.FirstOrDefault(c => c.Decks.Contains(deck));
			if(hsClass != null)
				hsClass.Decks.Remove(deck);
			if(ListboxPicker.Items.Contains(deck))
				ListboxPicker.Items.Remove(deck);
		}

		private void ListboxPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(e.AddedItems == null || e.AddedItems.Count == 0)
				return;
			if(!_initialized)
				return;
			ChangedSelection = true;
			var selectedClass = e.AddedItems[0] as HsClass;
			if(selectedClass != null)
			{
				if(_inClassSelect)
				{
					ShowAll = selectedClass.Name == "All";

					_selectedClass = selectedClass;

					ListboxPicker.Items.Clear();
					ListboxPicker.Items.Add(new HsClass("Back"));

					if(ShowAll)
					{
						foreach(var d in _hsClasses.SelectMany(hsc => hsc.Decks))
						{
							if(DeckMatchesSelectedTags(d))
								ListboxPicker.Items.Add(d);
						}
					}
					else
					{
						foreach(var d in selectedClass.Decks)
						{
							if(DeckMatchesSelectedTags(d))
								ListboxPicker.Items.Add(d);
						}
					}
					_inClassSelect = false;
					SortDecks();
				}
				else if(selectedClass.Name == "Back")
				{
					_selectedClass = null;
					ListboxPicker.Items.Clear();
					ListboxPicker.Items.Add(new HsClass("All"));
					foreach(var hsClass in _hsClasses)
						ListboxPicker.Items.Add(hsClass);
					_inClassSelect = true;
				}
			}
			else
			{
				var newSelectedDeck = e.AddedItems[0] as Deck;
				if(newSelectedDeck == null)
				{
					ChangedSelection = false;
					return;
				}

				if(Equals(newSelectedDeck, SelectedDeck))
				{
					ChangedSelection = false;
					return;
				}

				if(OnSelectedDeckChanged != null)
					OnSelectedDeckChanged(this, newSelectedDeck);
				if(SelectedDeck != null)
					SelectedDeck.IsSelectedInGui = false;
				newSelectedDeck.IsSelectedInGui = true;
				ListboxPicker.Items.Refresh();


				SelectedDeck = newSelectedDeck;
			}
			ChangedSelection = false;
		}

		internal void SetSelectedTags(List<string> tags)
		{
			SelectedTags = tags;

			foreach(var hsClass in _hsClasses)
				hsClass.SelectedTags = tags;

			UpdateList();
		}

		public void UpdateList()
		{
			if(!_inClassSelect)
			{
				ListboxPicker.Items.Clear();
				ListboxPicker.Items.Add(new HsClass("Back"));
				if(ShowAll)
				{
					foreach(var d in _hsClasses.SelectMany(hsc => hsc.Decks))
					{
						if(DeckMatchesSelectedTags(d))
							ListboxPicker.Items.Add(d);
					}
				}
				else
				{
					foreach(var d in _selectedClass.Decks)
					{
						if(DeckMatchesSelectedTags(d))
							ListboxPicker.Items.Add(d);
					}
				}
				SortDecks();
				Console.WriteLine("UPDATE - SORT");
			}
			else
			{
				_selectedClass = null;
				ListboxPicker.Items.Clear();
				ListboxPicker.Items.Add(new HsClass("All"));
				foreach(var hsClass in _hsClasses)
					ListboxPicker.Items.Add(hsClass);
				_inClassSelect = true;
			}
		}

		public void SetTagOperation(TagFilerOperation o)
		{
			TagOperation = o;
			foreach(var hsClass in _hsClasses)
				hsClass.TagOperation = o;
		}

		public void SortDecks()
		{
			if(_inClassSelect)
				return;
			var returnButton = ListboxPicker.Items.GetItemAt(0);
			var orderedDecks = ListboxPicker.Items.OfType<Deck>().ToList();

			ListboxPicker.Items.Clear();
			ListboxPicker.Items.Add(returnButton);

			switch(Config.Instance.SelectedDeckSorting)
			{
				case "Name":
					orderedDecks = orderedDecks.OrderBy(x => x.Name).ToList();
					break;
				case "Last Edited":
					orderedDecks = orderedDecks.OrderByDescending(x => x.LastEdited).ToList();
					break;
				case "Tag":
					orderedDecks = orderedDecks.OrderBy(x => x.TagList).ToList();
					break;
				case "Win Rate":
					orderedDecks = orderedDecks.OrderByDescending(x => x.WinPercent).ToList();
					break;
			}

			//sort by class if in "All"
			if(ShowAll)
				orderedDecks = orderedDecks.OrderBy(x => x.GetClass).ToList();

			foreach(var deck in orderedDecks)
				ListboxPicker.Items.Add(deck);
		}

		private async void DeckPickerItem_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(_wasClicked)
				return;
			_wasClicked = true;
			await Task.Delay(SystemInformation.DoubleClickTime);
			_wasClicked = false;
			if(_wasDoubleClicked)
			{
				_wasDoubleClicked = false;
				return;
			}
			var deckPickerItem = (sender as DeckPickerItem);
			if(deckPickerItem == null)
				return;
			var deck = deckPickerItem.DataContext as Deck;
			if(deck == null)
				return;
			if(Equals(deck, SelectedDeck))
			{
				Helper.MainWindow.SelectDeck(null);
				e.Handled = true;
			}
		}

		internal Deck GetSelectedDeckVersion()
		{
			return SelectedDeck != null ? SelectedDeck.GetSelectedDeckVersion() : null;
		}

		private void Control_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			_wasDoubleClicked = true;
			if(OnDoubleClick != null)
				OnDoubleClick(this, SelectedDeck);
		}

		#region HsClass

		public class HsClass
		{
			public List<Deck> Decks;
			public string Name;
			public List<string> SelectedTags;
			public TagFilerOperation TagOperation;

			public HsClass(string name)
			{
				Name = name;
				Decks = new List<Deck>();
				SelectedTags = new List<string>();
			}

			public string TagList
			{
				get { return ""; }
			}

			public string GetName
			{
				get
				{
					return (Name == "Back" || Name == "All")
						       ? Name
						       : Name + " ("
						         + Decks.Count(
						                       d =>
						                       SelectedTags.Any(t => t == "All")
						                       || (TagOperation == TagFilerOperation.Or
							                           ? SelectedTags.Any(t => d.Tags.Contains(t) || t == "None" && d.Tags.Count == 0)
							                           : SelectedTags.All(t => d.Tags.Contains(t) || t == "None" && d.Tags.Count == 0))) + ")";
				}
			}

			public string WinPercentString
			{
				get
				{
					if(Name == "Back" || Name == "All")
						return "win%";
					var filteredDecks =
						Decks.Where(d => Config.Instance.SelectedTags.Any(t => t == "All" || d.Tags.Contains(t) || t == "None" && d.Tags.Count == 0))
						     .ToList();
					var total = filteredDecks.Sum(d => d.DeckStats.Games.Count);
					if(total == 0)
						return "-%";
					return Math.Round(100.0 * filteredDecks.Sum(d => d.DeckStats.Games.Count(g => g.Result == GameResult.Win)) / total, 0) + "%";
				}
			}

			// ReSharper disable PossibleNullReferenceException
			public Color ClassColor
			{
				get
				{
					switch(Name)
					{
						case "Druid":
							return (Color)ColorConverter.ConvertFromString("#FF7D0A");
						case "Death Knight":
							return (Color)ColorConverter.ConvertFromString("#C41F3B");
						case "Hunter":
							return (Color)ColorConverter.ConvertFromString("#ABD473");
						case "Mage":
							return (Color)ColorConverter.ConvertFromString("#69CCF0");
						case "Monk":
							return (Color)ColorConverter.ConvertFromString("#00FF96");
						case "Paladin":
							return (Color)ColorConverter.ConvertFromString("#F58CBA");
						case "Priest":
							return (Color)ColorConverter.ConvertFromString("#FFFFFF");
						case "Rogue":
							return (Color)ColorConverter.ConvertFromString("#FFF569");
						case "Shaman":
							return (Color)ColorConverter.ConvertFromString("#0070DE");
						case "Warlock":
							return (Color)ColorConverter.ConvertFromString("#9482C9");
						case "Warrior":
							return (Color)ColorConverter.ConvertFromString("#C79C6E");
						default:
							return Colors.Gray;
					}
				}
			}

			// ReSharper restore PossibleNullReferenceException
			public FontWeight GetFontWeight
			{
				get { return FontWeights.Bold; }
			}
		}

		#endregion

		#region Properties

		public delegate void DoubleClickHandler(DeckPicker sender, Deck deck);

		public delegate void SelectedDeckHandler(DeckPicker sender, Deck deck);


		private readonly List<string> _classNames = new List<string>
		{
			"Druid",
			"Hunter",
			"Mage",
			"Paladin",
			"Priest",
			"Rogue",
			"Shaman",
			"Warlock",
			"Warrior"
		};

		private readonly List<HsClass> _hsClasses;
		private readonly bool _initialized;
		public Deck SelectedDeck;
		public List<string> SelectedTags;
		public bool ShowAll;
		public TagFilerOperation TagOperation;
		private bool _inClassSelect;
		private HsClass _selectedClass;

		public string GetSelectedClass
		{
			get { return _selectedClass != null ? _selectedClass.Name : "None"; }
		}

		public bool ChangedSelection { get; set; }

		public event SelectedDeckHandler OnSelectedDeckChanged;
		public event DoubleClickHandler OnDoubleClick;

		#endregion
	}
}