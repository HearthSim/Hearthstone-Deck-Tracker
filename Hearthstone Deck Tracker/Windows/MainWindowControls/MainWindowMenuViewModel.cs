using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Stats;
using static System.Windows.Visibility;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public class MainWindowMenuViewModel : INotifyPropertyChanged
	{
		private const string LocLink = "DeckPicker_ContextMenu_LinkUrl";
		private const string LocLinkNew = "DeckPicker_ContextMenu_LinkNewUrl";

		private IEnumerable<Deck> _decks;
		public MainWindow MainWindow => Core.MainWindow;

		public IEnumerable<Deck> Decks
		{
			get => _decks ?? new List<Deck>();
			set
			{
				_decks = value; 
				OnPropertyChanged();
				OnPropertyChanged(nameof(HasSelectedDeck));
			}
		}

		public ICommand NewDeckCommand => new Command<string>(playerClass => MainWindow.ShowNewDeckMessage(playerClass));
		public ICommand EditDeckCommand => new Command(() => MainWindow.SetNewDeck(Decks.FirstOrDefault(), true));
		public ICommand RenameDeckNameCommand => new Command(() => MainWindow.ShowEditDeckNameDialog(Decks.FirstOrDefault()));
		public ICommand EditNotesCommand => new Command(() => MainWindow.ShowDeckNotesDialog(Decks.FirstOrDefault()));
		public ICommand EditTagsCommand => new Command(() => MainWindow.ShowTagEditDialog(Decks));
		public ICommand MoveToArenaCommand => new Command(() =>
		{
			MainWindow.MoveDecksToArena(Decks);
			OnPropertyChanged(nameof(MoveToArenaVisibility));
			OnPropertyChanged(nameof(MoveToConstructedVisibility));
		});
		public ICommand MoveToConstructedCommand => new Command(() =>
		{
			MainWindow.MoveDecksToConstructed(Decks);
			OnPropertyChanged(nameof(MoveToArenaVisibility));
			OnPropertyChanged(nameof(MoveToConstructedVisibility));
		});
		public ICommand MissingCardsCommand => new Command(() => MainWindow.ShowMissingCardsMessage(Decks.FirstOrDefault(), false).Forget());
		public ICommand UpdateFromWebCommand => new Command(() => MainWindow.UpdateDeckFromWeb(Decks.FirstOrDefault()));
		public ICommand SetDeckUrlCommand => new Command(() => MainWindow.SetDeckUrl(Decks.FirstOrDefault()));
		public ICommand OpenDeckUrlCommand => new Command(() => MainWindow.OpenDeckUrl(Decks.FirstOrDefault()));
		public ICommand ArchiveDeckCommand => new Command(() => MainWindow.ArchiveDecks(Decks));
		public ICommand UnarchiveDeckCommand => new Command(() => MainWindow.UnArchiveDecks(Decks));
		public ICommand DeleteDeckCommand => new Command(() => MainWindow.ShowDeleteDecksMessage(Decks));
		public ICommand CloneDeckCommand => new Command(() => MainWindow.ShowCloneDeckDialog(Decks.FirstOrDefault()));
		public ICommand CloneDeckVersionCommand => new Command(() => MainWindow.ShowCloneDeckVersionDialog(Decks.FirstOrDefault()));
		public ICommand ImportFromWebCommand => new Command(() => MainWindow.ImportDeck());
		public ICommand ImportFromConstructedCommand => new Command(() => MainWindow.ShowImportDialog(false));
		public ICommand ImportFromArenaCommand => new Command(() => MainWindow.StartArenaImporting().Forget());
		public ICommand ImportFromBrawlCommand => new Command(() => MainWindow.ShowImportDialog(true));
		public ICommand ImportFromFileCommand => new Command(() => MainWindow.ImportFromFile());
		public ICommand ImportFromIdStringCommand => new Command(() => MainWindow.ImportFromIdString());
		public ICommand ImportFromClipboardCommand => new Command(() => MainWindow.ImportFromClipboard());
		public ICommand ImportFromLastGameCommand => new Command(() => MainWindow.ImportFromLastGame());
		public ICommand ExportDeckCommand => new Command(() => MainWindow.ExportDeck(Decks.FirstOrDefault()));
		public ICommand ExportFromWebCommand => new Command(() => MainWindow.ExportDeckFromWeb());
		public ICommand SaveToDiskCommand => new Command(() => MainWindow.SaveDecksToDisk(Decks));
		public ICommand IdsToClipboardCommand => new Command(() => MainWindow.ExportIdsToClipboard(Decks.FirstOrDefault()));
		public ICommand NamesToClipboardCommand => new Command(() => MainWindow.ExportCardNamesToClipboard(Decks.FirstOrDefault()));
		public ICommand ScreenshotCommand => new Command(() => MainWindow.ShowScreenshotFlyout());
		public ICommand ArenaStatsCommand => new Command(() => MainWindow.ShowStats(true, false));
		public ICommand ConstructedStatsCommand => new Command(() => MainWindow.ShowStats(false, false));
		public ICommand ReplayFromStatsCommand => new Command(() => MainWindow.ShowStats(false, true));
		public ICommand ReplayFromFileCommand => new Command(() => MainWindow.ShowReplayFromFileDialog());
		public ICommand HsReplayNetCommand => new Command(() => Helper.TryOpenUrl("https://hsreplay.net/?utm_source=hdt&utm_medium=client"));
		public ICommand ClaimAccountCommand => new Command(() => MainWindow.StartClaimAccount());
		public ICommand MyAccountCommand => new Command(() => Helper.TryOpenUrl("https://hsreplay.net/games/mine/?utm_source=hdt&utm_medium=client"));

		public IEnumerable<SortFilterDecks.Tag> DeckTags => MainWindow?.TagControlEdit.Tags ?? new ObservableCollection<SortFilterDecks.Tag>();
		public string SetDeckUrlText => LocUtil.Get(string.IsNullOrEmpty(Decks.FirstOrDefault()?.Url) ? LocLinkNew : LocLink, true);
		public List<GameStats> LatestReplays => LastGames.Instance.Games;
		private IEnumerable<PluginWrapper> PluginsWithMenu => PluginManager.Instance.Plugins.Where(p => p.IsEnabled && p.MenuItem != null);
		public IEnumerable<MenuItem> PluginsMenuItems => PluginsWithMenu.Select(p => p.MenuItem);

		public Visibility ReplaysEmptyVisibility => LatestReplays.Count == 0 ? Visible : Collapsed;
		public Visibility ClaimAccountVisibility => Account.Instance.Status == AccountStatus.Anonymous ? Visible : Collapsed;
		public Visibility MyAccountVisibility => Account.Instance.Status == AccountStatus.Anonymous ? Collapsed : Visible;

		public Visibility PluginsEmptyVisibility => PluginsWithMenu.Any() ? Collapsed : Visible;

		public bool HasSelectedDeck => Decks.Any();
		public Visibility MoveToArenaVisibility => Decks.FirstOrDefault()?.IsArenaDeck ?? true ? Collapsed : Visible;
		public Visibility MoveToConstructedVisibility => Decks.FirstOrDefault()?.IsArenaDeck ?? false ? Visible : Collapsed;
		public Visibility MissingCardsVisibility => Decks.FirstOrDefault()?.MissingCards.Any() ?? false ? Visible : Collapsed;
		public Visibility SetDeckUrlVisibility => Decks.FirstOrDefault()?.IsArenaDeck ?? true ? Collapsed : Visible;
		public Visibility UpdateFromWebVisibility => string.IsNullOrEmpty(Decks.FirstOrDefault()?.Url) ? Collapsed : Visible;
		public Visibility OpenDeckUrlVisibility => string.IsNullOrEmpty(Decks.FirstOrDefault()?.Url) ? Collapsed : Visible;
		public Visibility ArchiveDeckVisibility => Decks.FirstOrDefault()?.Archived ?? true ? Collapsed : Visible;
		public Visibility UnarchiveDeckVisibility => Decks.FirstOrDefault()?.Archived ?? false ? Visible : Collapsed;
		public Visibility SeparatorVisibility => Decks.FirstOrDefault()?.IsArenaDeck ?? true ? Collapsed : Visible;

		public MainWindowMenuViewModel()
		{
			LastGames.Instance.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == "Games")
				{
					OnPropertyChanged(nameof(LatestReplays));
					OnPropertyChanged(nameof(ReplaysEmptyVisibility));
				}
			};

			Account.Instance.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == "Status")
				{
					OnPropertyChanged(nameof(ClaimAccountVisibility));
					OnPropertyChanged(nameof(MyAccountVisibility));
				}
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void DeckMenuOpened()
		{
			OnPropertyChanged(nameof(DeckTags));
			OnPropertyChanged(nameof(MoveToArenaVisibility));
			OnPropertyChanged(nameof(MoveToConstructedVisibility));
			OnPropertyChanged(nameof(MissingCardsVisibility));
			OnPropertyChanged(nameof(SetDeckUrlVisibility));
			OnPropertyChanged(nameof(UpdateFromWebVisibility));
			OnPropertyChanged(nameof(OpenDeckUrlVisibility));
			OnPropertyChanged(nameof(ArchiveDeckVisibility));
			OnPropertyChanged(nameof(UnarchiveDeckVisibility));
			OnPropertyChanged(nameof(SeparatorVisibility));
		}

		public void PluginsMenuOpened()
		{
			OnPropertyChanged(nameof(PluginsMenuItems));
			OnPropertyChanged(nameof(PluginsEmptyVisibility));
		}
	}

	public class StaticCommands
	{
		public static ICommand OpenReplayCommand => new Command<GameStats>(game =>
		{
			if(game != null)
				ReplayLauncher.ShowReplay(game, true).Forget();
		});
	}
}
