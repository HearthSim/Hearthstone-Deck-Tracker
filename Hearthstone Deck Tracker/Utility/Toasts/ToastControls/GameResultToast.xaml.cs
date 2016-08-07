#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	/// <summary>
	/// Interaction logic for GameResultToast.xaml
	/// </summary>
	public partial class GameResultToast : UserControl
	{
		private const int ExpandedHeight = 141;
		private const int ExpandedWidth = 350;
		private readonly GameStats _game;
		private bool _edited;
		private bool _expanded;

		public GameResultToast(string deckName, [NotNull] GameStats game)
		{
			InitializeComponent();
			InitializeComponent();
			DeckName = deckName;
			_game = game;
			ComboBoxResult.ItemsSource = new[] { GameResult.Win, GameResult.Loss };
			ComboBoxFormat.ItemsSource = new[] { Enums.Format.Standard, Enums.Format.Wild };
			ComboBoxGameMode.ItemsSource = new[]
			{
				GameMode.Arena,
				GameMode.Brawl,
				GameMode.Casual,
				GameMode.Friendly,
				GameMode.Practice,
				GameMode.Ranked,
				GameMode.Spectator
			};
		}

		public string DeckName { get; set; }

		public HeroClassWrapper Opponent
		{
			get
			{
				HeroClass heroClass;
				return Enum.TryParse(_game.OpponentHero, out heroClass) ? new HeroClassWrapper(heroClass) : null;
			}
			set
			{
				if(value == null)
					return;
				HeroClass heroClass;
				if(!Enum.TryParse(value.Class, out heroClass))
					return;
				_game.OpponentHero = heroClass.ToString();
				_edited = true;
			}
		}

		public bool FormatSelectionEnabled => Mode == GameMode.Casual || Mode == GameMode.Ranked;

		public Format? Format
		{
			get { return _game.Format; }
			set
			{
				_game.Format = value;
				_edited = true;
			}
		}

		public GameMode Mode
		{
			get { return _game.GameMode; }
			set
			{
				_game.GameMode = value;
				_edited = true;
				OnPropertyChanged(nameof(FormatSelectionEnabled));
			}
		}

		public GameResult Result
		{
			get { return _game.Result; }
			set
			{
				_game.Result = value;
				_edited = true;
			}
		}

		public BitmapImage PlayerClassImage => ImageCache.GetClassIcon(_game.PlayerHero);

		private void RectangleSettings_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			Core.MainWindow.FlyoutOptions.IsOpen = true;
			Core.MainWindow.Options.TreeViewItemTrackerNotifications.IsSelected = true;
			Core.MainWindow.ActivateWindow();
		}

		private void PanelSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Expand();

		private void Expand()
		{
			if(_expanded)
				return;
			_expanded = true;
			PanelSummary.Visibility = Visibility.Collapsed;
			PanelDetailHeader.Visibility = Visibility.Visible;
			PanelDetailBody.Visibility = Visibility.Visible;
			Height = ExpandedHeight;
			Width = ExpandedWidth;
		}

		private void GameResultToast_OnUnloaded(object sender, RoutedEventArgs e)
		{
			if(!_edited)
				return;
			DeckStatsList.Save();
			if(!Config.Instance.HearthStatsAutoUploadNewGames || !HearthStatsAPI.IsLoggedIn)
				return;
			var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == _game.DeckId);
			if(deck == null)
				return;
			if(_game.HasHearthStatsId)
			{
				if(_game.GameMode == GameMode.Arena)
					HearthStatsManager.UpdateArenaMatchAsync(_game, deck, true, true);
				else
					HearthStatsManager.UpdateMatchAsync(_game, deck.GetVersion(_game.PlayerDeckVersion), true, true);
			}
			else
			{
				if(_game.GameMode == GameMode.Arena)
					HearthStatsManager.UploadArenaMatchAsync(_game, deck, true, true).Forget();
				else
					HearthStatsManager.UploadMatchAsync(_game, deck.GetVersion(_game.PlayerDeckVersion), true, true).Forget();
			}
		}

		private void GameResultToast_OnMouseEnter(object sender, MouseEventArgs e)
		{
			if(!_expanded && Cursor != Cursors.Wait)
				Cursor = Cursors.Hand;
		}

		private void GameResultToast_OnMouseLeave(object sender, MouseEventArgs e)
		{
			if(Cursor != Cursors.Wait)
				Cursor = Cursors.Arrow;
		}

		private void RectangleClose_OnMouseDown(object sender, MouseButtonEventArgs e) => ToastManager.ForceCloseToast(this);

		public class HeroClassWrapper
		{
			public HeroClassWrapper(HeroClass heroClass)
			{
				Class = heroClass.ToString();
			}

			public BitmapImage ClassImage => ImageCache.GetClassIcon(Class);

			public string Class { get; }

			public override bool Equals(object obj)
			{
				var hcw = obj as HeroClassWrapper;
				return hcw != null && Equals(hcw);
			}

			protected bool Equals(HeroClassWrapper other) => string.Equals(Class, other.Class);

			public override int GetHashCode() => Class?.GetHashCode() ?? 0;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
