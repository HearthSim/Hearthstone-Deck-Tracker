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
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using System.Windows.Controls;
using System.Windows.Input;
using BobsBuddy.Anomalies;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.TrinketPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Anomalies;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Heroes;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Inspiration;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Overlay;
using Hearthstone_Deck_Tracker.Utility.RegionDrawer;
using Hearthstone_Deck_Tracker.Utility.Themes;
using HSReplay.Responses;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	///     Interaction logic for OverlayWindow.xaml
	/// </summary>
	// ReSharper disable once RedundantExtendsListEntry
	public partial class OverlayWindow : Window, INotifyPropertyChanged
	{
		private const int ChancePanelsMargins = 8;
		private readonly Point[][] _cardMarkPos = new Point[MaxHandSize][];
		private readonly List<CardMarker> _cardMarks = new List<CardMarker>();
		private readonly List<UIElement> _debugBoardObjects = new List<UIElement>();
		private readonly GameV2 _game;
		private readonly Dictionary<UIElement, ResizeGrip> _movableElements = new Dictionary<UIElement, ResizeGrip>();
		private readonly List<FrameworkElement> _clickableElements = new List<FrameworkElement>();
		private readonly HashSet<FrameworkElement> _hoverableElements = new HashSet<FrameworkElement>();
		private readonly List<Rectangle> _playerHand = new List<Rectangle>();
		private readonly List<Rectangle> _leaderboardIcons = new List<Rectangle>();
		private readonly List<HearthstoneTextBlock> _leaderboardDeadForText = new List<HearthstoneTextBlock>();
		private readonly List<HearthstoneTextBlock> _leaderboardDeadForTurnText = new List<HearthstoneTextBlock>();
		private bool? _isFriendsListOpen;
		private bool _lmbDown;
		private User32.MouseInput? _mouseInput;
		private Point _mousePos;
		private bool _opponentCardsHidden;
		private bool _playerCardsHidden;
		private bool _resizeElement;
		private bool _secretsTempVisible;
		private UIElement? _selectedUiElement;
		private bool _uiMovable;

		private OverlayElementBehavior _mulliganNotificationBehavior;
		private OverlayElementBehavior _heroNotificationBehavior;
		private OverlayElementBehavior _bgsTopBarBehavior;
		private OverlayElementBehavior _bgsBobsBuddyBehavior;
		private OverlayElementBehavior _bgsTopBarTriggerMaskBehavior;
		private OverlayElementBehavior _bgsPastOpponentBoardBehavior;
		private OverlayElementBehavior _experienceCounterBehavior;
		private OverlayElementBehavior _mercenariesTaskListBehavior;
		private OverlayElementBehavior _mercenariesTaskListButtonBehavior;
		private OverlayElementBehavior _tier7PreLobbyBehavior;
		private OverlayElementBehavior _constructedMulliganGuidePreLobbyBehaviour;
		private OverlayElementBehavior _battlegroundsInspirationBehavior;

		private const int LevelResetDelay = 500;
		private const int ExperienceFadeDelay = 6000;
		public OverlayOpacityMask OpacityMaskOverlay { get; } = new();

		public BattlegroundsCompsGuidesViewModel BattlegroundsCompsGuidesVM { get; } = new();
		public BattlegroundsMinionsViewModel BattlegroundsMinionsVM { get; } = new();
		public BattlegroundsSessionViewModel BattlegroundsSessionViewModelVM => Core.Game.BattlegroundsSessionViewModel;
		public BattlegroundsHeroPickingViewModel BattlegroundsHeroPickingViewModel { get; } = new();
		public BattlegroundsQuestPickingViewModel BattlegroundsQuestPickingViewModel { get; } = new();
		public BattlegroundsTrinketPickingViewModel BattlegroundsTrinketPickingViewModel { get; } = new();
		public BattlegroundsInspirationViewModel BattlegroundsInspirationViewModel { get; } = new();

		public BattlegroundsHeroGuideListViewModel BattlegroundsHeroGuideListViewModel { get; } = new();
		public BattlegroundsAnomalyGuideListViewModel BattlegroundsAnomalyGuideListViewModel { get; } = new();

		public ConstructedMulliganGuidePreLobbyViewModel ConstructedMulliganGuidePreLobbyViewModel { get; } = new();
		public ConstructedMulliganGuideViewModel ConstructedMulliganGuideViewModel { get; } = new();

		public MercenariesTaskListViewModel MercenariesTaskListVM { get; } = new MercenariesTaskListViewModel();
		public Tier7PreLobbyViewModel Tier7PreLobbyViewModel { get; } = new Tier7PreLobbyViewModel();

		public List<BoardMinionOverlayViewModel> OppBoard { get; } = new List<BoardMinionOverlayViewModel>(MaxBoardSize);
		public List<BoardMinionOverlayViewModel> PlayerBoard { get; } = new List<BoardMinionOverlayViewModel>(MaxBoardSize);

		private Dictionary<ItemsControl, List<Ellipse>> _boardHoverTargets = new Dictionary<ItemsControl, List<Ellipse>>();
		private List<Ellipse> GetBoardHoverTargets(ItemsControl container)
		{
			if(_boardHoverTargets.TryGetValue(container, out var targets))
				return targets;

			targets = new List<Ellipse>();
			for(int i = 0; i < MaxBoardSize; i++)
			{
				var presenter = (ContentPresenter)container.ItemContainerGenerator.ContainerFromIndex(i);
				presenter.ApplyTemplate();
				var ellipse = Helper.FindVisualChildren<BoardMinionOverlayView>(presenter).FirstOrDefault()?.Ellipse;
				if(ellipse != null)
					targets.Add(ellipse);
			}

			if(targets.Any())
				_boardHoverTargets[container] = targets;

			return targets;
		}

		public OverlayWindow(GameV2 game)
		{
			// These need to be set before InitializeComponent is called
			OverlayExtensions.OnRegisterHitTestVisible += (e, clickable) =>
			{
				if(clickable)
					_clickableElements.Add(e);
				else
					_clickableElements.Remove(e);
			};

			OverlayExtensions.OnRegisterHoverVisible += (e, hoverable) =>
			{
				if(hoverable)
					_hoverableElements.Add(e);
				else
					_hoverableElements.Remove(e);
			};

			StartInteractivityUpdates();

			OverlayExtensions.OnToolTipChanged += SetTooltip;

			_game = game;

			for(int i = 0; i < MaxBoardSize; i++)
			{
				OppBoard.Add(new BoardMinionOverlayViewModel());
				PlayerBoard.Add(new BoardMinionOverlayViewModel(AbilityAlignment.Bottom));
			}

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
					ShowBgsTopBarAndBobsBuddyPanel();
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

			_bgsTopBarTriggerMaskBehavior = new OverlayElementBehavior(BgsTopBarMask)
			{
				GetRight = () => 0,
				GetTop = () => 0,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Top,
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

			_bgsPastOpponentBoardBehavior = new OverlayElementBehavior(BgsOpponentInfoContainer)
			{
				GetLeft = () => Width / 2 - BgsOpponentInfoContainer.ActualWidth * AutoScaling / 2,
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

			_mercenariesTaskListButtonBehavior = new OverlayElementBehavior(MercenariesTaskListButton)
			{
				GetRight = () => Height * 0.01,
				GetBottom = () => MercenariesButtonOffset,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Right,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
			};

			_mercenariesTaskListBehavior = new OverlayElementBehavior(MercenariesTaskList)
			{
				GetRight = () => Height * 0.01,
				GetBottom = () => MercenariesTaskListButton.ActualHeight * AutoScaling + MercenariesButtonOffset + 8,
				GetScaling = () => AutoScaling,
				AnchorSide = Side.Right,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
			};

			_tier7PreLobbyBehavior = new OverlayElementBehavior(Tier7PreLobby)
			{
				GetLeft = () => Helper.GetScaledXPos(0.079, (int)Width, ScreenRatio),
				GetTop = () => Height * 0.103,
				GetScaling = () => Height / 1080,
				AnchorSide = Side.Top,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
				Fade = true,
				Distance = 50,
			};

			_constructedMulliganGuidePreLobbyBehaviour = new OverlayElementBehavior(ConstructedMulliganGuidePreLobby)
			{
				GetLeft = () => Helper.GetScaledXPos(0.087, (int)Width, ScreenRatio),
				GetTop = () => Height * 0.217,
				GetScaling = () => Height / 1080,
				AnchorSide = Side.Top,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
				Fade = true,
				Distance = 40,
			};

			_battlegroundsInspirationBehavior = new OverlayElementBehavior(BattlegroundsInspiration)
			{
				// Panel is set to 65% width in BattlegroundsInspiration.xaml
				// If the panel if close to or 80% wide, it should be slightly offset to the left to not cover the timer
				GetLeft = () => Helper.GetScaledXPos((1 - 0.65)/2, (int)Width, ScreenRatio),
				GetTop = () => Height * 0.13,
				GetScaling = () => Height / 1080,
				AnchorSide = Side.Top,
				EntranceAnimation = AnimationType.Slide,
				ExitAnimation = AnimationType.Slide,
				Fade = true,
				Distance = 40,
				HideCallback = () =>
				{
					BtnTier7Inspiration.IsEnabled = BattlegroundsInspirationViewModel.HasBeenActivated;
				}
			};

			ShowInTaskbar = Config.Instance.ShowInTaskbar;
			if(Config.Instance.VisibleOverlay)
				Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");
			if(Config.Instance.ShowBatteryLife)
				EnableBatteryMonitor();
			InitializeCollections();
			GridMain.Visibility = Hidden;
			if(User32.GetHearthstoneWindow() != IntPtr.Zero)
			{
				HookGameWindow();
				UpdatePosition();
			}
			Update(true);
			UpdateScaling();
			UpdatePlayerLayout();
			UpdateOpponentLayout();
			GridMain.Visibility = Visible;

			BattlegroundsInspirationViewModel.OnClose += HideBgsInspiration;
			Tier7Trial.OnTrialActivated += () =>
			{
				BattlegroundsMinionsVM.IsInspirationEnabled = _game.IsBattlegroundsMatch;
			};
			DeckList.Instance.ActiveDeckChanged += _ =>
			{
				Update(false);
				Core.UpdatePlayerCards(true);
			};
			ThemeManager.ThemeChanged += () =>
			{
				CanvasOpponentChance.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
				CanvasOpponentCount.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
				CanvasPlayerChance.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
				CanvasPlayerCount.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
			};

			_winEventCallback = OnHearthstoneWindowLocationChange;

			OpacityMaskOverlay.Changed += OpacityMaskOverlay_OnChanged;
		}

		private double ScreenRatio => (4.0 / 3.0) / (Width / Height);
		public event PropertyChangedEventHandler? PropertyChanged;

		public double PlayerStackHeight => (Config.Instance.PlayerDeckHeight / 100 * Height) / (Config.Instance.OverlayPlayerScaling / 100);

		public VerticalAlignment PlayerStackPanelAlignment
			=> Config.Instance.OverlayCenterPlayerStackPanel ? VerticalAlignment.Center : VerticalAlignment.Top;

		public double OpponentStackHeight => (Config.Instance.OpponentDeckHeight / 100 * Height) / (Config.Instance.OverlayOpponentScaling / 100);

		public VerticalAlignment OpponentStackPanelAlignment
			=> Config.Instance.OverlayCenterOpponentStackPanel ? VerticalAlignment.Center : VerticalAlignment.Top;

		public double BattlegroundsTileHeight => Height * 0.69 / 8;
		public double BattlegroundsDuosTileToSpacingRatio = 0.137;
		public double BattlegroundsDuosTileHeight => Height * 0.69 * (1 - BattlegroundsDuosTileToSpacingRatio) / 8;
		public double BattlegroundsDuosSpacingHeight => Height * 0.69 * BattlegroundsDuosTileToSpacingRatio / 3;
		public double BattlegroundsTileWidth => BattlegroundsTileHeight;

		private double MercenariesButtonOffset
		{
			get
			{
				// Avoid covering the "Back" button on narrow resolutions
				if(_game.IsInMenu && ScreenRatio > 0.9)
					return Height * 0.104;
				return Height * 0.05;
			}
		}
		public bool IsViewingBGsTeammate { get; set; }

		private readonly User32.WinEventCallback _winEventCallback;
		private void OnHearthstoneWindowLocationChange(IntPtr hwineventhook, uint eventtype, IntPtr hwnd, uint idobject, long idchild, uint dweventthread, uint dwmseventtime)
		{
			const uint childIdSelf = 0;
			if(idobject != childIdSelf)
				return;
			UpdatePosition();
		}

		private IntPtr _windowHook = IntPtr.Zero;
		internal void HookGameWindow()
		{
			if(_windowHook != IntPtr.Zero)
				return;
			var thread = User32.GetHearthstoneWindowThread();
			if(thread.ProcId == 0)
				return;
			const uint dwFlagsOutOfContextIgnoreSelf = 0x0000 | 0x001 | 0x002;
			const uint eventObjectLocationchange = 0x800B;
			_windowHook = User32.SetWinEventHook(eventObjectLocationchange, eventObjectLocationchange, IntPtr.Zero,
				_winEventCallback, thread.ProcId, thread.ThreadId, dwFlagsOutOfContextIgnoreSelf);
		}

		internal void UnhookGameWindow()
		{
			if(_windowHook == IntPtr.Zero)
				return;
			User32.UnhookWinEvent(_windowHook);
			_windowHook = IntPtr.Zero;
		}

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

		private void SetRect(int top, int left, int width, int height)
		{
			if(width < 0 || height < 0)
				return;
			Top = top;
			Left = left;
			Width = width;
			Height = height;
			CanvasInfo.Width = width;
			CanvasInfo.Height = height;
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
				= (Config.Instance.HideTimers || _game.IsBattlegroundsMatch || _game.IsMercenariesMatch) ? Hidden : Visible;

		public void ShowSecrets(List<Card> secrets, bool force = false)
		{
			if((Config.Instance.HideSecrets || _game.IsBattlegroundsMatch) && !force)
				return;

			StackPanelSecrets.Children.Clear();

			foreach(var secret in secrets)
			{
				if(secret.Count <= 0 && Config.Instance.RemoveSecretsFromList)
					continue;
				StackPanelSecrets.Children.Add(new CardTile { DataContext = new CardTileViewModel(secret) });
			}

			StackPanelSecrets.Visibility = Visible;
		}

		public void HideSecrets() => StackPanelSecrets.Visibility = Collapsed;
		public void UnhideSecrects() => StackPanelSecrets.Visibility = Visible;

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			StopInteractivityUpdates();
			UnhookGameWindow();
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

			Core.UpdatePlayerCards(true);

			OpacityMaskOverlay_OnChanged(); // Called once here to init
		}

		private void OpacityMaskOverlay_OnChanged()
		{
			OpacityMask = OpacityMaskOverlay.Mask;
			if(Config.Instance.ShowCapturableOverlay && Config.Instance.MaskCapturableOverlay)
				Core.Windows.CapturableOverlay?.UpdateOpacityMask(OpacityMaskOverlay);
		}

		public void SetFriendListOpacityMask(bool visible)
		{

			if(visible)
			{
				var regionDrawer = new RegionDrawer(Height, Width, ScreenRatio);
				var rect = regionDrawer.DrawFriendsListRegion();

				OpacityMaskOverlay.AddMaskedRegion("FriendsList", rect);
			}
			else
				OpacityMaskOverlay.RemoveMaskedRegion("FriendsList");
		}

		public void SetCardOpacityMask(BigCardState state)
		{
			if(IsViewingBGsTeammate) return;


			using var _ = OpacityMaskOverlay.StartBatchUpdate();

			var card = Database.GetCardFromId(state.CardId);
			var isFriendly = state.Side == (int)PlayerSide.FRIENDLY;
			var isHand = state.IsHand;

			OpacityMaskOverlay.RemoveMaskedRegion("BigCard");

			var regionDrawer = new RegionDrawer(Height, Width, ScreenRatio);

			// Enemy secret area
			if(card == null && state.ZonePosition > 0 && !isFriendly && !isHand)
			{
				var rects = regionDrawer.DrawSecretCardRegions(state.ZonePosition, isFriendly, state.TooltipHeights.Sum());

				foreach(var rect in rects)
				{
					OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
				}
			}

			if(card == null)
				return;

			if (card.Type is "Minion" or "Location" or "Battleground_Spell" && !isHand)
			{
				var rects = regionDrawer.DrawBoardCardRegions(state.ZoneSize, state.ZonePosition, isFriendly, state.TooltipHeights.Sum(), state.EnchantmentHeights.Sum());

				foreach(var rect in rects)
				{
					OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
				}
			}

			if(card.Type == "Spell" && !isHand)
			{

				var rects = regionDrawer.DrawSecretCardRegions(state.ZonePosition, isFriendly, state.TooltipHeights.Sum());

				foreach(var rect in rects)
				{
					OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
				}
			}

			if(card.Type == "Battleground_Trinket" && !isHand)
			{
				if(card.Id is "BG30_Trinket_1st" or "BG30_Trinket_2nd")
					return;

				var trinkets = isFriendly ? _game.Player.Trinkets : _game.Opponent.Trinkets;

				 var trinketEntity = trinkets.FirstOrDefault(x =>
					 x.HasTag(GameTag.TAG_SCRIPT_DATA_NUM_6) &&
					 x.Card.DbfId == card.DbfId
				 );

				 var hasAttachedCard = trinketEntity?.HasTag(GameTag.BACON_EVOLUTION_CARD_ID) ?? false;

				 var position = trinketEntity?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_6) ?? 0;

				if(position == 0)
					return;

				var rects = position == 3 ?
					regionDrawer.DrawBgHeroTrinketRegions(isFriendly, true, state.TooltipHeights.Sum())
					: regionDrawer.DrawBgTrinketRegions(position, hasAttachedCard, isFriendly, state.TooltipHeights.Sum());

				foreach(var rect in rects)
				{
					OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
				}
			}

			if(card.Type == "Hero Power" && !isHand)
			{

				var rect = regionDrawer.DrawHeroPowerRegion(isFriendly);

				OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
			}

			if(card.Type == "Weapon" && !isHand)
			{

				var rects = regionDrawer.DrawWeaponRegions(isFriendly, state.TooltipHeights.Sum(), state.EnchantmentHeights.Sum());

				foreach(var rect in rects)
				{
					OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
				}
			}

			if(isHand)
			{
				var rects = regionDrawer.DrawHandCardRegions(state.ZoneSize, state.ZonePosition, isFriendly, card.Type, state.TooltipHeights.Sum(), state.EnchantmentHeights.Sum());

				foreach(var rect in rects)
				{
					OpacityMaskOverlay.AddMaskedRegion("BigCard", rect);
				}
			}
		}

		public void SetHeroPickingTooltipMask(int zoneSize, int zonePosition, bool tooltipOnRight, int numCards)
		{
			OpacityMaskOverlay.RemoveMaskedRegion("HeroPickingTooltip");

			if(zoneSize == 0)
				return;

			var regionDrawer = new RegionDrawer(Height, Width, ScreenRatio);

			var rects = regionDrawer.DrawBgHeroPickingTooltipRegion(zoneSize, zonePosition, tooltipOnRight, numCards);

			using var _ = OpacityMaskOverlay.StartBatchUpdate();
			foreach(var rect in rects)
				OpacityMaskOverlay.AddMaskedRegion("HeroPickingTooltip", rect);
		}

		public void SetMulliganAnomalyMask(Card? card)
		{
			OpacityMaskOverlay.RemoveMaskedRegion("MulliganAnomaly");

			if(card == null)
				return;

			var regionDrawer = new RegionDrawer(Height, Width, ScreenRatio);

			var hasAttachedCard = card.GetTag(GameTag.BACON_EVOLUTION_CARD_ID) != 0;

			var rects = regionDrawer.DrawMulliganAnomalyRegions(hasAttachedCard, false);

			using var _ = OpacityMaskOverlay.StartBatchUpdate();
			foreach(var rect in rects)
				OpacityMaskOverlay.AddMaskedRegion("MulliganAnomaly", rect);
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void ShowRestartRequiredWarning() => TextBlockRestartWarning.Visibility = Visible;

		public void HideRestartRequiredWarning() => TextBlockRestartWarning.Visibility = Collapsed;

		private bool _mulliganToastVisible = false;

		internal void ShowMulliganToast(string shortId, int[] dbfIds, Dictionary<string, string>? parameters, bool showingMulliganStats = false)
		{
			MulliganNotificationPanel.Update(shortId, dbfIds, parameters, showingMulliganStats);
			if(MulliganNotificationPanel.ShouldShow())
			{
				_mulliganNotificationBehavior.Show();
				_mulliganToastVisible = true;
			}
		}

		internal void HideMulliganToast(bool wasClicked)
		{
			if(_mulliganToastVisible)
			{
				_mulliganToastVisible = false;
				Influx.OnMulliganToastClose(wasClicked, MulliganNotificationPanel.HasData);
			}
			_mulliganNotificationBehavior.Hide();
		}

		internal void ShowBattlegroundsHeroPanel(int[] heroIds, bool duos, Dictionary<string, string>? parameters)
		{
			HeroNotificationPanel.HeroIds = heroIds;
			HeroNotificationPanel.Duos = duos;
			HeroNotificationPanel.AnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
			HeroNotificationPanel.Parameters = parameters;
			_heroNotificationBehavior.Show();
		}

		internal void HideBattlegroundsHeroPanel()
		{
			_heroNotificationBehavior.Hide();
		}

		internal void ShowBgsTopBar()
		{
			TurnCounter.Visibility = Config.Instance.ShowBattlegroundsTurnCounter ? Visible : Collapsed;
			if(_game.GameEntity?.GetTag(GameTag.TURN) is int turn and > 0)
				Core.Overlay.TurnCounter.UpdateTurn((int)turn / 2);

			var anomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
			var anomalyCardId = anomalyDbfId.HasValue ? Database.GetCardFromDbfId(anomalyDbfId.Value, false)?.Id : null;
			BattlegroundsMinionsVM.AvailableRaces = BattlegroundsUtils.GetAvailableRaces();
			BattlegroundsMinionsVM.IsDuos = _game.IsBattlegroundsDuosMatch;
			BattlegroundsMinionsVM.Anomaly = anomalyCardId;
			BattlegroundsMinionsVM.PreloadCardTiles();


			var gameId = _game.MetaData.ServerInfo?.GameHandle;
			var userHasTier7 = (HSReplayNetOAuth.AccountData?.IsTier7 ?? false) || Tier7Trial.IsTrialForCurrentGameActive(gameId);
			BattlegroundsMinionsVM.IsInspirationEnabled = _game.IsBattlegroundsMatch && userHasTier7;

			IEnumerable<string> heroPowers = _game.Player.Board.Where(x => x.IsHeroPower).Select(x => x.Card.Id);
			if(!heroPowers.Any() && _game.GameEntity?.GetTag(GameTag.STEP) <= (int)Step.BEGIN_MULLIGAN)
			{
				var heroes = Core.Game.Player.PlayerEntities.Where(x => x.IsHero && (x.HasTag(GameTag.BACON_HERO_CAN_BE_DRAFTED) || x.HasTag(GameTag.BACON_SKIN)) && !x.HasTag(GameTag.BACON_LOCKED_MULLIGAN_HERO));
				heroPowers = heroes.Select(x => Database.GetCardFromDbfId(x.GetTag(GameTag.HERO_POWER), collectible: false)?.Id ?? "");
			}
			BattlegroundsMinionsVM.OnHeroPowers(heroPowers);

			if(Config.Instance.ShowBattlegroundsGuides)
			{
				GuidesTabs.Visibility = Visible;
				BattlegroundsMinions.Visibility = Collapsed;
			} else if (Config.Instance.ShowBattlegroundsBrowser)
			{
				GuidesTabs.Visibility = Collapsed;
				BattlegroundsMinions.Visibility = Visible;
			} else {
				GuidesTabs.Visibility = Collapsed;
				BattlegroundsMinions.Visibility = Collapsed;
			}

			BtnTier7Inspiration.IsEnabled = BattlegroundsInspirationViewModel.HasBeenActivated;

			BattlegroundsCompsGuidesVM.OnMatchStart();

			_bgsTopBarBehavior.Show();
			_bgsTopBarTriggerMaskBehavior.Show();
		}

		internal void ShowBgsTopBarAndBobsBuddyPanel()
		{
			ShowBgsTopBar();
			ShowBobsBuddyPanel();
		}

		internal void HideBgsTopBar()
		{
			BattlegroundsMinionsVM.Reset();
			BattlegroundsCompsGuidesVM.OnMatchEnd();
			_bgsTopBarBehavior.Hide();
			_bgsTopBarTriggerMaskBehavior.Hide();
			TurnCounter.UpdateTurn(1);
			HideBobsBuddyPanel();

			BattlegroundsInspirationViewModel.Reset();
			HideBgsInspiration();
			BtnTier7Inspiration.IsEnabled = false;
		}

		internal void ShowBattlegroundsHeroPickingStats(
			IEnumerable<BattlegroundsHeroPickStats.BattlegroundsSingleHeroPickStats> heroStats,
			Dictionary<string, string>? parameters,
			int? minMmr,
			bool anomalyAdjusted
		)
		{
			BattlegroundsHeroPickingViewModel.SetHeroStats(heroStats, parameters, minMmr, anomalyAdjusted);
		}

		internal void InvalidateBattlegroundsHeroPickingStats(int dbfId)
		{
			BattlegroundsHeroPickingViewModel.InvalidateSingleHeroStats(dbfId);
		}

		internal void ShowMulliganGuideStats(IEnumerable<SingleCardStats> stats, int maxRank, Dictionary<string, string>? selectedParams)
		{
			ConstructedMulliganGuideViewModel.SetMulliganData(stats, maxRank, selectedParams);
		}

		internal void HideMulliganGuideStats()
		{
			ConstructedMulliganGuideViewModel.Reset();
		}

		internal void ShowLinkOpponentDeckDisplay()
		{
			LinkOpponentDeckDisplay.Show(true);
		}

		internal void HideLinkOpponentDeckDisplay()
		{
			LinkOpponentDeckDisplay.Hide(true);
		}

		internal void ShowBobsBuddyPanel()
		{
			if(!Config.Instance.RunBobsBuddy)
				return;
			if(Remote.Config.Data?.BobsBuddy?.Disabled ?? false)
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
			// Disabled for the time being with patch 24.2
			//if(Config.Instance.ShowExperienceCounter)
				//ExperienceCounter.Visibility = Visible;
		}

		internal void HideExperienceCounter()
		{
			// Disabled for the time being with patch 24.2
			//if(!AnimatingXPBar)
				//ExperienceCounter.Visibility = Collapsed;
		}
		internal void ShowMercenariesTasksButton()
		{
			_mercenariesTaskListButtonBehavior.Show();
		}

		internal void HideMercenariesTasksButton()
		{
			HideMercenariesTasks();
			_mercenariesTaskListButtonBehavior.Hide();
		}

		private void ShowMercenariesTasks()
		{
			ShowMercenariesTasksButton();
			if(MercenariesTaskListVM.Update())
			{
				_mercenariesTaskListBehavior.Show();
				if(_game.IsMercenariesMatch)
					_game.Metrics.IncrementMercenariesTaskHoverDuringMatch();
			}
		}

		private void HideMercenariesTasks()
		{
			_mercenariesTaskListBehavior.Hide();
		}

		public void ShowBgsInspiration()
		{
			if(!_game.IsBattlegroundsMatch)
				return;
			_battlegroundsInspirationBehavior.Show();
			OpacityMaskOverlay.Disable();
			BtnTier7Inspiration.IsEnabled = false;
		}

		private void HideBgsInspiration()
		{
			_battlegroundsInspirationBehavior.Hide();
			OpacityMaskOverlay.Enable();
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
			var index = _game.BattlegroundsHeroCount() - 1;
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

		private long _update ;
		internal async void UpdateMercenariesOverlay()
		{
			// Debounce
			var ts = DateTime.Now.Ticks;
			_update = ts;
			await Task.Delay(50);
			if(_update != ts)
				return;


			OnPropertyChanged(nameof(MinionMargin));

			var step = _game.GameEntity?.GetTag(GameTag.STEP);
			var showAbilities = _game.IsMercenariesMatch && (step == (int)Step.MAIN_ACTION || step == (int)Step.MAIN_PRE_ACTION);

			var oppAbilities = showAbilities && Config.Instance.ShowMercsOpponentAbilityIcons ? GetMercAbilities(_game.Opponent) : null;
			var playerAbilities = showAbilities && Config.Instance.ShowMercsPlayerAbilityIcons ? GetMercAbilities(_game.Player) : null;

			for(int i = 0; i < OppBoard.Count; i++)
			{
				OppBoard[i].Width = MinionWidth;
				OppBoard[i].Height = BoardHeight;
				OppBoard[i].Margin = MinionMargin;
				OppBoard[i].MercenariesAbilities = oppAbilities?.ElementAtOrDefault(i)?.Select(x => new MercenariesAbilityViewModel(x)).ToList();
			}

			for(int i = 0; i < PlayerBoard.Count; i++)
			{
				PlayerBoard[i].Width = MinionWidth;
				PlayerBoard[i].Height = BoardHeight;
				PlayerBoard[i].Margin = MinionMargin;
				PlayerBoard[i].MercenariesAbilities = playerAbilities?.ElementAtOrDefault(i)?.Select(x => new MercenariesAbilityViewModel(x)).ToList();
			}

			UpdateBoardPosition();
		}

		internal void HideMercenariesGameOverlay()
		{
			for(var i = 0; i < MaxBoardSize; i++)
			{
				PlayerBoard[i].MercenariesAbilities = null;
				OppBoard[i].MercenariesAbilities = null;
			}
			ClearMercHover();
		}

		public List<List<MercAbilityData>> GetMercAbilities(Player player)
		{
			return player.Board
				.Where(x => x.IsMinion)
				.OrderBy(x => x.ZonePosition)
				.Select(entity =>
				{
					var dbfId = entity.Card?.DbfId;
					var abilityCards = new List<(Card, bool)>();
					var actualAbilities = player.PlayerEntities
						.Where(x => x.GetTag(GameTag.LETTUCE_ABILITY_OWNER) == entity.Id
									&& !x.HasTag(GameTag.LETTUCE_IS_EQUPIMENT)
									&& !x.HasTag(GameTag.DONT_SHOW_IN_HISTORY)
									&& x.HasCardId
									&& x.Card != null)
						.ToList();
					var staticAbilities = dbfId != null ? Remote.Mercenaries.Data?
						.FirstOrDefault(x => x.ArtVariationIds.Contains(dbfId.Value))?.Specializations.FirstOrDefault()?.Abilities ?? new List<RemoteData.MercenaryAbility>() : new List<RemoteData.MercenaryAbility>();

					var data = new List<MercAbilityData>();
					var max = Math.Min(3, Math.Max(staticAbilities.Count, actualAbilities.Count));
					for(var i = 0; i < max; i++)
					{
						var staticAbility = staticAbilities.ElementAtOrDefault(i);
						var actual = staticAbility != null
							? actualAbilities.FirstOrDefault(x => staticAbility.Tiers.Any(t => t.DbfId == x.Card?.DbfId))
							: actualAbilities.FirstOrDefault(x => data.All(d => (d.Entity?.CardId ?? d.Card?.Id) != x.CardId));
						if(actual != null)
						{
							var active = entity.GetTag(GameTag.LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY) == actual.Id
										|| entity.GetTag(GameTag.LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE) == actual.Id;
							data.Add(new MercAbilityData() { Entity = actual, Active = active });
						}
						else if(staticAbility != null)
						{
							var card = actual?.Card ?? Database.GetCardFromDbfId(staticAbility.Tiers.LastOrDefault()?.DbfId ?? 0, false);
							if(card != null)
							{
								var gameTurn = _game.GameEntity?.GetTag(GameTag.TURN) ?? 0;
								data.Add(new MercAbilityData() { Card = card, GameTurn = gameTurn, HasTiers = staticAbility.Tiers.Count > 1 });
							}
						}
					}
					return data;
				})
				.ToList();
		}

		public class MercAbilityData
		{
			public Entity? Entity { get; set; }
			public Card? Card { get; set; }
			public bool Active { get; set; }
			public int GameTurn { get; set; }
			public bool HasTiers { get; set; }
		}

		private void StackPanelOpponent_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!(e is CustomMouseEventArgs))
				return;
			LinkOpponentDeckDisplay.Show(false);
		}

		private void StackPanelOpponent_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!(e is CustomMouseEventArgs))
				return;
			LinkOpponentDeckDisplay.Hide();
		}

		private void LinkOpponentDeckDisplay_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!(e is CustomMouseEventArgs))
				return;
			LinkOpponentDeckDisplay.Show(false);
		}

		private void LinkOpponentDeckDisplay_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!(e is CustomMouseEventArgs))
				return;
			LinkOpponentDeckDisplay.Hide();
		}

		private bool _showMercTasks;
		private async void MercenariesTaskListButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!(e is CustomMouseEventArgs))
				return;
			_showMercTasks = true;
			await Task.Delay(150);
			if(!_showMercTasks)
				return;
			ShowMercenariesTasks();
		}

		private void MercenariesTaskListButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!(e is CustomMouseEventArgs))
				return;
			_showMercTasks = false;
			HideMercenariesTasks();
		}

		internal void SetDeckPickerState(VisualsFormatType vft, IEnumerable<CollectionDeckBoxVisual?> decksOnPage, bool isModalOpen)
		{
			var decksList = decksOnPage.ToList();
			if(ConstructedMulliganGuidePreLobbyViewModel.DecksOnPage == null || !decksList.Equals(ConstructedMulliganGuidePreLobbyViewModel.DecksOnPage))
				ConstructedMulliganGuidePreLobbyViewModel.DecksOnPage = decksList.ToList();
			ConstructedMulliganGuidePreLobbyViewModel.VisualsFormatType = vft;
			ConstructedMulliganGuidePreLobbyViewModel.IsModalOpen = isModalOpen;
		}

		internal void SetBaconState(SelectedBattlegroundsGameMode mode, bool isAnyOpen)
		{
			Tier7PreLobbyViewModel.BattlegroundsGameMode = mode;
			Tier7PreLobbyViewModel.IsModalOpen = !_game.QueueEvents.IsInQueue && isAnyOpen;
			BattlegroundsSessionViewModelVM.BattlegroundsGameMode = mode;
			UpdateTier7PreLobbyVisibility();
		}

		internal void SetConstructedQueue(bool inQueue)
		{
			ConstructedMulliganGuidePreLobbyViewModel.IsInQueue = inQueue;
		}

		internal void SetBaconQueue(bool isAnyOpen)
		{
			UpdateTier7PreLobbyVisibility();
		}

		internal void SetChoicesVisible(bool choicesVisible)
		{
			BattlegroundsTrinketPickingViewModel.ChoicesVisible = choicesVisible;
		}

		private void BgsInspirationCover_OnMouseDown(object sender, MouseButtonEventArgs e) => HideBgsInspiration();

		private void BtnBgsInspiration_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			ShowBgsInspiration();
			_game.Metrics.BattlegroundsInspirationToggleClicks++;
		}
	}
}
