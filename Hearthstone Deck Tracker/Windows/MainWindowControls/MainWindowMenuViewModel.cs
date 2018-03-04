using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Stats;
using static System.Windows.Visibility;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public class MainWindowMenuViewModel : ViewModel
	{
		private const string LocLink = "DeckPicker_ContextMenu_LinkUrl";
		private const string LocLinkNew = "DeckPicker_ContextMenu_LinkNewUrl";

		private IEnumerable<Deck> _decks;
		private bool _loginButtonEnabled = true;
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
		public ICommand EditDeckCommand => new Command(() => MainWindow.ShowDeckEditorFlyout(Decks.FirstOrDefault(), false));
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
		public ICommand ExportDeckCommand => new Command(() => MainWindow.ShowExportFlyout(Decks.FirstOrDefault()));
		public ICommand ExportFromWebCommand => new Command(() => MainWindow.ExportDeckFromWeb());
		public ICommand SaveToDiskCommand => new Command(() => MainWindow.SaveDecksToDisk(Decks));
		public ICommand IdsToClipboardCommand => new Command(() => MainWindow.ExportIdsToClipboard(Decks.FirstOrDefault()));
		public ICommand NamesToClipboardCommand => new Command(() => MainWindow.ExportCardNamesToClipboard(Decks.FirstOrDefault()));
		public ICommand ScreenshotCommand => new Command(() => MainWindow.ShowScreenshotFlyout());
		public ICommand ArenaStatsCommand => new Command(() => MainWindow.ShowStats(true, false));
		public ICommand ConstructedStatsCommand => new Command(() => MainWindow.ShowStats(false, false));
		public ICommand ReplayFromStatsCommand => new Command(() => MainWindow.ShowStats(false, true));
		public ICommand ReplayFromFileCommand => new Command(() => MainWindow.ShowReplayFromFileDialog());
		public ICommand MyReplaysCommand => new Command(() =>
		{
			var url = Helper.BuildHsReplayNetUrl("/games/mine", "menu");
			Helper.TryOpenUrl(url);
		});
		public ICommand DeckHistoryCommand => new Command(() => MainWindow.ShowDeckHistoryFlyout());
		public ICommand ImportFromDeckString => new Command(() => MainWindow.ImportFromClipboard());

		public ICommand LoginCommand => new Command(async () =>
		{
			Helper.OptionsMain.TreeViewItemHSReplayAccount.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
			await HSReplayNetHelper.TryAuthenticate();
		});

		public ICommand MetaCommand => new Command(() => Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("meta", "menu")));

		public ICommand DecksCommand => new Command(() => Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("decks", "menu")));

		public IEnumerable<SortFilterDecks.Tag> DeckTags => MainWindow?.TagControlEdit.Tags ?? new ObservableCollection<SortFilterDecks.Tag>();
		public string SetDeckUrlText => LocUtil.Get(string.IsNullOrEmpty(Decks.FirstOrDefault()?.Url) ? LocLinkNew : LocLink, true);
		public List<GameStats> LatestReplays => LastGames.Instance.Games;
		private IEnumerable<PluginWrapper> PluginsWithMenu => PluginManager.Instance.Plugins.Where(p => p.IsEnabled && p.MenuItem != null);
		public IEnumerable<MenuItem> PluginsMenuItems => PluginsWithMenu.Select(p => p.MenuItem);

		public Visibility ReplaysEmptyVisibility => LatestReplays.Count == 0 ? Visible : Collapsed;
		public Visibility MyReplaysVisibility => Account.Instance.Status == AccountStatus.Anonymous ? Collapsed : Visible;

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
		public Visibility DeckHistoryVisibility => Decks.FirstOrDefault()?.HasVersions ?? false ? Visible : Collapsed;
		public Visibility LoginVisibility => HSReplayNetOAuth.IsFullyAuthenticated ? Collapsed : Visible;

		public bool LoginButtonEnabled
		{
			get => _loginButtonEnabled;
			set
			{
				_loginButtonEnabled = value; 
				OnPropertyChanged();
			}
		}

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
					OnPropertyChanged(nameof(MyReplaysVisibility));
			};

			HSReplayNetOAuth.AccountDataUpdated += UpdateHSReplayNetMenu;
			HSReplayNetOAuth.LoggedOut += UpdateHSReplayNetMenu;
			HSReplayNetHelper.Authenticating += EnableLoginButton;
		}

		private void EnableLoginButton(bool authenticating)
		{
			if(authenticating)
			{
				LoginButtonEnabled = false;
				Task.Run(async () =>
				{
					await Task.Delay(5000);
					LoginButtonEnabled = true;
				}).Forget();
			}
			else
				LoginButtonEnabled = true;
		}

		public void UpdateHSReplayNetMenu()
		{
			OnPropertyChanged(nameof(LoginVisibility));
			OnPropertyChanged(nameof(MyReplaysVisibility));
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
			OnPropertyChanged(nameof(DeckHistoryVisibility));
		}

		public void PluginsMenuOpened()
		{
			OnPropertyChanged(nameof(PluginsMenuItems));
			OnPropertyChanged(nameof(PluginsEmptyVisibility));
		}
	}
}
