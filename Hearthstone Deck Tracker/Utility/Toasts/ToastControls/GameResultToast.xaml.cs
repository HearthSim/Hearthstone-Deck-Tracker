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
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class GameResultToast : INotifyPropertyChanged
	{
		private readonly GameStats _game;
		private bool _edited;
		private bool _showDetails;

		public GameResultToast(string deckName, [NotNull] GameStats game)
		{
			DeckName = deckName;
			_game = game;
			Results = new[] { GameResult.Win, GameResult.Loss };
			Formats = new[] { Enums.Format.Standard, Enums.Format.Wild };
			Modes = new[]
			{
				GameMode.Arena,
				GameMode.Brawl,
				GameMode.Casual,
				GameMode.Friendly,
				GameMode.Practice,
				GameMode.Ranked,
				GameMode.Spectator
			};
			InitializeComponent();
		}

		public GameMode[] Modes { get; }

		public Format[] Formats { get; }

		public GameResult[] Results { get; }

		public string DeckName { get; }

		public HeroClassWrapper Opponent
		{
			get => Enum.TryParse(_game.OpponentHero, out HeroClass heroClass) ? new HeroClassWrapper(heroClass) : null;
			set
			{
				if(value == null)
					return;
				if(!Enum.TryParse(value.Class, out HeroClass heroClass))
					return;
				_game.OpponentHero = heroClass.ToString();
				_edited = true;
			}
		}

		public bool FormatSelectionEnabled => Mode == GameMode.Casual || Mode == GameMode.Ranked;

		public Format? Format
		{
			get => _game.Format;
			set
			{
				_game.Format = value;
				_edited = true;
			}
		}

		public GameMode Mode
		{
			get => _game.GameMode;
			set
			{
				_game.GameMode = value;
				_edited = true;
				OnPropertyChanged(nameof(FormatSelectionEnabled));
			}
		}

		public GameResult Result
		{
			get => _game.Result;
			set
			{
				_game.Result = value;
				_edited = true;
			}
		}

		public BitmapImage PlayerClassImage => ImageCache.GetClassIcon(_game.PlayerHero);

		public bool ShowDetails
		{
			get => _showDetails;
			set
			{
				_showDetails = value; 
				OnPropertyChanged();
			}
		}

		private void RectangleSettings_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			Core.MainWindow.FlyoutOptions.IsOpen = true;
			Core.MainWindow.Options.TreeViewItemTrackerNotifications.IsSelected = true;
			Core.MainWindow.ActivateWindow();
		}

		private void PanelSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ShowDetails = true;

		private void GameResultToast_OnUnloaded(object sender, RoutedEventArgs e)
		{
			if(_edited)
				DeckStatsList.Save();
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

			public override bool Equals(object obj) => obj is HeroClassWrapper hcw && Equals(hcw);

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
