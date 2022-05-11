using HearthDb;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsGame: INotifyPropertyChanged
	{
		public BattlegroundsGame()
		{
			InitializeComponent();
		}

		private GameItem? _game = null;

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty HeroNameProperty = DependencyProperty.Register("HeroName", typeof(string), typeof(BattlegroundsGame));
		public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register("Placement", typeof(string), typeof(BattlegroundsGame));
		public static readonly DependencyProperty MMRDeltaProperty = DependencyProperty.Register("MMRDelta", typeof(string), typeof(BattlegroundsGame));
		public static readonly DependencyProperty CardImageProperty = DependencyProperty.Register("CardImage", typeof(CardAssetViewModel), typeof(BattlegroundsGame));
		public static readonly DependencyProperty CrownVisibilityProperty = DependencyProperty.Register("CrownVisibility", typeof(Visibility), typeof(BattlegroundsGame));

		public GameItem? Game
		{
			get { return _game; }
			set
			{
				_game = value;
				OnPropertyChanged();
				UpdateGameProperties();
			}
		}

		public SolidColorBrush PlacementTextBrush
		{
			get
			{
				if(_game == null)
					return new SolidColorBrush(Colors.White);
				return new SolidColorBrush(_game.Placement <= 4 ? Color.FromRgb(109, 235, 108) : Color.FromRgb(236, 105, 105));
			}
		}

		public SolidColorBrush MMRDeltaTextBrush
		{
			get
			{
				var mmrDelta = _game == null ? 0 : _game.RatingAfter - _game.Rating;
				if(mmrDelta == 0)
					return new SolidColorBrush(Colors.White);
				return new SolidColorBrush(mmrDelta > 0 ? Color.FromRgb(139, 210, 134) : Color.FromRgb(236, 105, 105));
			}
		}

		private void UpdateGameProperties()
		{
			if (_game != null)
			{
				var heroCardId = _game.Hero ?? "";
				var heroCard = Database.GetCardFromId(heroCardId);
				if (heroCard?.BattlegroundsSkinParentId > 0)
					heroCard = Database.GetCardFromDbfId(heroCard.BattlegroundsSkinParentId, false);

				SetValue(CardImageProperty, new CardAssetViewModel(heroCard, Utility.Assets.CardAssetType.Tile));
				OnPropertyChanged(nameof(CardImageProperty));

				var heroName = Remote.Config.Data?.BattlegroundsShortNames
					?.Find(sn => sn.DbfId == heroCard?.DbfId)?.ShortName ?? heroCard?.Name;
				SetValue(HeroNameProperty, heroName);
				OnPropertyChanged(nameof(HeroNameProperty));

				// This is fine here since we do not have 11, 12, 13 as placement option
				var placement = (_game.Placement + "th")
					.Replace("1th", "1st")
					.Replace("2th", "2nd")
					.Replace("3th", "3rd");
				SetValue(PlacementProperty, placement);
				OnPropertyChanged(nameof(PlacementProperty));

				var mmrDelta = _game.RatingAfter - _game.Rating;
				var signal = mmrDelta > 0 ? "+" : "";
				SetValue(MMRDeltaProperty, $"{signal}{mmrDelta}");
				OnPropertyChanged(nameof(MMRDeltaProperty));

				SetValue(CrownVisibilityProperty, _game.Placement == 1 ? Visibility.Visible : Visibility.Hidden);
				OnPropertyChanged(nameof(CrownVisibilityProperty));
			}
		}
	}
}
