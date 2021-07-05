#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Hearthstone_Deck_Tracker.Utility;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Analytics;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	///     Interaction logic for OverlayWindow.xaml
	/// </summary>
	// ReSharper disable once RedundantExtendsListEntry
	public partial class OverlayWindow : Window, INotifyPropertyChanged
	{
		private const string LocFatigue = "Overlay_DeckList_Label_Fatigue";
		private const int ChancePanelsMargins = 8;
		private readonly Point[][] _cardMarkPos = new Point[MaxHandSize][];
		private readonly List<CardMarker> _cardMarks = new List<CardMarker>();
		private readonly int _customHeight;
		private readonly int _customWidth;
		private readonly List<UIElement> _debugBoardObjects = new List<UIElement>();
		private readonly GameV2 _game;
		private readonly Dictionary<UIElement, ResizeGrip> _movableElements = new Dictionary<UIElement, ResizeGrip>();
		private readonly List<FrameworkElement> _clickableElements = new List<FrameworkElement>();
		private readonly List<HoverableElement> _hoverableElements = new List<HoverableElement>();
		private readonly int _offsetX;
		private readonly int _offsetY;
		private readonly List<Ellipse> _oppBoard = new List<Ellipse>();
		private readonly List<Ellipse> _playerBoard = new List<Ellipse>();
		private readonly List<Rectangle> _playerHand = new List<Rectangle>();
		private readonly List<Rectangle> _leaderboardIcons = new List<Rectangle>();
		private readonly List<HearthstoneTextBlock> _leaderboardDeadForText = new List<HearthstoneTextBlock>();
		private readonly List<HearthstoneTextBlock> _leaderboardDeadForTurnText = new List<HearthstoneTextBlock>();
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

		private OverlayElementBehavior _mulliganNotificationBehavior;
		private OverlayElementBehavior _heroNotificationBehavior;
		private OverlayElementBehavior _bgsTopBarBehavior;
		private OverlayElementBehavior _bgsBobsBuddyBehavior;
		private OverlayElementBehavior _bgsPastOpponentBoardBehavior;
		private OverlayElementBehavior _experienceCounterBehavior;

		private const int LevelResetDelay = 500;
		private const int ExperienceFadeDelay = 6000;

		Regex BattlegroundsHeroRegex = new Regex(@"TB_BaconShop_HERO_\d\d");

		public OverlayWindow(GameV2 game)
		{
			_game = game;
			InitializeComponent();

			_mulliganNotificationBehavior = new OverlayElementBehavior(MulliganNotificationPanel)
			{
				GetRight = () => 0,
				GetBottom = () => Height * 0.04,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Bottom,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
			};

			_heroNotificationBehavior = new OverlayElementBehavior(HeroNotificationPanel)
			{
				GetRight = () => 0,
				GetBottom = () => Height * 0.04,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Bottom,
				HideCallback = () => {
					ShowBgsTopBar();
				},
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
			};

			_bgsTopBarBehavior = new OverlayElementBehavior(BgsTopBar)
			{
				GetRight = () => 0,
				GetTop = () => 0,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Top,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
			};

			_bgsBobsBuddyBehavior = new OverlayElementBehavior(BobsBuddyDisplay)
			{
				GetLeft = () => Width / 2 - BobsBuddyDisplay.ActualWidth * AutoScaling / 2,
				GetTop = () => 0,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Top,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
			};

			_bgsPastOpponentBoardBehavior = new OverlayElementBehavior(PastOpponentBoardDisplay)
			{
				GetLeft = () => Width / 2 - PastOpponentBoardDisplay.ActualWidth * AutoScaling / 2,
				GetTop = () => 0,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Top,
				EntranceAnimation = AnimationType.Instant,
				ExitAnimation = AnimationType.Instant,
			};

			_experienceCounterBehavior = new OverlayElementBehavior(ExperienceCounter)
			{
				GetRight = () => Height * .35,
				GetTop = () => Height * .9652,
				AnchorSide = Side.Bottom,
				GetScaling = () => AutoScaling,
			};

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

		public double BattlegroundsTileHeight => Height * 0.69 / 8;
		public double BattlegroundsTileWidth => BattlegroundsTileHeight;

		public void ShowOverlay(bool enable)
		{
			if(enable)
			{
				try
				{
					Show();
				}
				catch(InvalidOperationException e)
				{
					Log.Error(e);
				}
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
			if(width < 0 || height < 0)
				return;
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
			User32.SetWindowExStyle(hwnd, User32.WsExToolWindow | User32.WsExNoActivate | User32.WsExTransparent);
		}

		private bool _clickthrough = false;
		private bool SetClickthrough(bool clickthrough)
		{
			if(_clickthrough == clickthrough)
				return false;
			_clickthrough = clickthrough;
			var hwnd = new WindowInteropHelper(this).Handle;
			if(clickthrough)
				User32.SetWindowExStyle(hwnd, User32.WsExTransparent);
			else
				User32.RemoveWindowExStyle(hwnd, User32.WsExTransparent);
			return true;
		}

		public void HideTimers() => LblPlayerTurnTime.Visibility = LblOpponentTurnTime.Visibility = LblTurnTime.Visibility = Hidden;

		public void ShowTimers()
			=>
				LblPlayerTurnTime.Visibility = LblOpponentTurnTime.Visibility = LblTurnTime.Visibility
				= (Config.Instance.HideTimers || _game.IsBattlegroundsMatch) ? Hidden : Visible;

		public void ShowSecrets(List<Card> secrets, bool force = false)
		{
			if((Config.Instance.HideSecrets || _game.IsBattlegroundsMatch) && !force)
				return;

			StackPanelSecrets.Children.Clear();

			foreach(var secret in secrets)
			{
				if(secret.Count <= 0 && Config.Instance.RemoveSecretsFromList)
					continue;
				var cardObj = new Controls.Card();
				cardObj.SetValue(DataContextProperty, secret);
				StackPanelSecrets.Children.Add(cardObj);
			}

			StackPanelSecrets.Visibility = Visible;
		}

		public void HideSecrets() => StackPanelSecrets.Visibility = Collapsed;
		public void UnhideSecrects() => StackPanelSecrets.Visibility = Visible;

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

		public void ShowRestartRequiredWarning() => TextBlockRestartWarning.Visibility = Visible;

		public void HideRestartRequiredWarning() => TextBlockRestartWarning.Visibility = Collapsed;

		private bool _mulliganToastVisible = false;
		internal void ShowMulliganPanel(string shortId, int[] dbfIds, CardClass opponent, bool hasCoin, int playerStarLevel)
		{
			MulliganNotificationPanel.Update(shortId, dbfIds, opponent, hasCoin, playerStarLevel);
			if(MulliganNotificationPanel.ShouldShow())
			{
				_mulliganNotificationBehavior.Show();
				_mulliganToastVisible = true;
			}
		}

		internal void HideMulliganPanel(bool wasClicked)
		{
			if(_mulliganToastVisible)
			{
				_mulliganToastVisible = false;
				Influx.OnMulliganToastClose(wasClicked, MulliganNotificationPanel.HasData);
			}
			_mulliganNotificationBehavior.Hide();
		}

		internal void ShowBattlegroundsHeroPanel(int[] heroIds)
		{
			HeroNotificationPanel.HeroIds = heroIds;
			_heroNotificationBehavior.Show();
		}

		internal void HideBattlegroundsHeroPanel()
		{
			_heroNotificationBehavior.Hide();
		}

		internal void ShowBgsTopBar()
		{
			TurnCounter.Visibility = Config.Instance.ShowBattlegroundsTurnCounter ? Visible : Collapsed;
			BattlegroundsMinionsPanel.Visibility = Config.Instance.ShowBattlegroundsTiers ? Visible : Collapsed;

			_bgsTopBarBehavior.Show();
			ShowBobsBuddyPanel();
		}

		internal void HideBgsTopBar()
		{
			BattlegroundsMinionsPanel.Reset();
			_bgsTopBarBehavior.Hide();
			TurnCounter.UpdateTurn(1);
			HideBobsBuddyPanel();
		}

		internal void ShowLinkOpponentDeckDisplay()
		{
			LinkOpponentDeckDisplay.AutoShown = true;
			LinkOpponentDeckDisplay.Show();
		}

		internal void ShowBobsBuddyPanel()
		{
			if(!Config.Instance.RunBobsBuddy)
				return;
			if(RemoteConfig.Instance.Data?.BobsBuddy?.Disabled ?? false)
				return;
			_bgsBobsBuddyBehavior.Show();
		}

		internal void HideBobsBuddyPanel()
		{
			_bgsBobsBuddyBehavior.Hide();
			BobsBuddyDisplay.ResetDisplays();
		}

		internal void ShowExperienceCounter()
		{
			//_experienceCounterBehavior.Show();
			if(Config.Instance.ShowExperienceCounter)
				ExperienceCounter.Visibility = Visible;
		}

		internal void HideExperienceCounter()
		{
			if(!AnimatingXPBar)
				ExperienceCounter.Visibility = Collapsed;
		}

		public static bool AnimatingXPBar = false;

		internal async Task ExperienceChangedAsync(int experience, int experienceNeeded, int level, int levelChange, bool animate)
		{
			while(_game.CurrentMode == Enums.Hearthstone.Mode.GAMEPLAY && _game.PreviousMode == Enums.Hearthstone.Mode.BACON)
			{
				await Task.Delay(500);
			}
			ExperienceCounter.XPDisplay = string.Format($"{experience}/{experienceNeeded}");
			ExperienceCounter.LevelDisplay = (level + 1).ToString();
			if(animate)
			{
				AnimatingXPBar = true;
				ShowExperienceCounter();
				for(int i = 0; i < levelChange; i++)
				{
					ExperienceCounter.ChangeRectangleFill(1, false);
					await Task.Delay(ExperienceFadeDelay);
					ExperienceCounter.ResetRectangleFill();
					await Task.Delay(LevelResetDelay);
				}
				ExperienceCounter.ChangeRectangleFill((double)experience / (double)experienceNeeded, false);
				await Task.Delay(ExperienceFadeDelay);
				AnimatingXPBar = false;
			}
			else
			{
				ExperienceCounter.ChangeRectangleFill((double)experience / (double)experienceNeeded, true);
			}
			if(_game.CurrentMode != Enums.Hearthstone.Mode.HUB)
				HideExperienceCounter();
		}

		internal void UpdateOpponentDeadForTurns(List<int> turns)
		{
			var index = _game.Entities.Values.Where(x => x.IsHero && x.Info.Turn == 0 && BattlegroundsHeroRegex.IsMatch(x.CardId) && !x.Info.Discarded).Count() - 1;
			foreach(var text in _leaderboardDeadForText)
				text.Text = "";
			foreach(var text in _leaderboardDeadForTurnText)
				text.Text = "";
			foreach(var turn in turns)
			{
				if(index < _leaderboardDeadForText.Count && index < _leaderboardDeadForTurnText.Count && index >= 0)
				{
					_leaderboardDeadForText[index].Text = $"{turn}";
					_leaderboardDeadForTurnText[index].Text = turn == 1 ? LocUtil.Get("Overlay_Battlegrounds_Dead_For_Turn") : LocUtil.Get("Overlay_Battlegrounds_Dead_For_Turns");
				}
				index--;
			}
		}
	}
}
