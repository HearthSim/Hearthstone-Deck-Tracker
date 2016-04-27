#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	///     Interaction logic for OverlayWindow.xaml
	/// </summary>
	// ReSharper disable once RedundantExtendsListEntry
	public partial class OverlayWindow : Window, INotifyPropertyChanged
	{
		private const double RankCoveredMaxLeft = 0.1;
		private const double PlayerRankCoveredMaxHeight = 0.8;
		private const double OpponentRankCoveredMaxTop = 0.12;
		private const int ChancePanelsMargins = 8;
		private readonly Point[][] _cardMarkPos = new Point[MaxHandSize][];
		private readonly List<CardMarker> _cardMarks = new List<CardMarker>();
		private readonly int _customHeight;
		private readonly int _customWidth;
		private readonly List<UIElement> _debugBoardObjects = new List<UIElement>();
		private readonly GameV2 _game;
		private readonly Dictionary<UIElement, ResizeGrip> _movableElements = new Dictionary<UIElement, ResizeGrip>();
		private readonly int _offsetX;
		private readonly int _offsetY;
		private readonly List<Ellipse> _oppBoard = new List<Ellipse>();
		private readonly List<Ellipse> _playerBoard = new List<Ellipse>();
		private readonly List<Rectangle> _playerHand = new List<Rectangle>();
		private bool? _isFriendsListOpen;
		private string _lastToolTipCardId;
		private bool _lmbDown;
		private User32.MouseInput _mouseInput;
		private Point _mousePos;
		private bool _opponentCardsHidden;
		private bool _playerCardsHidden;
		private bool _resizeElement;
		private bool _secretsTempVisible;
		private UIElement _selectedUiElement;
		private bool _uiMovable;

		public OverlayWindow(GameV2 game)
		{
			_game = game;
			InitializeComponent();

			if(Config.Instance.ExtraFeatures && Config.Instance.ForceMouseHook)
				HookMouse();
			ShowInTaskbar = Config.Instance.ShowInTaskbar;
			if(Config.Instance.VisibleOverlay)
				Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");
			_offsetX = Config.Instance.OffsetX;
			_offsetY = Config.Instance.OffsetY;
			_customWidth = Config.Instance.CustomWidth;
			_customHeight = Config.Instance.CustomHeight;
			if(Config.Instance.ShowBatteryLife)
				EnableBatteryMonitor();
			InitializeCollections();
			GridMain.Visibility = Hidden;
			if(User32.GetHearthstoneWindow() != IntPtr.Zero)
				UpdatePosition();
			Update(true);
			UpdateScaling();
			UpdatePlayerLayout();
			UpdateOpponentLayout();
			GridMain.Visibility = Visible;
		}

		private double ScreenRatio => (4.0 / 3.0) / (Width / Height);
		public bool ForceHidden { get; set; }
		public Visibility WarningVisibility { get; set; }
		public List<Card> PlayerDeck => _game.Player.PlayerCardList;
		public List<Card> OpponentDeck => _game.Opponent.OpponentCardList;
		public event PropertyChangedEventHandler PropertyChanged;

		public double PlayerStackHeight => (Config.Instance.PlayerDeckHeight / 100 * Height) / (Config.Instance.OverlayPlayerScaling / 100);
		public double PlayerListHeight => PlayerStackHeight - PlayerLabelsHeight;
		public double PlayerLabelsHeight => CanvasPlayerChance.ActualHeight + CanvasPlayerCount.ActualHeight
			+ LblPlayerFatigue.ActualHeight + LblDeckTitle.ActualHeight + LblWins.ActualHeight + ChancePanelsMargins;

		public VerticalAlignment PlayerStackPanelAlignment
			=> Config.Instance.OverlayCenterPlayerStackPanel ? VerticalAlignment.Center : VerticalAlignment.Top;

		public double OpponentStackHeight => (Config.Instance.OpponentDeckHeight / 100 * Height) / (Config.Instance.OverlayOpponentScaling / 100);
		public double OpponentListHeight => OpponentStackHeight - OpponentLabelsHeight;

		public double OpponentLabelsHeight => CanvasOpponentChance.ActualHeight + CanvasOpponentCount.ActualHeight
											+ LblOpponentFatigue.ActualHeight + LblWinRateAgainst.ActualHeight + ChancePanelsMargins;

		public VerticalAlignment OpponentStackPanelAlignment
			=> Config.Instance.OverlayCenterOpponentStackPanel ? VerticalAlignment.Center : VerticalAlignment.Top;

		public void ShowOverlay(bool enable)
		{
			if(enable)
			{
				Show();
				if(User32.GetForegroundWindow() == new WindowInteropHelper(this).Handle)
					User32.BringHsToForeground();
			}
			else
				Hide();
		}

		public void ForceHide(bool hide)
		{
			ForceHidden = hide;
			UpdatePosition();
		}

		private void SetRect(int top, int left, int width, int height)
		{
			Top = top + _offsetY;
			Left = left + _offsetX;
			Width = (_customWidth == -1) ? width : _customWidth;
			Height = (_customHeight == -1) ? height : _customHeight;
			CanvasInfo.Width = (_customWidth == -1) ? width : _customWidth;
			CanvasInfo.Height = (_customHeight == -1) ? height : _customHeight;
			StackPanelAdditionalTooltips.MaxHeight = Height;
		}

		private void Window_SourceInitialized_1(object sender, EventArgs e)
		{
			var hwnd = new WindowInteropHelper(this).Handle;
			User32.SetWindowExStyle(hwnd, User32.WsExTransparent | User32.WsExToolWindow);
		}


		public void HideTimers() => LblPlayerTurnTime.Visibility = LblOpponentTurnTime.Visibility = LblTurnTime.Visibility = Hidden;

		public void ShowTimers()
			=>
				LblPlayerTurnTime.Visibility =
				LblOpponentTurnTime.Visibility = LblTurnTime.Visibility = Config.Instance.HideTimers ? Hidden : Visible;

		public void ShowSecrets(bool force = false, HeroClass? heroClass = null)
		{
			if(Config.Instance.HideSecrets && !force)
				return;

			StackPanelSecrets.Children.Clear();
			var secrets = heroClass == null ? _game.OpponentSecrets.GetSecrets() : _game.OpponentSecrets.GetDefaultSecrets(heroClass.Value);
			foreach(var id in secrets)
			{
				var cardObj = new Controls.Card();
				var card = Database.GetCardFromId(id.CardId);
				card.Count = id.AdjustedCount(_game);
				cardObj.SetValue(DataContextProperty, card);
				StackPanelSecrets.Children.Add(cardObj);
			}

			StackPanelSecrets.Visibility = Visible;
		}

		public void HideSecrets() => StackPanelSecrets.Visibility = Collapsed;

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if(_mouseInput != null)
				UnHookMouse();
			DisableBatteryMonitor();
		}

		private void OverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			//in addition to setting this in mainwindow_load: (in case of minimized)
			var presentationsource = PresentationSource.FromVisual(this);
			Helper.DpiScalingX = presentationsource?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
			Helper.DpiScalingY = presentationsource?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public bool IsRankConvered(bool requireOpponentRank = false)
		{
			if(Canvas.GetLeft(StackPanelPlayer) < RankCoveredMaxLeft * Width)
			{
				if(Canvas.GetTop(StackPanelPlayer) + StackPanelPlayer.ActualHeight > PlayerRankCoveredMaxHeight * Height)
				{
					Log.Info("Player rank is potentially covered by player deck.");
					return true;
				}
				if(Canvas.GetTop(StackPanelPlayer) < OpponentRankCoveredMaxTop * Height)
				{
					Log.Info("Opponent rank is potentially covered by player deck.");
					if(requireOpponentRank)
						return true;
				}
			}
			if(Canvas.GetLeft(StackPanelOpponent) < RankCoveredMaxLeft * Width)
			{
				if(Canvas.GetTop(StackPanelOpponent) + StackPanelOpponent.ActualHeight > PlayerRankCoveredMaxHeight * Height)
				{
					Log.Info("Player rank is potentially covered by opponent deck.");
					return true;
				}
				if(Canvas.GetTop(StackPanelOpponent) < OpponentRankCoveredMaxTop * Height)
				{
					Log.Info("Opponent rank is potentially covered by opponent deck.");
					if(requireOpponentRank)
						return true;
				}
			}
			Log.Info("No ranks should be covered by any decks.");
			return false;
		}

		public void ShowFriendsListWarning(bool show) => StackPanelFriendsListWarning.Visibility = show ? Visible : Collapsed;

		public void ShowRestartRequiredWarning() => TextBlockRestartWarning.Visibility = Visible;

		public void HideRestartRequiredWarning() => TextBlockRestartWarning.Visibility = Collapsed;
	}
}