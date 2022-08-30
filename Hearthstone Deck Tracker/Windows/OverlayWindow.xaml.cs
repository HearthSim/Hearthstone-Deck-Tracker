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
using Hearthstone_Deck_Tracker.Controls.Overlay;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds;
using Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

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
		private readonly List<FrameworkElement> _hoverableElements = new List<FrameworkElement>();
		private readonly int _offsetX;
		private readonly int _offsetY;
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
		private bool _battlegroundsSessionVisibleTemp;
		private bool _secretsTempVisible;
		private UIElement? _selectedUiElement;
		private bool _uiMovable;

		private OverlayElementBehavior _mulliganNotificationBehavior;
		private OverlayElementBehavior _heroNotificationBehavior;
		private OverlayElementBehavior _bgsTopBarBehavior;
		private OverlayElementBehavior _bgsBobsBuddyBehavior;
		private OverlayElementBehavior _bgsPastOpponentBoardBehavior;
		private OverlayElementBehavior _experienceCounterBehavior;
		private OverlayElementBehavior _mercenariesTaskListBehavior;
		private OverlayElementBehavior _mercenariesTaskListButtonBehavior;

		private const int LevelResetDelay = 500;
		private const int ExperienceFadeDelay = 6000;

		public BattlegroundsSessionViewModel BattlegroundsSessionViewModelVM => Core.Game.BattlegroundsSessionViewModel;

		public MercenariesTaskListViewModel MercenariesTaskListVM { get; } = new MercenariesTaskListViewModel();

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
				{
					_hoverableElements.Add(e);
					RunHoverUpdates();
				}
				else
					_hoverableElements.Remove(e);
			};

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
		public event PropertyChangedEventHandler? PropertyChanged;

		public double PlayerStackHeight => (Config.Instance.PlayerDeckHeight / 100 * Height) / (Config.Instance.OverlayPlayerScaling / 100);
		public double PlayerListHeight => PlayerStackHeight - PlayerLabelsHeight;
		public double PlayerLabelsHeight => CanvasPlayerChance.ActualHeight + CanvasPlayerCount.ActualHeight
			+ LblPlayerFatigue.ActualHeight + LblDeckTitle.ActualHeight + LblWins.ActualHeight + ChancePanelsMargins + PlayerTopDeckLens.ActualHeight + PlayerBottomDeckLens.ActualHeight;

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

		public void ShowOverlay(bool enable)
		{
			if(enable)
			{
				try
				{
					Show();
					RunHoverUpdates();
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
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
			HeroNotificationPanel.MMR = _game.BattlegroundsRatingInfo?.Rating;
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

		internal void ShowBattlegroundsSession()
		{
			BattlegroundsSessionViewModelVM.Update();
			BattlegroundsSession.Show();
		}

		internal void HideBattlegroundsSession()
		{
			if (_battlegroundsSessionVisibleTemp)
				return;
			BattlegroundsSession.Hide();
		}

		internal void ShowLinkOpponentDeckDisplay()
		{
			LinkOpponentDeckDisplay.Show(true);
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
				_mercenariesTaskListBehavior.Show();
		}

		private void HideMercenariesTasks()
		{
			_mercenariesTaskListBehavior.Hide();
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
	}
}
