#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

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
				                              DeckMatchesSelectedTags(d)
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

		public void Sort()
		{
			var view1 = (CollectionView)CollectionViewSource.GetDefaultView(ListViewDecks.ItemsSource);
			view1.SortDescriptions.Clear();

			var direction = Config.Instance.SelectedDeckSorting == "Name" || Config.Instance.SelectedDeckSorting == "Tag"
				                ? ListSortDirection.Ascending : ListSortDirection.Descending;
			view1.SortDescriptions.Add(new SortDescription(Config.Instance.SelectedDeckSorting, direction));
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
				//something...
				return;
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

		private void ListViewDecks_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if(OnDoubleClick != null)
				OnDoubleClick(this, DeckList.Instance.ActiveDeck);
		}
	}
}