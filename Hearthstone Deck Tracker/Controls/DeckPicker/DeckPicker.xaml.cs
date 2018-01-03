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
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.ComponentModel.ListSortDirection;
using static System.Windows.Visibility;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;
using Hearthstone_Deck_Tracker.Windows;
using DeckType = Hearthstone_Deck_Tracker.Enums.DeckType;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	/// <summary>
	/// Interaction logic for DeckPicker.xaml
	/// </summary>
	public partial class DeckPicker : INotifyPropertyChanged
	{
		private const string LocLink = "DeckPicker_ContextMenu_LinkUrl";
		private const string LocLinkNew = "DeckPicker_ContextMenu_LinkNewUrl";

		public delegate void DoubleClickHandler(DeckPicker sender, Deck deck);

		public delegate void SelectedDeckHandler(DeckPicker sender, List<Deck> deck);

		private readonly DeckPickerClassItem _archivedClassItem;
		private readonly Dictionary<Deck, DeckPickerItem> _cachedDeckPickerItems = new Dictionary<Deck, DeckPickerItem>();
		private readonly ObservableCollection<DeckPickerClassItem> _classItems;
		private readonly List<DeckPickerItem> _displayedDecks;
		private bool _clearingClasses;
		private ObservableCollection<DeckType> _deckTypeItems;
		private bool _ignoreSelectionChange;
		private DateTime _lastActiveDeckPanelClick = DateTime.MinValue;
		private bool _reselectingClasses;
		public bool ChangedSelection;
		private bool _searchBarVisibile;
		private bool _archivedClassVisible;

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
			_displayedDecks = new List<DeckPickerItem>();
		}

		public List<DeckPickerItem> DisplayedDecks => _displayedDecks;

		public List<Deck> SelectedDecks => ListViewDecks.SelectedItems.Cast<DeckPickerItem>().Select(x => x.Deck).ToList();

		public ObservableCollection<HeroClassAll> SelectedClasses { get; }

		public void ReloadUI()
		{
			_deckTypeItems = null;
			OnPropertyChanged(nameof(DeckTypeItems));
			RefreshDisplayedDecks();
		}

		public bool ArchivedClassVisible
		{
			get => _archivedClassVisible;
			set
			{
				_archivedClassVisible = value;
				OnPropertyChanged();
			}
		}

		public bool SearchBarVisibile
		{
			get => _searchBarVisibile;
			set
			{
				_searchBarVisibile = value;
				OnPropertyChanged(nameof(VisibilitySearchBar));
				OnPropertyChanged(nameof(VisibilitySearchIcon));
			}
		}

		public string DeckNameFilter { get; set; }

		public Visibility VisibilitySearchIcon => SearchBarVisibile ? Collapsed : Visible;

		public Visibility VisibilitySearchBar => SearchBarVisibile ? Visible : Collapsed;

		public ObservableCollection<DeckType> DeckTypeItems => _deckTypeItems ?? (_deckTypeItems = new ObservableCollection<DeckType>(Enum.GetValues(typeof(DeckType)).OfType<DeckType>().Distinct().Take(6)));

		public Deck ActiveDeck => DeckList.Instance.ActiveDeck;

		public Visibility VisibilityNoDeck => DeckList.Instance.ActiveDeck == null ? Visible : Collapsed;

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void ActiveDeckChanged()
		{
			OnPropertyChanged(nameof(ActiveDeck));
			OnPropertyChanged(nameof(VisibilityNoDeck));
			UpdateDeckModeToggleButton();
		}

		public event SelectedDeckHandler OnSelectedDeckChanged;
		public event DoubleClickHandler OnDoubleClick;

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(_reselectingClasses)
				return;

			var removedPickerClassItems = e.RemovedItems.OfType<DeckPickerClassItem>();
			var addedPickerClassItems = e.AddedItems.OfType<DeckPickerClassItem>().ToList();
			var addedClasses = PickerClassItemsAsEnum(addedPickerClassItems).ToList();
			if(addedClasses.Contains(HeroClassAll.All))
			{
				_reselectingClasses = true;
				var senderList = ((ListView)sender);
				senderList.UnselectAll();
				foreach(var item in senderList.Items)
				{
					if(item is DeckPickerClassItem dpci)
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
				foreach(var dpci in removedPickerClassItems)
				{
					var heroClass = dpci.DataContext as HeroClassAll?;
					switch(heroClass)
					{
						case null:
							continue;
						case HeroClassAll.All:
							// We remove this from SelectedClasses now but we don't raise it's OnDeselected event yet,
							// instead store a reference to it in case we want to quietly add this back to the
							// SelectedClasses list later
							SelectedClasses.Remove(heroClass.Value);
							removedAllClassItem = dpci;
							break;
						default:
							DeselectPickerClassItem(dpci);
							break;
					}
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
				removedAllClassItem?.OnDelselected();
			}

			if(Core.MainWindow.IsLoaded)
				UpdateDecks();
		}

		private void SelectPickerClassItem(DeckPickerClassItem dpci)
		{
#pragma warning disable IDE0019
			var heroClass = dpci.DataContext as HeroClassAll?;
			if(heroClass != null && !SelectedClasses.Contains(heroClass.Value))
			{
				SelectedClasses.Add(heroClass.Value);
				dpci.OnSelected();
			}
#pragma warning restore IDE0019
		}

		private void DeselectPickerClassItem(DeckPickerClassItem dpci)
		{
#pragma warning disable IDE0019
			var heroClass = dpci.DataContext as HeroClassAll?;
			if(heroClass != null && SelectedClasses.Remove(heroClass.Value))
				dpci.OnDelselected();
#pragma warning restore IDE0019
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
				var heroClass = pickerItem?.DataContext as HeroClassAll?;
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
			{
				var dpi = GetDeckPickerItemFromCache(deck);
				if(dpi != null)
				{
					_displayedDecks.Add(dpi);
					dpi.RefreshProperties();
				}
			}
			Sort();
			OnPropertyChanged(nameof(DisplayedDecks));
			if(selectedDeck != null && reselectActiveDeck && decks.Contains(selectedDeck))
				SelectDeck(selectedDeck);
			ActiveDeck?.StatsUpdated();
			selectedDeck?.StatsUpdated();
		}

		private DeckPickerItem GetDeckPickerItemFromCache(Deck deck)
		{
			if(deck == null)
				return null;
			if(_cachedDeckPickerItems.TryGetValue(deck, out var dpi))
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

		public void ClearFromCache(Deck deck) => _cachedDeckPickerItems.Remove(deck);

		public void UpdateArchivedClassVisibility()
		{
			if(DeckList.Instance.Decks.Any(d => d.Archived))
			{
				if(!_classItems.Contains(_archivedClassItem))
				{
					_classItems.Add(_archivedClassItem);
					ArchivedClassVisible = true;
				}
			}
			else
			{
				if (_classItems.Remove(_archivedClassItem))
					ArchivedClassVisible = false;

				SelectedClasses.Remove(HeroClassAll.Archived);
				if(SelectedClasses.Count == 0)
					SelectClass(HeroClassAll.All);
			}
		}

		private bool IsConstructedDeck(Deck deck) => !deck.IsBrawlDeck && !deck.IsDungeonDeck && !deck.IsArenaDeck;

		private bool DeckMatchesSelectedDeckType(Deck deck)
		{
			switch(Config.Instance.SelectedDeckPickerDeckType)
			{
				case DeckType.All:
					return true;
				case DeckType.Arena:
					return deck.IsArenaDeck;
				case DeckType.Dungeon:
					return deck.IsDungeonDeck;
				case DeckType.Brawl:
					return deck.IsBrawlDeck;
				case DeckType.Standard:
					return IsConstructedDeck(deck) && deck.StandardViable;
				case DeckType.Wild:
					return IsConstructedDeck(deck) && (Config.Instance.DeckPickerWildIncludesStandard || !deck.StandardViable);
				default:
					return false;
			}
		}

		public void Sort()
		{
			var view = (CollectionView)CollectionViewSource.GetDefaultView(DisplayedDecks);
			view.SortDescriptions.Clear();
			if(Config.Instance.SortDecksFavoritesFirst)
				view.SortDescriptions.Add(new SortDescription("Favorite", Descending));

			if(Config.Instance.SortDecksByClass && Config.Instance.SelectedDeckPickerDeckType != DeckType.Arena
			   || Config.Instance.SortDecksByClassArena && Config.Instance.SelectedDeckPickerDeckType == DeckType.Arena)
				view.SortDescriptions.Add(new SortDescription("Class", Ascending));

			var deckSorting = Config.Instance.SelectedDeckPickerDeckType == DeckType.Arena
				                  ? Config.Instance.SelectedDeckSortingArena : Config.Instance.SelectedDeckSorting;
			switch(deckSorting)
			{
				case "Most Played":
					view.SortDescriptions.Add(new SortDescription("NumGames", Descending));
					break;
				case "Name":
					view.SortDescriptions.Add(new SortDescription("DeckName", Ascending));
					break;
				case "Last Played":
					view.SortDescriptions.Add(new SortDescription("LastPlayed", Descending));
					break;
				case "Last Played (new first)":
					view.SortDescriptions.Add(new SortDescription("LastPlayedNewFirst", Descending));
					break;
				case "Last Edited":
					view.SortDescriptions.Add(new SortDescription("LastEdited", Descending));
					break;
				case "Tag":
					view.SortDescriptions.Add(new SortDescription("TagList", Ascending));
					break;
				case "Win Rate":
					view.SortDescriptions.Add(new SortDescription("WinPercent", Descending));
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
			OnSelectedDeckChanged?.Invoke(this, SelectedDecks);
		}

		public void SelectDeckAndAppropriateView(Deck deck)
		{
			if(deck == null)
				return;
			ClearFromCache(deck);
			if(Config.Instance.SelectedDeckPickerDeckType != DeckType.All)
			{
				if(deck.IsArenaDeck)
					SelectDeckType(DeckType.Arena);
				else if(deck.IsDungeonDeck)
					SelectDeckType(DeckType.Dungeon);
				else if(deck.IsBrawlDeck)
					SelectDeckType(DeckType.Brawl);
				else if(IsConstructedDeck(deck))
					SelectDeckType(deck.StandardViable ? DeckType.Standard : DeckType.Wild);
			}
			if(deck.Archived && !SelectedClasses.Contains(HeroClassAll.Archived))
				SelectClass(HeroClassAll.Archived);
			else if(!SelectedClasses.Contains(HeroClassAll.All) && Enum.TryParse(deck.Class, out HeroClassAll deckClass) && !SelectedClasses.Contains(deckClass))
				SelectClass(deckClass);

			if(!DeckMatchesSelectedTags(deck))
			{
				if(Config.Instance.TagOperation == TagFilerOperation.Or)
				{
					var missingTags = deck.Tags.Where(tag => !Config.Instance.SelectedTags.Contains(tag)).ToList();
					if(missingTags.Any())
					{
						Config.Instance.SelectedTags.AddRange(missingTags);
						Log.Info("Added missing tags so the deck shows up: " + missingTags.Aggregate((c, n) => c + ", " + n));
					}
					else
					{
						Config.Instance.SelectedTags.Add("None");
						Log.Info("Added missing tags so the deck shows up: None");
					}
				}
				else
				{
					Config.Instance.SelectedTags = new List<string> {"All"};
					Log.Info("Set tags to ALL so the deck shows up");
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
						if(Enum.TryParse(deck.Class, out HeroClassAll heroClass))
							SelectClass(heroClass);
					}

					var deckType = (DeckType)ListViewDeckType.SelectedIndex;
					if(deckType != DeckType.All && deck.IsArenaDeck != (deckType == DeckType.Arena))
						SelectDeckType(DeckType.All);

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
			if(_ignoreSelectionChange || !Core.Initialized)
				return;
			if(e.AddedItems.Count > 0)
			{
				var selected = (DeckType)ListViewDeckType.SelectedItem;
				if(Config.Instance.SelectedDeckPickerDeckType != selected)
				{
					Config.Instance.SelectedDeckPickerDeckType = selected;
					Config.Save();
				}
				UpdateDecks();
			}
			else
				ListViewDeckType.SelectedIndex = 0;
		}

		public void SelectDeckType(DeckType selectedDeckType, bool ignoreSelectionChange = false)
		{
			if(ListViewDeckType.SelectedIndex == (int)selectedDeckType)
				return;
			if(ignoreSelectionChange)
				_ignoreSelectionChange = true;
			ListViewDeckType.SelectedIndex = (int)selectedDeckType;
			if(ignoreSelectionChange)
				_ignoreSelectionChange = false;
		}

		private void RectangleSearchIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SearchBarVisibile = true;
			TextBoxSearchBar.Focus();
		}

		private void RectangleCloseIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => CloseSearchField();

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
			var updateDecks = !string.IsNullOrEmpty(DeckNameFilter);
			TextBoxSearchBar.Clear();
			DeckNameFilter = null;
			SearchBarVisibile = false;
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
			MenuItemMoveDecktoArena.Visibility = selectedDecks.First().IsArenaDeck ? Collapsed : Visible;
			MenuItemMoveDeckToConstructed.Visibility = selectedDecks.First().IsArenaDeck ? Visible : Collapsed;
			MenuItemMissingCards.Visibility = selectedDecks.First().MissingCards.Any() ? Visible : Collapsed;
			MenuItemSetDeckUrl.Visibility = selectedDecks.First().IsArenaDeck ? Collapsed : Visible;
			MenuItemSetDeckUrl.Header = string.IsNullOrEmpty(selectedDecks.First().Url) ? LocUtil.Get(LocLink, true) : LocUtil.Get(LocLinkNew, true);
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(selectedDecks.First().Url) ? Collapsed : Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(selectedDecks.First().Url) ? Collapsed : Visible;
			MenuItemArchive.Visibility = selectedDecks.Any(d => !d.Archived) ? Visible : Collapsed;
			MenuItemUnarchive.Visibility = selectedDecks.Any(d => d.Archived) ? Visible : Collapsed;
			SeparatorDeck1.Visibility = selectedDecks.First().IsArenaDeck ? Collapsed : Visible;
			MenuItemUseDeck.Visibility =
				SeparatorUseDeck.Visibility =
				selectedDecks.First().Equals(DeckList.Instance.ActiveDeck) ? Collapsed : Visible;
			MenuItemExportDeck.Visibility = selectedDecks.First().IsArenaDeck ? Collapsed : Visible;
			MenuItemVersionHistory.Visibility = selectedDecks.First().HasVersions ? Visible : Collapsed;
		}


		private void BtnEditDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowDeckEditorFlyout(SelectedDecks.FirstOrDefault(), false);
		private void BtnNotes_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowDeckNotesDialog(SelectedDecks.FirstOrDefault());
		private void BtnTags_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowTagEditDialog(SelectedDecks);
		private void BtnMoveDeckToArena_Click(object sender, RoutedEventArgs e) => Core.MainWindow.MoveDecksToArena(SelectedDecks);
		private void BtnMoveDeckToConstructed_Click(object sender, RoutedEventArgs e) => Core.MainWindow.MoveDecksToConstructed(SelectedDecks);
		private void MenuItemMissingDust_OnClick(object sender, RoutedEventArgs e) => Core.MainWindow.ShowMissingCardsMessage(SelectedDecks.FirstOrDefault(), false).Forget();
		private void BtnSetDeckUrl_Click(object sender, RoutedEventArgs e) => Core.MainWindow.SetDeckUrl(SelectedDecks.FirstOrDefault());
		private void BtnUpdateDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.UpdateDeckFromWeb(SelectedDecks.FirstOrDefault());
		private void BtnOpenDeckUrl_Click(object sender, RoutedEventArgs e) => Core.MainWindow.OpenDeckUrl(SelectedDecks.FirstOrDefault());
		private void BtnArchiveDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ArchiveDecks(SelectedDecks);
		private void BtnUnarchiveDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.UnArchiveDecks(SelectedDecks);
		private void BtnDeleteDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowDeleteDecksMessage(SelectedDecks);
		private void BtnCloneDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowCloneDeckDialog(SelectedDecks.FirstOrDefault());
		private void BtnCloneSelectedVersion_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowCloneDeckVersionDialog(SelectedDecks.FirstOrDefault());
		private void BtnName_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowEditDeckNameDialog(SelectedDecks.FirstOrDefault());
		private void BtnExportDeck_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowExportFlyout(SelectedDecks.FirstOrDefault());
		private void BtnScreenshotCards_Click(object sender, RoutedEventArgs e) => Core.MainWindow.ShowScreenshotFlyout();
		private void MenuItemVersionHistory_OnClick(object sender, RoutedEventArgs e) => Core.MainWindow.ShowDeckHistoryFlyout();

		private void ActiveDeckPanel_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
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

		private void RectangleSortIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) 
			=> Core.MainWindow.FlyoutSortFilter.IsOpen = !Core.MainWindow.FlyoutSortFilter.IsOpen;

		private void RectangleUseNoDeckIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(DeckList.Instance.ActiveDeck == null)
				Core.MainWindow.SelectLastUsedDeck();
			else
				Core.MainWindow.SelectDeck(null, true);
			UpdateDeckModeToggleButton();
		}

		public void UpdateDeckModeToggleButton()
		{
			OnPropertyChanged(nameof(BorderDeckModeBackground));
			OnPropertyChanged(nameof(BorderDeckModeTextBrush));
		}

		public SolidColorBrush BorderDeckModeBackground
			=> DeckList.Instance.ActiveDeck == null ? (SolidColorBrush)FindResource("AccentColorBrush") : new SolidColorBrush(Colors.Transparent);

		public SolidColorBrush BorderDeckModeTextBrush
			=> DeckList.Instance.ActiveDeck == null ? new SolidColorBrush(Colors.White) : (SolidColorBrush)FindResource("TextBrush");

		private void ListViewDecks_OnKeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Delete)
				Core.MainWindow.ShowDeleteDecksMessage(SelectedDecks);
		}

		private void ListViewDecks_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(!SelectedDecks.Any())
				e.Handled = true;
		}
	}
}
