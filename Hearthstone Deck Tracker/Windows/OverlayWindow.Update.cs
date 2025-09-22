using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;
using static HearthDb.Enums.GameTag;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Animations;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System.Collections.Generic;
using System.Windows.Input;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using static HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private async void SetTopmost()
		{
			if(User32.GetHearthstoneWindow() == IntPtr.Zero)
			{
				Log.Info("Hearthstone window not found");
				return;
			}

			for(var i = 0; i < 20; i++)
			{
				var isTopmost = User32.IsTopmost(new WindowInteropHelper(this).Handle);
				if(isTopmost)
				{
					Log.Info($"Overlay is topmost after {i + 1} tries.");
					return;
				}

				Topmost = false;
				Topmost = true;
				await Task.Delay(250);
			}

			Log.Info("Could not set overlay as topmost");
		}

		public void OnHearthstoneFocused()
		{
			Update(true);

			if(_game.CurrentMode == Mode.BACON)
			{
				Tier7PreLobbyViewModel.OnFocus();
			}
			if(_game.CurrentMode == Mode.DRAFT)
			{
				ArenaPreDraftViewModel.OnFocus();
			}
		}

		public void Update(bool refresh)
		{
			if (refresh)
			{
				SetTopmost();
				UpdateVisibilities();
			}

			var opponentHandCount = _game.Opponent.HandCount;
			for (var i = 0; i < 10; i++)
			{
				if (i < opponentHandCount)
				{
					var entity = _game.Opponent.Hand.FirstOrDefault(x => x.GetTag(ZONE_POSITION) == i + 1);
					if(entity == null)
						continue;
					if(!Config.Instance.HideOpponentCardAge)
						_cardMarks[i].UpdateCardAge(entity.Info.Turn);
					else
						_cardMarks[i].UpdateCardAge(null);
					if(!Config.Instance.HideOpponentCardMarks)
					{
						if(entity.HasCardId && !entity.Info.Hidden && entity.Info.CardMark != CardMark.Coin)
						{
							_cardMarks[i].UpdateSource(entity.Card, CardMarker.SourceType.Known);
							if(entity.Info.CardMark == CardMark.Returned)
							{
								_cardMarks[i].UpdateIcon(entity.Info.CardMark);
							}
							else if(entity.Info.CopyOfCardId is not null)
							{
								_cardMarks[i].UpdateIcon(CardMark.None);
							}
						}
						else
						{
							_cardMarks[i].UpdateIcon(entity.Info.CardMark);
							if(entity.Info.CardMark == CardMark.Created)
							{
								var creatorId = entity.Info.GetCreatorId();
								if(creatorId > 0 && _game.Entities.TryGetValue(creatorId, out var creator))
									_cardMarks[i].UpdateSource(creator.Card, CardMarker.SourceType.CreatedBy);
								else
									_cardMarks[i].UpdateSource(null, null);
							}
							else if(entity.Info.GetDrawerId() != null)
							{
								var drawerId = entity.Info.GetDrawerId();
								if(drawerId > 0 && _game.Entities.TryGetValue(drawerId ?? 0, out var drawer))
								{
									var blacklist = GetDrawBlacklist();
									if(!blacklist.Contains(drawer.Card.DbfId))
									{
										_cardMarks[i].UpdateSource(drawer.Card, CardMarker.SourceType.DrawnBy);
									}
									else
									{
										_cardMarks[i].UpdateSource(null, null);
										_cardMarks[i].UpdateIcon(CardMark.None);
									}
								}
								else
									_cardMarks[i].UpdateSource(null, null);
							}
							else
								_cardMarks[i].UpdateSource(null, null);
						}
						_cardMarks[i].UpdateCostReduction(entity.Info.CostReduction);
					}
					else
					{
						_cardMarks[i].UpdateIcon(CardMark.None);
						_cardMarks[i].UpdateSource(null, null);
					}
					_cardMarks[i].Visibility = _game.IsInMenu || _game.IsBattlegroundsMatch || !_game.IsMulliganDone || Config.Instance.HideOpponentCardAge && Config.Instance.HideOpponentCardMarks
												   ? Hidden : Visible;
				}
				else
					_cardMarks[i].Visibility = Collapsed;
			}

			var oppBoard = Core.Game.Opponent.Board.Where(x => x.TakesBoardSlot).OrderBy(x => x.GetTag(ZONE_POSITION)).ToList();
			var playerBoard = Core.Game.Player.Board.Where(x => x.TakesBoardSlot).OrderBy(x => x.GetTag(ZONE_POSITION)).ToList();
			UpdateMouseOverDetectionRegions(oppBoard, playerBoard);
			if(!_game.IsInMenu && (_game.IsMulliganDone || _game.IsBattlegroundsMatch || _game.IsMercenariesMatch) && User32.IsHearthstoneInForeground() && IsVisible)
				DetectMouseOver(playerBoard, oppBoard);
			else
				FlavorTextVisibility = Collapsed;

			StackPanelPlayer.Opacity = Config.Instance.PlayerOpacity / 100;
			StackPanelOpponent.Opacity = Config.Instance.OpponentOpacity / 100;
			SecretsContainer.Opacity = Config.Instance.SecretsOpacity / 100;
			Opacity = Config.Instance.OverlayOpacity / 100;

			var inBattlegrounds = _game.IsBattlegroundsMatch || Core.Game.CurrentMode == Mode.BACON;
			var inMercenaries = _game.IsMercenariesMatch;
			var hideDeck = Config.Instance.HideDecksInOverlay || inBattlegrounds || inMercenaries || (Config.Instance.HideInMenu && _game.IsInMenu);

			if (!_playerCardsHidden)
			{
				StackPanelPlayer.Visibility = (hideDeck && !_uiMovable) || inBattlegrounds ? Collapsed : Visible;
			}

			if (!_opponentCardsHidden)
			{
				StackPanelOpponent.Visibility = (hideDeck && !_uiMovable) || inBattlegrounds ? Collapsed : Visible;
			}

			CanvasPlayerChance.Visibility = Config.Instance.HideDrawChances ? Collapsed : Visible;
			CanvasPlayerCount.Visibility = Config.Instance.HidePlayerCardCount ? Collapsed : Visible;

			CanvasOpponentChance.Visibility = Config.Instance.HideOpponentDrawChances ? Collapsed : Visible;
			CanvasOpponentCount.Visibility = Config.Instance.HideOpponentCardCount ? Collapsed : Visible;

			if (_game.IsInMenu && !_uiMovable)
				HideTimers();

			ListViewOpponent.Visibility = Config.Instance.HideOpponentCards ? Collapsed : Visible;
			ListViewPlayer.Visibility = Config.Instance.HidePlayerCards ? Collapsed : Visible;

			var gameStarted = !_game.IsInMenu && _game.SetupDone && _game.Player.PlayerEntities.Any();
			SetCardCount(_game.Player.HandCount, !gameStarted ? 30 : _game.Player.DeckCount);

			SetOpponentCardCount(_game.Opponent.HandCount, !gameStarted || !_game.IsMulliganDone ? 30 - _game.Opponent.HandCount : _game.Opponent.DeckCount);


			LblWins.Visibility = Config.Instance.ShowDeckWins && _game.IsUsingPremade ? Visible : Collapsed;
			LblDeckTitle.Visibility = Config.Instance.ShowDeckTitle && _game.IsUsingPremade ? Visible : Collapsed;
			LblWinRateAgainst.Visibility = Config.Instance.ShowWinRateAgainst && _game.IsUsingPremade
											   ? Visible : Collapsed;

			var showWarning = !_game.IsInMenu && DeckList.Instance.ActiveDeckVersion != null && DeckManager.NotFoundCards.Any() && DeckManager.IgnoredDeckId != DeckList.Instance.ActiveDeckVersion.DeckId;
			StackPanelWarning.Visibility = showWarning ? Visible : Collapsed;
			if(showWarning)
			{
				LblWarningCards.Text = string.Join(", ", DeckManager.NotFoundCards.Take(Math.Min(DeckManager.NotFoundCards.Count, 3)).Select(c => c.LocalizedName));
				if(DeckManager.NotFoundCards.Count > 3)
					LblWarningCards.Text += ", ...";
			}

			if(_game.IsInMenu || !inBattlegrounds)
			{
				HideBgsTopBar();
			}

			UpdateActiveEffects();

			UpdateCounters();

			UpdatePlayerResourcesWidgetVisibility();

			UpdateIcons();

			SetDeckTitle();
			SetWinRates();

			UpdateElementSizes();
			UpdateElementPositions();

			if (Core.Windows.PlayerWindow.Visibility == Visible)
				Core.Windows.PlayerWindow.Update();
			if (Core.Windows.OpponentWindow.Visibility == Visible)
				Core.Windows.OpponentWindow.Update();

		}

		private List<int> GetDrawBlacklist()
		{
			return Remote.Config.Data?.DrawCardBlacklist?.WhereNotNull().Select(obj => obj.DbfId).ToList() ?? new List<int>();
		}

		private void UpdateActiveEffects()
		{
			var inBattlegrounds = _game.IsBattlegroundsMatch;

			if(_game.IsInMenu || !_game.IsMulliganDone || inBattlegrounds)
			{
				PlayerActiveEffects.Visibility = Collapsed;
				OpponentActiveEffects.Visibility = Collapsed;
			}
			else
			{
				PlayerActiveEffects.Visibility = Config.Instance.HidePlayerActiveEffects ? Collapsed : Visible;
				OpponentActiveEffects.Visibility = Config.Instance.HideOpponentActiveEffects ? Collapsed : Visible;
			}
		}

		private void UpdateCounters()
		{
			if(_game.IsInMenu || !_game.IsMulliganDone)
			{
				PlayerCounters.Visibility = Collapsed;
				OpponentCounters.Visibility = Collapsed;
			}
			else
			{
				PlayerCounters.Visibility = Config.Instance.HidePlayerCounters ? Collapsed : Visible;
				OpponentCounters.Visibility = Config.Instance.HideOpponentCounters || _game.IsBattlegroundsMatch ? Collapsed : Visible;
			}
		}

		private void UpdatePlayerResourcesWidgetVisibility()
		{
			if(_game.IsInMenu || !_game.IsMulliganDone || _game.IsBattlegroundsMatch)
			{
				PlayerResourcesWidget.Visibility = Collapsed;
				OpponentResourcesWidget.Visibility = Collapsed;
			}
			else
			{
				PlayerResourcesWidget.Visibility = Config.Instance.HidePlayerMaxResourcesWidget ? Collapsed : Visible;
				OpponentResourcesWidget.Visibility = Config.Instance.HideOpponentMaxResourcesWidget ? Collapsed : Visible;
			}
		}

		private void UpdateIcons()
		{
			var inBattlegrounds = _game.IsBattlegroundsMatch;
			var inMercenaries = _game.IsMercenariesMatch;

			IconBoardAttackPlayer.Visibility = Config.Instance.HidePlayerAttackIcon || _game.IsInMenu || !_game.IsMulliganDone || inBattlegrounds || inMercenaries ? Collapsed : Visible;
			IconBoardAttackOpponent.Visibility = Config.Instance.HideOpponentAttackIcon || _game.IsInMenu || !_game.IsMulliganDone || inBattlegrounds || inMercenaries ? Collapsed : Visible;

			// do the calculation if at least one of the icons is visible
			if (_game.SetupDone && (IconBoardAttackPlayer.Visibility == Visible || IconBoardAttackOpponent.Visibility == Visible))
			{
				var board = new BoardState();
				TextBlockPlayerAttack.Text = board.Player.Damage.ToString();
				TextBlockOpponentAttack.Text = board.Opponent.Damage.ToString();
			}
		}

		public void UpdateVisibility()
		{
			var isHearthstoneInForeground = User32.IsHearthstoneInForeground();

			//hide the overlay depending on options
			var visible = !((Config.Instance.HideInBackground && !isHearthstoneInForeground && !_game.IsInMenu)
			                || (Config.Instance.HideMenuOverlayInBackground && !isHearthstoneInForeground && _game.IsInMenu)
			                || (Config.Instance.HideOverlayInSpectator && _game.CurrentGameMode == GameMode.Spectator)
			                || Config.Instance.HideOverlay
			                || Helper.GameWindowState == WindowState.Minimized);
			var updatePosition = visible && !IsVisible;
			ShowOverlay(visible);
			if(updatePosition)
				UpdatePosition();
		}

		public void UpdatePosition()
		{
			var hsRect = User32.GetHearthstoneRect(true);

			//hs window has height 0 if it just launched, screwing things up if the tracker is started before hs is.
			//this prevents that from happening.
			if(hsRect.Height == 0 || (Visibility != Visible && Core.Windows.CapturableOverlay == null))
				return;

			var prevWidth = Width;
			var prevHeight = Height;
			SetRect(hsRect.Top, hsRect.Left, hsRect.Width, hsRect.Height);
			if(Width != prevWidth || Height != prevHeight)
			{
				OnPropertyChanged(nameof(BoardWidth));
				OnPropertyChanged(nameof(BoardHeight));
				OnPropertyChanged(nameof(MinionWidth));
				OnPropertyChanged(nameof(CardWidth));
				OnPropertyChanged(nameof(CardHeight));
				OnPropertyChanged(nameof(MercAbilityHeight));
				OnPropertyChanged(nameof(MinionMargin));
				for(int i = 0; i < OppBoard.Count; i++)
				{
					OppBoard[i].Width = MinionWidth;
					OppBoard[i].Height = BoardHeight;
					OppBoard[i].Margin = MinionMargin;
				}

				for(int i = 0; i < PlayerBoard.Count; i++)
				{
					PlayerBoard[i].Width = MinionWidth;
					PlayerBoard[i].Height = BoardHeight;
					PlayerBoard[i].Margin = MinionMargin;
				}
			}

			ApplyAutoScaling();
			UpdateElementSizes();
			UpdateElementPositions();

			if(_game is { IsBattlegroundsMatch: true, IsBattlegroundsHeroPickingDone: false })
				Core.Overlay.SetAnomalyGuidesMulliganTrigger();
		}

		internal void UpdateBattlegroundsOverlay()
		{
			try
			{
				var fadeBgsMinionsList = false;
				var turn = _game.GetTurnNumber();
				_leaderboardDeadForText.ForEach(x => x.Visibility = Visibility.Collapsed);
				_leaderboardDeadForTurnText.ForEach(x => x.Visibility = Visibility.Collapsed);
				if(turn == 0)
					return;
				var shouldShowOpponentInfo = false;
				if(_leaderboardHoveredEntityId is int heroEntityId)
				{
					fadeBgsMinionsList = true;
					_leaderboardDeadForText.ForEach(x => x.Visibility = Visibility.Visible);
					_leaderboardDeadForTurnText.ForEach(x => x.Visibility = Visibility.Visible);

					// check if it's the team mate
					Core.Game.Entities.TryGetValue(heroEntityId, out var entity);
					var state = _game.GetBattlegroundsBoardStateFor(heroEntityId);
					BgsOpponentInfo.Update(heroEntityId, state, turn);
					shouldShowOpponentInfo = !(entity != null && (
						entity.IsControlledBy(_game.Player.Id) ||
						(
							Core.Game.IsBattlegroundsDuosMatch &&
							entity.GetTag(GameTag.BACON_DUO_TEAM_ID)
							== Core.Game.PlayerEntity?.GetTag(GameTag.BACON_DUO_TEAM_ID)
						)
					));
				}

				if(shouldShowOpponentInfo)
				{
					BgsOpponentInfo.Visibility = Visibility.Visible;
					BgsOpponentInfo.UpdateLayout();
					_bgsBobsBuddyBehavior.Hide();
					_bgsPastOpponentBoardBehavior.Show();
				}
				else
				{
					BgsOpponentInfo.Visibility = Visibility.Collapsed;
					_bgsPastOpponentBoardBehavior.Hide();
					BgsOpponentInfo.ClearLastKnownBoard();
					ShowBobsBuddyPanelDelayed();
				}

				// Only fade the minions, if we're out of mulligan
				if(_game.GameEntity?.GetTag(GameTag.STEP) <= (int)Step.BEGIN_MULLIGAN)
					fadeBgsMinionsList = false;
				BgsTopBar.Opacity = fadeBgsMinionsList ? 0.3 : 1;
				BobsBuddyDisplay.Opacity = fadeBgsMinionsList ? 0.3 : 1;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight)
			=> PointInsideControl(pos, actualWidth, actualHeight, new Thickness(0));

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight, Thickness margin)
			=> pos.X > 0 - margin.Left && pos.X < actualWidth + margin.Right && (pos.Y > 0 - margin.Top && pos.Y < actualHeight + margin.Bottom);

		internal void UpdateTurnTimer(TimerState timerState)
		{
			if((timerState.PlayerSeconds <= 0 && timerState.OpponentSeconds <= 0) || _game.CurrentMode != Mode.GAMEPLAY)
				return;
			ShowTimers();
			var seconds = (int)Math.Abs(timerState.Seconds);
			LblTurnTime.Text = double.IsPositiveInfinity(timerState.Seconds) ? "\u221E" : $"{seconds / 60 % 60:00}:{seconds % 60:00}";
			LblTurnTime.Fill = timerState.Seconds < 0 ? Brushes.LimeGreen : Brushes.White;
			LblPlayerTurnTime.Text = $"{timerState.PlayerSeconds / 60 % 60:00}:{timerState.PlayerSeconds % 60:00}";
			LblOpponentTurnTime.Text = $"{timerState.OpponentSeconds / 60 % 60:00}:{timerState.OpponentSeconds % 60:00}";
		}

		public void UpdateScaling()
		{
			StackPanelPlayer.RenderTransform = new ScaleTransform(Config.Instance.OverlayPlayerScaling / 100,
																  Config.Instance.OverlayPlayerScaling / 100);
			StackPanelOpponent.RenderTransform = new ScaleTransform(Config.Instance.OverlayOpponentScaling / 100,
																	Config.Instance.OverlayOpponentScaling / 100);
			SecretsContainer.RenderTransform = new ScaleTransform(Config.Instance.SecretsPanelScaling, Config.Instance.SecretsPanelScaling);
			LinkOpponentDeckDisplay.RenderTransform = new ScaleTransform(Config.Instance.OverlayOpponentScaling / 100,
																	Config.Instance.OverlayOpponentScaling / 100);
			BattlegroundsSession.RenderTransform = new ScaleTransform(Config.Instance.OverlaySessionRecapScaling / 100,
																	Config.Instance.OverlaySessionRecapScaling / 100);
		}

		public double AutoScaling { get; set; } = 1;

		private void UpdateBoardPosition()
		{
			var step = _game.GameEntity?.GetTag(STEP);
			var isMainAction = step == (int)Step.MAIN_ACTION || step == (int)Step.MAIN_POST_ACTION || step == (int)Step.MAIN_PRE_ACTION;
			var mercsToNominate = _game.GameEntity?.HasTag(ALLOW_MOVE_MINION) ?? false;
			var opponentBoardOffset = _game.IsMercenariesMatch && isMainAction && !mercsToNominate ? Height * 0.142 : Height * 0.045;
			Canvas.SetTop(GridOpponentBoard, Height / 2 - GridOpponentBoard.ActualHeight - opponentBoardOffset);

			var playerBoardOffset = _game.IsMercenariesMatch ? isMainAction && !mercsToNominate ? Height * -0.09 : Height * 0.003 : Height * 0.03 ;
			Canvas.SetTop(GridPlayerBoard, Height / 2 - playerBoardOffset);
		}

		private void UpdateElementPositions()
		{
			var BorderStackPanelOpponentTop = Height * Config.Instance.OpponentDeckTop / 100;

			Canvas.SetTop(BorderStackPanelPlayer, Height * Config.Instance.PlayerDeckTop / 100);
			Canvas.SetLeft(BorderStackPanelPlayer, Width * Config.Instance.PlayerDeckLeft / 100 - StackPanelPlayer.ActualWidth * Config.Instance.OverlayPlayerScaling / 100);
			Canvas.SetTop(BorderStackPanelOpponent, BorderStackPanelOpponentTop);
			Canvas.SetLeft(BorderStackPanelOpponent, Width * Config.Instance.OpponentDeckLeft / 100);
			Canvas.SetTop(SecretsContainer, Height * Config.Instance.SecretsTop / 100);
			Canvas.SetLeft(SecretsContainer, Width * Config.Instance.SecretsLeft / 100);
			Canvas.SetTop(LblTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 - 5);
			Canvas.SetLeft(LblTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100);
			Canvas.SetTop(LblOpponentTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 - Config.Instance.TimersVerticalSpacing);
			Canvas.SetLeft(LblOpponentTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
			Canvas.SetTop(LblPlayerTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
			Canvas.SetLeft(LblPlayerTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
			Canvas.SetTop(PlayerActiveEffects, Height * Config.Instance.PlayerActiveEffectsVertical / 100);
			Canvas.SetLeft(PlayerActiveEffects, Helper.GetScaledXPos(Config.Instance.PlayerActiveEffectsHorizontal / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(OpponentActiveEffects, Height - (OpponentActiveEffects.ActualHeight * _activeEffectsScale + Height * Config.Instance.OpponentActiveEffectsVertical / 100));
			Canvas.SetLeft(OpponentActiveEffects, Helper.GetScaledXPos(Config.Instance.OpponentActiveEffectsHorizontal / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(PlayerCounters, Height * Config.Instance.PlayerCountersVertical / 100);
			Canvas.SetLeft(PlayerCounters, Helper.GetScaledXPos(Config.Instance.PlayerCountersHorizontal / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(OpponentCounters, Height - (OpponentCounters.ActualHeight * _activeEffectsScale + Height * Config.Instance.OpponentCountersVertical / 100));
			Canvas.SetLeft(OpponentCounters, Helper.GetScaledXPos(Config.Instance.OpponentCountersHorizontal / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(PlayerResourcesWidget, Height * Config.Instance.PlayerMaxResourcesVertical / 100);
			Canvas.SetLeft(PlayerResourcesWidget, Helper.GetScaledXPos(Config.Instance.PlayerMaxResourcesHorizontal / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(OpponentResourcesWidget, Height * Config.Instance.OpponentMaxResourcesVertical / 100);
			Canvas.SetLeft(OpponentResourcesWidget, Helper.GetScaledXPos(Config.Instance.OpponentMaxResourcesHorizontal / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(IconBoardAttackPlayer, Height * Config.Instance.AttackIconPlayerVerticalPosition / 100);
			Canvas.SetLeft(IconBoardAttackPlayer, Helper.GetScaledXPos(Config.Instance.AttackIconPlayerHorizontalPosition / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(IconBoardAttackOpponent, Height * Config.Instance.AttackIconOpponentVerticalPosition / 100);
			Canvas.SetLeft(IconBoardAttackOpponent, Helper.GetScaledXPos(Config.Instance.AttackIconOpponentHorizontalPosition / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(BattlegroundsSessionStackPanel, Height * Config.Instance.SessionRecapTop / 100);
			Canvas.SetLeft(BattlegroundsSessionStackPanel, Width * Config.Instance.SessionRecapLeft / 100);
			UpdateBoardPosition();

			Canvas.SetLeft(LinkOpponentDeckDisplay, Width * Config.Instance.OpponentDeckLeft / 100);

			var OpponentStackVisibleHeight = (CanvasOpponentCount
				.ActualHeight + CanvasOpponentChance.ActualHeight + ListViewOpponent
				.ActualHeight + OpponentPackageCardsDeckLens.ActualHeight + OpponentRelatedCardsDeckLens.ActualHeight)
				* Config.Instance.OverlayOpponentScaling / 100;

			if(BorderStackPanelOpponentTop + OpponentStackVisibleHeight + 10 + LinkOpponentDeckDisplay.ActualHeight < Height)
			{
				Canvas.SetTop(LinkOpponentDeckDisplay, BorderStackPanelOpponentTop + OpponentStackVisibleHeight + 10);
			}
			else
			{
				Canvas.SetTop(LinkOpponentDeckDisplay, BorderStackPanelOpponentTop - (LinkOpponentDeckDisplay.ActualHeight* Config.Instance.OverlayOpponentScaling / 100) - 10);
			}

			if (Config.Instance.ShowFlavorText)
			{
				Canvas.SetTop(GridFlavorText, Height - GridFlavorText.ActualHeight - 10);
				Canvas.SetLeft(GridFlavorText, Width - GridFlavorText.ActualWidth - 10);
			}
			var handCount = _game.Opponent.HandCount > 10 ? 10 : _game.Opponent.HandCount;
			var opponentScaling = Config.Instance.OverlayOpponentScaling / 100;
			var opponentOpacity = Config.Instance.OpponentOpacity / 100;
			for (var i = 0; i < handCount; i++)
			{
				_cardMarks[i].Opacity = opponentOpacity;
				_cardMarks[i].ScaleTransform = new ScaleTransform(opponentScaling, opponentScaling);
				var width = _cardMarks[i].Width * Config.Instance.OverlayOpponentScaling / 100;
				var height = 34.0 * Config.Instance.OverlayOpponentScaling / 100;
				Canvas.SetLeft(_cardMarks[i], Helper.GetScaledXPos(_cardMarkPos[handCount - 1][i].X, (int)Width, ScreenRatio) - width / 2);
				Canvas.SetTop(_cardMarks[i], Math.Max(_cardMarkPos[handCount - 1][i].Y * Height - height / 3, 5));
			}
		}

		private double _activeEffectsScale;
		private void UpdateElementSizes()
		{
			OnPropertyChanged(nameof(PlayerStackHeight));
			OnPropertyChanged(nameof(OpponentStackHeight));
			OnPropertyChanged(nameof(SecretsHeight));
			OnPropertyChanged(nameof(BattlegroundsTileHeight));
			OnPropertyChanged(nameof(BattlegroundsTileWidth));

			//Scale attack icons, with height
			var atkWidth = (int)Math.Round(Height * 0.0695, 0);
			var atkFont = (int)Math.Round(Height * 0.0204, 0);
			var atkFontMarginTop = (int)Math.Round(Height * 0.0038, 0);
			IconBoardAttackPlayer.Width = atkWidth;
			IconBoardAttackPlayer.Height = atkWidth;
			TextBlockPlayerAttack.Width = atkWidth;
			TextBlockPlayerAttack.Height = atkWidth;
			IconBoardAttackOpponent.Width = atkWidth;
			IconBoardAttackOpponent.Height = atkWidth;
			TextBlockOpponentAttack.Width = atkWidth;
			TextBlockOpponentAttack.Height = atkWidth;
			TextBlockPlayerAttack.Margin = new Thickness(0, atkFontMarginTop, 0, 0);
			TextBlockOpponentAttack.Margin = new Thickness(0, atkFontMarginTop, 0, 0);

			if(atkFont > 0)
			{
				TextBlockPlayerAttack.FontSize = atkFont;
				TextBlockOpponentAttack.FontSize = atkFont;
			}

			var activeEffectsSize = Height / 1080;
			if(_activeEffectsScale != activeEffectsSize)
			{
				PlayerActiveEffects.RenderTransform = new ScaleTransform(activeEffectsSize, activeEffectsSize);
				OpponentActiveEffects.RenderTransform = new ScaleTransform(activeEffectsSize, activeEffectsSize);

				PlayerCounters.RenderTransform = new ScaleTransform(activeEffectsSize, activeEffectsSize);
				OpponentCounters.RenderTransform = new ScaleTransform(activeEffectsSize, activeEffectsSize);

				PlayerResourcesWidget.RenderTransform = new ScaleTransform(activeEffectsSize, activeEffectsSize);
				OpponentResourcesWidget.RenderTransform = new ScaleTransform(activeEffectsSize, activeEffectsSize);

				_activeEffectsScale = activeEffectsSize;
			}

			if(GuidesTabs.TabsContent is not null)
			{
				GuidesTabs.TabsContent.MaxHeight =  Math.Max(0, (ActualHeight - 54) * 0.95 / _bgsTopBarBehavior.GetScaling?.Invoke() ?? 1.0);
			}

			if(BattlegroundsMinions is not null)
			{
				BattlegroundsMinions.MaxHeight =  Math.Max(0, ActualHeight * 0.95 / _bgsTopBarBehavior.GetScaling?.Invoke() ?? 1.0);
			}
		}

		public void ApplyAutoScaling()
		{
			var scaling = Height / 1080;
			AutoScaling = Math.Max(0.8, Math.Min(1.3, scaling));

			_mulliganNotificationBehavior.UpdatePosition();
			_mulliganNotificationBehavior.UpdateScaling();

			_bgsTopBarBehavior.UpdatePosition();
			_bgsTopBarBehavior.UpdateScaling();

			_bgsTopBarTriggerMaskBehavior.UpdatePosition();
			_bgsTopBarTriggerMaskBehavior.UpdateScaling();

			_heroNotificationBehavior.UpdatePosition();
			_heroNotificationBehavior.UpdateScaling();

			_bgsBobsBuddyBehavior.UpdatePosition();
			_bgsBobsBuddyBehavior.UpdateScaling();

			_bgsPastOpponentBoardBehavior.UpdatePosition();
			_bgsPastOpponentBoardBehavior.UpdateScaling();

			_experienceCounterBehavior.UpdatePosition();
			_experienceCounterBehavior.UpdateScaling();

			_mercenariesTaskListButtonBehavior.UpdatePosition();
			_mercenariesTaskListButtonBehavior.UpdateScaling();

			_mercenariesTaskListBehavior.UpdatePosition();
			_mercenariesTaskListBehavior.UpdateScaling();

			_tier7PreLobbyBehavior.UpdatePosition();
			_tier7PreLobbyBehavior.UpdateScaling();

			_battlegroundsInspirationBehavior.UpdatePosition();
			_battlegroundsInspirationBehavior.UpdateScaling();

			_constructedMulliganGuidePreLobbyBehaviour.UpdatePosition();
			_constructedMulliganGuidePreLobbyBehaviour.UpdateScaling();

			_bgsChinaModuleBehavior.UpdatePosition();
			_bgsChinaModuleBehavior.UpdateScaling();

			_arenaOverlayBehavior.UpdatePosition();
			_arenaOverlayBehavior.UpdateScaling();

			_arenaPreLobbyBehavior.UpdatePosition();
			_arenaPreLobbyBehavior.UpdateScaling();

			BattlegroundsHeroPickingViewModel.Scaling = scaling;
			BattlegroundsHeroPicking.Width = Width / scaling;
			BattlegroundsHeroPicking.Height = Height / scaling;

			BattlegroundsQuestPickingViewModel.Scaling = scaling;
			BattlegroundsQuestPicking.Width = Width / scaling;
			BattlegroundsQuestPicking.Height = Height / scaling;

			BattlegroundsTrinketPickingViewModel.Scaling = scaling;
			BattlegroundsTrinketPicking.Width = Width / scaling;
			BattlegroundsTrinketPicking.Height = Height / scaling;

			ConstructedMulliganGuideViewModel.Scaling = scaling;
			ConstructedMulliganGuide.Width = Width / scaling;
			ConstructedMulliganGuide.Height = Height / scaling;
		}

		public void UpdateStackPanelAlignment()
		{
			OnPropertyChanged(nameof(PlayerStackPanelAlignment));
			OnPropertyChanged(nameof(OpponentStackPanelAlignment));
		}

		private void OverlayWindow_OnDeactivated(object sender, EventArgs e) => SetTopmost();

		public void UpdateVisibilities()
		{
			UpdateBattlegroundsSessionVisibility();
			UpdateTier7PreLobbyVisibility();
			UpdateMulliganGuidePreLobbyVisibility();
			UpdateArenaPickHelperVisibility();
			UpdateArenaPreLobbyVisibility();
		}

		public void UpdateBattlegroundsSessionVisibility()
		{
			var show = _game.IsRunning
			    && (Config.Instance.ShowSessionRecap || _uiMovable)
				&& (
					(
						// Scene is not transitioning
						SceneHandler.Scene != null &&
						SceneHandler.Scene switch
						{
							Mode.BACON => Config.Instance.ShowSessionRecapBetweenGames,
							Mode.GAMEPLAY => Core.Game.IsBattlegroundsMatch,
							_ => false
						}
					)
					|| (
						// Scene is transitioning - do not check for IsBattlegroundsMatch because that might not be set yet/still
						SceneHandler.Scene == null && Config.Instance.ShowSessionRecapBetweenGames &&
						(
							// Start of Match
							(SceneHandler.LastScene == Mode.BACON && SceneHandler.NextScene == Mode.GAMEPLAY)
							// End of Match
							|| (SceneHandler.LastScene == Mode.GAMEPLAY && SceneHandler.NextScene == Mode.BACON)
						)
					)
				);

			if(show || (_game.IsChinaModuleActive && Config.Instance.ShowSessionRecap))
			{
				FadeAnimation.SetVisibility(BattlegroundsSessionStackPanel, Visible);
				BattlegroundsSessionViewModelVM.Update();
				Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
			}
			else
			{
				FadeAnimation.SetVisibility(BattlegroundsSessionStackPanel, Collapsed);
			}
		}

		public void UpdateTier7PreLobbyVisibility()
		{
			var show = (
				_game.IsRunning &&
				_game.IsInMenu &&
				!_game.QueueEvents.IsInQueue &&
				SceneHandler.Scene == Mode.BACON &&
				Config.Instance.EnableBattlegroundsTier7Overlay &&
				Config.Instance.ShowBattlegroundsTier7PreLobby &&
				(
					Tier7PreLobbyViewModel.BattlegroundsGameMode == SelectedBattlegroundsGameMode.SOLO ||
					Tier7PreLobbyViewModel.BattlegroundsGameMode == SelectedBattlegroundsGameMode.DUOS
				)
			);

			if(show)
			{
				_tier7PreLobbyBehavior.Show();
				Tier7PreLobbyViewModel.Update().Forget();
			}
			else
			{
				_tier7PreLobbyBehavior.Hide();
			}
		}

		public void UpdateMulliganGuidePreLobbyVisibility()
		{
			var isPremium = HSReplayNetOAuth.AccountData?.IsPremium ?? false;;
			var show = (
				_game.IsRunning &&
				_game.IsInMenu &&
				SceneHandler.Scene == Mode.TOURNAMENT &&
				Config.Instance.EnableMulliganGuide &&
				Config.Instance.ShowMulliganGuidePreLobby &&
				isPremium
			);

			if(show)
			{
				_constructedMulliganGuidePreLobbyBehaviour.Show();
				ConstructedMulliganGuidePreLobbyViewModel.EnsureLoaded().Forget();
			}
			else
			{
				_constructedMulliganGuidePreLobbyBehaviour.Hide();
			}
		}

		public void UpdateArenaPreLobbyVisibility()
		{
			var show = (
				_game.IsRunning &&
				_game.IsInMenu &&
				!_game.QueueEvents.IsInQueue &&
				SceneHandler.Scene == Mode.DRAFT &&
				Config.Instance.EnableArenasmithOverlay &&
				Config.Instance.ShowArenasmithPreLobby
			);

			if(show)
			{
				_arenaPreLobbyBehavior.Show();
				ArenaPreDraftViewModel.Update().Forget();
			}
			else
			{
				_arenaPreLobbyBehavior.Hide();
			}
		}

		private void BgsMinionsFilterCover_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			BattlegroundsMinionsVM.IsFiltersOpen = false;
		}

		private void BgsMinionsFilterCover_OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
		{
			BattlegroundsMinionsVM.IsFilterRegionHovered = true;
		}

		private void BgsMinionsFilterCover_OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
		{
			BattlegroundsMinionsVM.IsFilterRegionHovered = false;
		}
	}
}
