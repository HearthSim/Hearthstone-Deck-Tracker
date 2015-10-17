#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	/// <summary>
	/// Interaction logic for DeckPicker.xaml
	/// </summary>
	public partial class DeckPicker : INotifyPropertyChanged
	{
		public delegate void DoubleClickHandler(DeckPicker sender, Deck deck);

		public delegate void SelectedDeckHandler(DeckPicker sender, Deck deck);

		private readonly DeckPickerClassItem _archivedClassItem;
		private readonly Dictionary<Deck, DeckPickerItem> _cachedDeckPickerItems = new Dictionary<Deck, DeckPickerItem>();
		private readonly ObservableCollection<DeckPickerClassItem> _classItems;
		private readonly ObservableCollection<DeckPickerItem> _displayedDecks;
		private bool _clearingClasses;
		private ObservableCollection<string> _deckTypeItems;
		private bool _ignoreSelectionChange;
		private DateTime _lastActiveDeckPanelClick = DateTime.MinValue;
		private bool _reselectingClasses;
		public bool ChangedSelection;

		public DeckPicker()
		{
			InitializeComponent();
			_classItems =
				new ObservableCollection<DeckPickerClassItem>(
					Enum.GetValues(typeof(HeroClassAll)).OfType<HeroClassAll>().Select(x => new DeckPickerClassItem {DataContext = x}));
			_archivedClassItem = _classItems.ElementAt((int)HeroClassAll.Archived);
			_classItems.Remove(_archivedClassItem);
			ListViewClasses.ItemsSource = _classItems;
			SelectedClasses = new ObservableCollection<HeroClassAll>();
			_displayedDecks = new ObservableCollection<DeckPickerItem>();
			ListViewDecks.ItemsSource = _displayedDecks;
			DeckTypeItems = new ObservableCollection<string> {"ALL", "ARENA", "CONSTRUCTED"};
		}

		public List<Deck> SelectedDecks
		{
			get { return ListViewDecks.SelectedItems.Cast<DeckPickerItem>().Select(x => x.Deck).ToList(); }
		}

		public ObservableCollection<HeroClassAll> SelectedClasses { get; private set; }
		public bool ArchivedClassVisible { get; set; }
		public bool SearchBarVisibile { get; set; }
		public string DeckNameFilter { get; set; }

		public Visibility VisibilitySearchIcon
		{
			get { return SearchBarVisibile ? Visibility.Collapsed : Visibility.Visible; }
		}

		public Visibility VisibilitySearchBar
		{
			get { return SearchBarVisibile ? Visibility.Visible : Visibility.Collapsed; }
		}

		public ObservableCollection<string> DeckTypeItems
		{
			get { return _deckTypeItems; }
			set
			{
				_deckTypeItems = value;
				OnPropertyChanged();
			}
		}

		public Deck ActiveDeck
		{
			get { return DeckList.Instance.ActiveDeck; }
		}

		public Visibility VisibilityNoDeck
		{
			get { return DeckList.Instance.ActiveDeck == null ? Visibility.Visible : Visibility.Collapsed; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void ActiveDeckChanged()
		{
			OnPropertyChanged("ActiveDeck");
			OnPropertyChanged("VisibilityNoDeck");
		}

		public event SelectedDeckHandler OnSelectedDeckChanged;
		public event DoubleClickHandler OnDoubleClick;

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(_reselectingClasses)
				return;

			IEnumerable<DeckPickerClassItem> removedPickerClassItems;
			var addedPickerClassItems = e.AddedItems.OfType<DeckPickerClassItem>();
			var addedClasses = PickerClassItemsAsEnum(addedPickerClassItems);
			if(addedClasses.Contains(HeroClassAll.All))
			{
				_reselectingClasses = true;
				var senderList = ((ListView)sender);
				senderList.UnselectAll();
				foreach(var item in senderList.Items)
				{
					var dpci = item as DeckPickerClassItem;
					if(dpci != null)
					{
						var hca = (HeroClassAll)dpci.DataContext;

						switch(hca)
						{
							case HeroClassAll.All:
								senderList.SelectedItems.Add(item);
								SelectPickerClassItem(dpci);
								break;
							case HeroClassAll.Archived:
								if(!SelectedClasses.Contains(HeroClassAll.Archived))
								{
									if(addedClasses.Contains(HeroClassAll.Archived))
									{
										senderList.SelectedItems.Add(item);
										SelectPickerClassItem(dpci);
									}
								}
								else
								{
									removedPickerClassItems = e.RemovedItems.OfType<DeckPickerClassItem>();
									if(PickerClassItemsAsEnum(removedPickerClassItems).Contains(HeroClassAll.Archived))
										DeselectPickerClassItem(dpci);
									else
										senderList.SelectedItems.Add(item);
								}
								break;
							default:
								DeselectPickerClassItem(dpci);
								break;
						}
					}
				}
				_reselectingClasses = false;
			}
			else
			{
				DeckPickerClassItem removedAllClassItem = null;
				removedPickerClassItems = e.RemovedItems.OfType<DeckPickerClassItem>();
				foreach(var dpci in removedPickerClassItems)
				{
					var heroClass = dpci.DataContext as HeroClassAll?;
					if(heroClass == null)
						continue;

					if(heroClass == HeroClassAll.All)
					{
						// We remove this from SelectedClasses now but we don't raise it's OnDeselected event yet,
						// instead store a reference to it in case we want to quietly add this back to the
						// SelectedClasses list later
						SelectedClasses.Remove(heroClass.Value);
						removedAllClassItem = dpci;
					}
					else
						DeselectPickerClassItem(dpci);
				}

				var allIsSelected = SelectedClasses.Contains(HeroClassAll.All);
				foreach(var dpci in addedPickerClassItems)
				{
					var heroClass = dpci.DataContext as HeroClassAll?;
					if(heroClass == null)
						continue;

					if(allIsSelected && heroClass != HeroClassAll.Archived)
					{
						_reselectingClasses = true;
						((ListView)sender).SelectedItems.Remove(dpci);
						_reselectingClasses = false;
						continue;
					}

					SelectPickerClassItem(dpci);
				}

				if(SelectedClasses.Count == 0 && !_clearingClasses)
				{
					var senderList = (ListView)sender;
					if(removedAllClassItem == null)
					{
						var dpciAll = PickerClassItemFromEnum(senderList, HeroClassAll.All);

						// Select 'All', raising its OnSelected event
						_reselectingClasses = true;
						senderList.SelectedItems.Add(dpciAll);
						SelectPickerClassItem(dpciAll);
						_reselectingClasses = false;
					}
					else
					{
						// If there are no selected classes, and we earlier removed 'All', quietly add it back
						_reselectingClasses = true;
						senderList.SelectedItems.Add(removedAllClassItem);
						SelectedClasses.Add(HeroClassAll.All);
						_reselectingClasses = false;

						// And make sure we do not raise its OnDeselected event if we were going to
						removedAllClassItem = null;
					}
				}

				// If we removed the 'All' class earlier, raise the DeckPickerClassItem's OnDeselected event now
				if(removedAllClassItem != null)
					removedAllClassItem.OnDelselected();
			}

			if(Core.MainWindow.IsLoaded)
				UpdateDecks();
		}

		private void SelectPickerClassItem(DeckPickerClassItem dpci)
		{
			var heroClass = dpci.DataContext as HeroClassAll?;
			if(heroClass != null && !SelectedClasses.Contains(heroClass.Value))
			{
				SelectedClasses.Add(heroClass.Value);
				dpci.OnSelected();
			}
		}

		private void DeselectPickerClassItem(DeckPickerClassItem dpci)
		{
			var heroClass = dpci.DataContext as HeroClassAll?;
			if(heroClass != null && SelectedClasses.Remove(heroClass.Value))
				dpci.OnDelselected();
		}

		private static IEnumerable<HeroClassAll?> PickerClassItemsAsEnum(IEnumerable<DeckPickerClassItem> items)
		{
			return items.Select(x => x.DataContext as HeroClassAll?).Where(x => x != null);
		}

		private static DeckPickerClassItem PickerClassItemFromEnum(ListView sender, HeroClassAll heroClass)
		{
			var items = sender.Items.OfType<DeckPickerClassItem>().Where(x => (x.DataContext as HeroClassAll?).HasValue);
			return items.FirstOrDefault(x => (x.DataContext as HeroClassAll?).Value == heroClass);
		}

		public void SelectClasses(List<HeroClassAll> classes)
		{
			_clearingClasses = true;
			ListViewClasses.SelectedItems.Clear();
			_clearingClasses = false;

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

		public void SelectClass(HeroClassAll heroClass)
		{
			if(!SelectedClasses.Contains(heroClass))
			{
				var dpci = PickerClassItemFromEnum(ListViewClasses, heroClass);
				ListViewClasses.SelectedItems.Add(dpci);
			}
		}

		public void UpdateDecks(bool reselectActiveDeck = true, IEnumerable<Deck> forceUpdate = null)
		{
			var selectedDeck = SelectedDecks.FirstOrDefault();
			var decks =
				DeckList.Instance.Decks.Where(
				                              d =>
				                              (string.IsNullOrEmpty(DeckNameFilter)
				                               || d.Name.ToLowerInvariant().Contains(DeckNameFilter.ToLowerInvariant()))
				                              && DeckMatchesSelectedDeckType(d) && DeckMatchesSelectedTags(d)
				                              && (SelectedClasses.Any(
				                                                      c =>
				                                                      ((c.ToString() == "All" || d.Class == c.ToString()) && !d.Archived)
				                                                      || (c.ToString() == "Archived" && d.Archived)))).ToList();


			if(forceUpdate == null)
				forceUpdate = new List<Deck>();
			foreach(var deck in _displayedDecks.Where(dpi => !decks.Contains(dpi.Deck) || forceUpdate.Contains(dpi.Deck)).ToList())
				_displayedDecks.Remove(deck);
			foreach(var deck in decks.Where(d => !_displayedDecks.Select(x => x.Deck).Contains(d)))
				_displayedDecks.Add(GetDeckPickerItemFromCache(deck));
			Sort();
			if(selectedDeck != null && reselectActiveDeck && decks.Contains(selectedDeck))
				SelectDeck(selectedDeck);
			if(ActiveDeck != null)
				ActiveDeck.StatsUpdated();
		}

		private DeckPickerItem GetDeckPickerItemFromCache(Deck deck)
		{
			DeckPickerItem dpi;
			if(_cachedDeckPickerItems.TryGetValue(deck, out dpi))
				return dpi;
			Type layout;
			switch(Config.Instance.DeckPickerItemLayout)
			{
				case DeckLayout.Layout1:
					layout = typeof(DeckPickerItemLayout1);
					break;
				case DeckLayout.Layout2:
					layout = typeof(DeckPickerItemLayout2);
					break;
				case DeckLayout.Legacy:
					layout = typeof(DeckPickerItemLayoutLegacy);
					break;
				default:
					layout = typeof(DeckPickerItemLayout1);
					break;
			}
			dpi = new DeckPickerItem(deck, layout);
			_cachedDeckPickerItems.Add(deck, dpi);
			return dpi;
		}

		public void ClearFromCache(Deck deck)
		{
			if(_cachedDeckPickerItems.ContainsKey(deck))
				_cachedDeckPickerItems.Remove(deck);
		}

		public void UpdateArchivedClassVisibility()
		{
			if(DeckList.Instance.Decks.Any(d => d.Archived))
			{
				if(!_classItems.Contains(_archivedClassItem))
				{
					_classItems.Add(_archivedClassItem);
					ArchivedClassVisible = true;

					if(PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs("ArchivedClassVisible"));
				}
			}
			else
			{
				var removed = _classItems.Remove(_archivedClassItem);

				if(removed)
				{
					ArchivedClassVisible = false;

					if(PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs("ArchivedClassVisible"));
				}

				SelectedClasses.Remove(HeroClassAll.Archived);
				if(SelectedClasses.Count == 0)
					SelectClass(HeroClassAll.All);
			}
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
			var view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewDecks.ItemsSource);
			view.SortDescriptions.Clear();
			if(Config.Instance.SortDecksByClass && Config.Instance.SelectedDeckType != DeckType.Arena
			   || Config.Instance.SortDecksByClassArena && Config.Instance.SelectedDeckType == DeckType.Arena)
				view.SortDescriptions.Add(new SortDescription("Class", ListSortDirection.Ascending));

			var deckSorting = Config.Instance.SelectedDeckType == DeckType.Arena
				                  ? Config.Instance.SelectedDeckSortingArena : Config.Instance.SelectedDeckSorting;
			switch(deckSorting)
			{
				case "Name":
					view.SortDescriptions.Add(new SortDescription("DeckName", ListSortDirection.Ascending));
					break;
				case "Last Played":
					view.SortDescriptions.Add(new SortDescription("LastPlayed", ListSortDirection.Descending));
					break;
				case "Last Played (new first)":
					view.SortDescriptions.Add(new SortDescription("LastPlayedNewFirst", ListSortDirection.Descending));
					break;
				case "Last Edited":
					view.SortDescriptions.Add(new SortDescription("LastEdited", ListSortDirection.Descending));
					break;
				case "Tag":
					view.SortDescriptions.Add(new SortDescription("TagList", ListSortDirection.Ascending));
					break;
				case "Win Rate":
					view.SortDescriptions.Add(new SortDescription("WinPercent", ListSortDirection.Descending));
					break;
			}
		}

		private void ListViewDecks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(Config.Instance.DeckPickerItemLayout == DeckLayout.Legacy)
			{
				foreach(var deck in e.AddedItems.Cast<DeckPickerItem>())
					deck.RefreshProperties();
				foreach(var deck in e.RemovedItems.Cast<DeckPickerItem>())
					deck.RefreshProperties();
			}
			if(e.AddedItems.Count > 0 && OnSelectedDeckChanged != null)
				OnSelectedDeckChanged(this, SelectedDecks.FirstOrDefault());
		}

		public void SelectDeckAndAppropriateView(Deck deck)
		{
			if(deck == null)
				return;
			ClearFromCache(deck);
			if(Config.Instance.SelectedDeckType != DeckType.All)
			{
				if(deck.IsArenaDeck && Config.Instance.SelectedDeckType != DeckType.Arena)
					SelectDeckType(DeckType.Arena);
				else if(!deck.IsArenaDeck && Config.Instance.SelectedDeckType != DeckType.Constructed)
					SelectDeckType(DeckType.Constructed);
			}

			if(deck.Archived && !SelectedClasses.Contains(HeroClassAll.Archived))
				SelectClass(HeroClassAll.Archived);
			else if(!SelectedClasses.Contains(HeroClassAll.All))
			{
				HeroClassAll deckClass;
				if(Enum.TryParse(deck.Class, out deckClass))
				{
					if(!SelectedClasses.Contains(deckClass))
						SelectClass(deckClass);
				}
			}

			if(!DeckMatchesSelectedTags(deck))
			{
				if(Config.Instance.TagOperation == TagFilerOperation.Or)
				{
					var missingTags = deck.Tags.Where(tag => !Config.Instance.SelectedTags.Contains(tag)).ToList();
					if(missingTags.Any())
					{
						Config.Instance.SelectedTags.AddRange(missingTags);
						Logger.WriteLine("Added missing tags so the deck shows up: " + missingTags.Aggregate((c, n) => c + ", " + n));
					}
					else
					{
						Config.Instance.SelectedTags.Add("None");
						Logger.WriteLine("Added missing tags so the deck shows up: None");
					}
				}
				else
				{
					Config.Instance.SelectedTags = new List<string> {"All"};
					Logger.WriteLine("Set tags to ALL so the deck shows up");
				}
				Config.Save();
				Core.MainWindow.SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);
			}

			UpdateDecks(false);
			SelectDeck(deck);
			var dpi = _displayedDecks.FirstOrDefault(x => Equals(x.Deck, deck));
			if(dpi != null)
				ListViewDecks.ScrollIntoView(dpi);
		}

		public void SelectDeck(Deck deck)
		{
			if(deck == null)
				return;
			ChangedSelection = true;
			var dpi = _displayedDecks.FirstOrDefault(x => Equals(x.Deck, deck));
			if(ListViewDecks.SelectedItem != dpi)
			{
				if(dpi == null)
				{
					if(deck.Archived)
						SelectClass(HeroClassAll.Archived);
					else
					{
						HeroClassAll heroClass;
						if(Enum.TryParse(deck.Class, out heroClass))
							SelectClass(heroClass);
					}

					UpdateDecks();
					dpi = _displayedDecks.FirstOrDefault(x => Equals(x.Deck, deck));
					if(dpi == null)
					{
						ChangedSelection = false;
						return;
					}
				}
				ListViewDecks.SelectedItem = dpi;
				deck.StatsUpdated();
			}
			ChangedSelection = false;
		}

		public void DeselectDeck()
		{
			ListViewDecks.SelectedItem = null;
			RefreshDisplayedDecks();
		}

		public void RefreshDisplayedDecks()
		{
			foreach(var deckPickerItem in _displayedDecks)
				deckPickerItem.RefreshProperties();
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
				OnDoubleClick(this, SelectedDecks.FirstOrDefault());
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
				var item = e.AddedItems[0] as string;
				if(item != null)
				{
					switch(item)
					{
						case "ALL":
							Config.Instance.SelectedDeckType = DeckType.All;
							break;
						case "ARENA":
							Config.Instance.SelectedDeckType = DeckType.Arena;
							break;
						case "CONSTRUCTED":
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

		private void RectangleSearchIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SearchBarVisibile = true;
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("VisibilitySearchBar"));
				PropertyChanged(this, new PropertyChangedEventArgs("VisibilitySearchIcon"));
			}
			TextBoxSearchBar.Focus();
		}

		private void RectangleCloseIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			CloseSearchField();
		}

		private void TextBoxSearchBar_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				DeckNameFilter = TextBoxSearchBar.Text;
				UpdateDecks();
				e.Handled = true;
			}
			else if(e.Key == Key.Escape)
				CloseSearchField();
		}

		private void CloseSearchField()
		{
			bool updateDecks = !string.IsNullOrEmpty(DeckNameFilter);
			TextBoxSearchBar.Clear();
			DeckNameFilter = null;
			SearchBarVisibile = false;
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("VisibilitySearchBar"));
				PropertyChanged(this, new PropertyChangedEventArgs("VisibilitySearchIcon"));
			}
			if(updateDecks)
				UpdateDecks();
		}

		private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
		{
			var selectedDecks = Core.MainWindow.DeckPickerList.SelectedDecks;
			if(!selectedDecks.Any())
				return;
			Core.MainWindow.TagControlEdit.SetSelectedTags(selectedDecks);
			MenuItemQuickSetTag.ItemsSource = Core.MainWindow.TagControlEdit.Tags;
			MenuItemMoveDecktoArena.Visibility = selectedDecks.First().IsArenaDeck ? Visibility.Collapsed : Visibility.Visible;
			MenuItemMoveDeckToConstructed.Visibility = selectedDecks.First().IsArenaDeck ? Visibility.Visible : Visibility.Collapsed;
			MenuItemMissingCards.Visibility = selectedDecks.First().MissingCards.Any() ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(selectedDecks.First().Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(selectedDecks.First().Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemArchive.Visibility = selectedDecks.Any(d => !d.Archived) ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUnarchive.Visibility = selectedDecks.Any(d => d.Archived) ? Visibility.Visible : Visibility.Collapsed;
			SeparatorDeck1.Visibility = string.IsNullOrEmpty(selectedDecks.First().Url) && !selectedDecks.First().MissingCards.Any()
				                            ? Visibility.Collapsed : Visibility.Visible;
			MenuItemOpenHearthStats.Visibility = selectedDecks.First().HasHearthStatsId ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUseDeck.Visibility =
				SeparatorUseDeck.Visibility = selectedDecks.First().Equals(DeckList.Instance.ActiveDeck) ? Visibility.Collapsed : Visibility.Visible;
		}

		private void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnEditDeck_Click(sender, e);
		}

		private void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnNotes_Click(sender, e);
		}

		private void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnTags_Click(sender, e);
		}

		private void BtnMoveDeckToArena_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnMoveDeckToArena_Click(sender, e);
		}

		private void BtnMoveDeckToConstructed_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnMoveDeckToConstructed_Click(sender, e);
		}

		private void MenuItemMissingDust_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.MenuItemMissingDust_OnClick(sender, e);
		}

		private void BtnUpdateDeck_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnUpdateDeck_Click(sender, e);
		}

		private void BtnOpenDeckUrl_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnOpenDeckUrl_Click(sender, e);
		}

		private void BtnArchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnArchiveDeck_Click(sender, e);
		}

		private void BtnUnarchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnUnarchiveDeck_Click(sender, e);
		}

		private void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnDeleteDeck_Click(sender, e);
		}

		private void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnCloneDeck_Click(sender, e);
		}

		private void BtnCloneSelectedVersion_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnCloneSelectedVersion_Click(sender, e);
		}

		private void BtnName_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnName_Click(sender, e);
		}

		private void BtnOpenHearthStats_Click(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.BtnOpenHearthStats_Click(sender, e);
		}

		private async void ActiveDeckPanel_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if((DateTime.Now - _lastActiveDeckPanelClick).TotalMilliseconds < SystemInformation.DoubleClickTime)
			{
				if(ActiveDeck != null)
					SelectDeckAndAppropriateView(ActiveDeck);
				_lastActiveDeckPanelClick = DateTime.MinValue;
			}
			else
				_lastActiveDeckPanelClick = DateTime.Now;
		}

		private void BtnUseDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = SelectedDecks.FirstOrDefault();
			if(deck != null)
			{
				Core.MainWindow.DeckPickerList.SelectDeck(deck);
				Core.MainWindow.SelectDeck(deck, true);
				Core.MainWindow.DeckPickerList.RefreshDisplayedDecks();
			}
		}
	}
}