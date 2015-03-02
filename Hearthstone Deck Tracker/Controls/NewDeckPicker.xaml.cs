#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;

#endregion

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for NewDeckPicker.xaml
	/// </summary>
	public partial class NewDeckPicker
	{
		public delegate void DoubleClickHandler(NewDeckPicker sender, Deck deck);

		public delegate void SelectedDeckHandler(NewDeckPicker sender, Deck deck);

		private readonly ObservableCollection<NewDeckPickerItem> _displayedDecks;
		public bool ChangedSelection;
		private bool _ignoreSelectionChange;
		private bool _refillingList;
		private bool _reselecting;

		public NewDeckPicker()
		{
			InitializeComponent();
			ListViewClasses.ItemsSource =
				Enum.GetValues(typeof(HeroClassAll)).OfType<HeroClassAll>().Select(x => new DeckPickerClassItem {DataContext = x}).ToList();
			SelectedClasses = new ObservableCollection<HeroClassAll>();
			_displayedDecks = new ObservableCollection<NewDeckPickerItem>();
			ListViewDecks.ItemsSource = _displayedDecks;
		}

		public ObservableCollection<HeroClassAll> SelectedClasses { get; private set; }

		public event SelectedDeckHandler OnSelectedDeckChanged;
		public event DoubleClickHandler OnDoubleClick;

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(
				e.AddedItems.OfType<DeckPickerClassItem>()
				 .Select(x => x.DataContext as HeroClassAll?)
				 .Where(x => x != null)
				 .Contains(HeroClassAll.All))
			{
				foreach(var item in ((ListView)sender).Items)
				{
					var dpci = item as DeckPickerClassItem;
					if(dpci != null)
					{
						if((HeroClassAll)dpci.DataContext == HeroClassAll.All)
						{
							dpci.OnSelected();
							if(!SelectedClasses.Contains(HeroClassAll.All))
								SelectedClasses.Add(HeroClassAll.All);
						}
						else
						{
							dpci.OnDelselected();
							if(SelectedClasses.Contains((HeroClassAll)dpci.DataContext))
								SelectedClasses.Remove((HeroClassAll)dpci.DataContext);
						}
					}
				}
			}
			else
			{
				foreach(var item in e.AddedItems)
				{
					var pickerItem = item as DeckPickerClassItem;
					if(pickerItem == null)
						continue;
					var heroClass = pickerItem.DataContext as HeroClassAll?;
					if(heroClass == null)
						continue;
					if(!SelectedClasses.Contains(heroClass.Value))
					{
						pickerItem.OnSelected();
						SelectedClasses.Add(heroClass.Value);
					}
				}
				foreach(var item in e.RemovedItems)
				{
					var pickerItem = item as DeckPickerClassItem;
					if(pickerItem == null)
						continue;
					var heroClass = pickerItem.DataContext as HeroClassAll?;
					if(heroClass == null)
						continue;
					if(SelectedClasses.Contains(heroClass.Value))
					{
						pickerItem.OnDelselected();
						SelectedClasses.Remove(heroClass.Value);
					}
				}
			}

			UpdateDecks();
		}

		public void SelectClasses(List<HeroClassAll> classes)
		{
			ListViewClasses.SelectedItems.Clear();
			foreach(var item in ListViewClasses.Items)
			{
				var pickerItem = item as DeckPickerClassItem;
				if(pickerItem == null)
					continue;
				var heroClass = pickerItem.DataContext as HeroClassAll?;
				if(heroClass == null || !classes.Contains(heroClass.Value))
					continue;
				ListViewClasses.SelectedItems.Add(pickerItem);
			}
		}

		public void UpdateDecks()
		{
			_refillingList = true;
			_displayedDecks.Clear();
			var decks =
				DeckList.Instance.Decks.Where(
				                              d =>
				                              DeckMatchesSelectedDeckType(d) && DeckMatchesSelectedTags(d)
				                              && SelectedClasses.Any(c => c.ToString() == "All" || d.Class == c.ToString())).ToList();
			foreach(var deck in decks)
				_displayedDecks.Add(new NewDeckPickerItem(deck));
			Sort();
			_refillingList = false;
			_reselecting = true;
			if(decks.Contains(DeckList.Instance.ActiveDeck))
				SelectDeck(DeckList.Instance.ActiveDeck);
			_reselecting = false;
		}

		private bool DeckMatchesSelectedDeckType(Deck deck)
		{
			if(Config.Instance.SelectedDeckType == DeckType.All)
				return true;
			return Config.Instance.SelectedDeckType == DeckType.Arena && deck.IsArenaDeck
			       || Config.Instance.SelectedDeckType == DeckType.Constructed && !deck.IsArenaDeck;
		}

		public void Sort()
		{
			var view1 = (CollectionView)CollectionViewSource.GetDefaultView(ListViewDecks.ItemsSource);
			view1.SortDescriptions.Clear();

			if(SelectedClasses.Contains(HeroClassAll.All))
				view1.SortDescriptions.Add(new SortDescription("Class", ListSortDirection.Ascending));

			switch(Config.Instance.SelectedDeckSorting)
			{
				case "Name":
					view1.SortDescriptions.Add(new SortDescription("DeckName", ListSortDirection.Ascending));
					break;
				case "Last Edited":
					view1.SortDescriptions.Add(new SortDescription("LastEdited", ListSortDirection.Descending));
					break;
				case "Tag":
					view1.SortDescriptions.Add(new SortDescription("TagList", ListSortDirection.Ascending));
					break;
				case "Win Rate":
					view1.SortDescriptions.Add(new SortDescription("WinPercent", ListSortDirection.Descending));
					break;
			}

			//view1.SortDescriptions.Add(new SortDescription(Config.Instance.SelectedDeckSorting, direction));
			//ListViewDecks.Items.Refresh();
		}

		private void ListViewDecks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach(var item in e.AddedItems)
			{
				var pickerItem = item as NewDeckPickerItem;
				if(pickerItem == null)
					continue;
				if(!_refillingList)
				{
					DeckList.Instance.ActiveDeck = pickerItem.DataContext as Deck;
					pickerItem.OnSelected();
					if(!_reselecting && OnSelectedDeckChanged != null)
						OnSelectedDeckChanged(this, DeckList.Instance.ActiveDeck);
				}
			}
			foreach(var item in e.RemovedItems)
			{
				var pickerItem = item as NewDeckPickerItem;
				if(pickerItem == null)
					continue;
				if(!_refillingList)
					pickerItem.OnDelselected();
			}
		}

		public void SelectDeck(Deck deck)
		{
			if(deck == null)
				return;
			ChangedSelection = true;
			var dpi = _displayedDecks.FirstOrDefault(x => Equals(x.Deck, deck));
			if(ListViewDecks.SelectedItem == dpi)
				return;
			if(dpi == null)
			{
				HeroClassAll heroClass;
				if(Enum.TryParse(deck.Class, out heroClass))
				{
					SelectClasses(new List<HeroClassAll> {heroClass});
					dpi = _displayedDecks.FirstOrDefault(x => Equals(x.Deck, deck));
					if(dpi == null)
						return;
				}
			}
			ListViewDecks.SelectedItem = dpi;
			DeckList.Instance.ActiveDeck = deck;
			ChangedSelection = false;
		}

		public void DeselectDeck()
		{
			ListViewDecks.SelectedItem = null;
		}

		private bool DeckMatchesSelectedTags(Deck deck)
		{
			var selectedTags = Config.Instance.SelectedTags;
			return selectedTags.Any(t => t == "All")
			       || (Config.Instance.TagOperation == TagFilerOperation.Or
				           ? selectedTags.Any(t => deck.Tags.Contains(t) || t == "None" && deck.Tags.Count == 0)
				           : selectedTags.All(t => deck.Tags.Contains(t) || t == "None" && deck.Tags.Count == 0));
		}

		private async void ListViewDecks_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if(OnDoubleClick != null)
			{
				//wait for doubleclick to be over to not reselect the deck
				await Task.Delay(SystemInformation.DoubleClickTime);
				OnDoubleClick(this, DeckList.Instance.ActiveDeck);
			}
		}

		private void ListViewDeckType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(_ignoreSelectionChange)
				return;
			if(e.AddedItems.Count == 0)
				Config.Instance.SelectedDeckType = DeckType.All;
			else
			{
				var item = e.AddedItems[0] as ListViewItem;
				if(item != null)
				{
					switch(item.Name)
					{
						case "ListViewItemAll":
							Config.Instance.SelectedDeckType = DeckType.All;
							break;
						case "ListViewItemArena":
							Config.Instance.SelectedDeckType = DeckType.Arena;
							break;
						case "ListViewItemConstructed":
							Config.Instance.SelectedDeckType = DeckType.Constructed;
							break;
					}
				}
				Config.Save();
				UpdateDecks();
			}
		}

		public void SelectDeckType(DeckType selectedDeckType, bool ignoreSelectionChange = false)
		{
			if(ignoreSelectionChange)
				_ignoreSelectionChange = true;
			ListViewDeckType.SelectedIndex = (int)selectedDeckType;
			if(ignoreSelectionChange)
				_ignoreSelectionChange = false;
		}
	}
}