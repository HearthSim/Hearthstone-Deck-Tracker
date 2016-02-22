using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		public void Update(bool refresh)
		{
			if (refresh)
			{
				ListViewPlayer.Items.Refresh();
				ListViewOpponent.Items.Refresh();
				Topmost = false;
				Topmost = true;
				Log.Info("Refreshed overlay topmost status");
			}

			var opponentHandCount = _game.Opponent.HandCount;
			for (var i = 0; i < 10; i++)
			{
				if (i < opponentHandCount)
				{
					_cardMarks[i].Text = !Config.Instance.HideOpponentCardAge ? _game.Opponent.Hand[i].Turn.ToString() : "";
					_cardMarks[i].Mark = !Config.Instance.HideOpponentCardMarks ? _game.Opponent.Hand[i].CardMark : CardMark.None;
					_cardMarks[i].Visibility = (_game.IsInMenu || (Config.Instance.HideOpponentCardAge && Config.Instance.HideOpponentCardMarks))
												   ? Hidden : Visible;
				}
				else
					_cardMarks[i].Visibility = Collapsed;
			}

			var oppBoard = Core.Game.Opponent.Board.Where(x => x.Entity.IsMinion).OrderBy(x => x.Entity.GetTag(GAME_TAG.ZONE_POSITION)).ToList();
			var playerBoard =
				Core.Game.Player.Board.Where(x => x.Entity.IsMinion).OrderBy(x => x.Entity.GetTag(GAME_TAG.ZONE_POSITION)).ToList();
			UpdateMouseOverDetectionRegions(oppBoard, playerBoard);
			if(!_game.IsInMenu && _game.IsMulliganDone && User32.IsHearthstoneInForeground())
				DetectMouseOver(playerBoard, oppBoard);
			else
				FlavorTextVisibility = Collapsed;

			StackPanelPlayer.Opacity = Config.Instance.PlayerOpacity / 100;
			StackPanelOpponent.Opacity = Config.Instance.OpponentOpacity / 100;
			StackPanelSecrets.Opacity = Config.Instance.SecretsOpacity / 100;
			Opacity = Config.Instance.OverlayOpacity / 100;

			if (!_playerCardsHidden)
			{
				StackPanelPlayer.Visibility = (Config.Instance.HideDecksInOverlay || (Config.Instance.HideInMenu && _game.IsInMenu)) && !_uiMovable
												  ? Collapsed : Visible;
			}

			if (!_opponentCardsHidden)
			{
				StackPanelOpponent.Visibility = (Config.Instance.HideDecksInOverlay || (Config.Instance.HideInMenu && _game.IsInMenu))
												&& !_uiMovable ? Collapsed : Visible;
			}

			CanvasPlayerChance.Visibility = Config.Instance.HideDrawChances ? Collapsed : Visible;
			LblPlayerFatigue.Visibility = Config.Instance.HidePlayerFatigueCount ? Collapsed : Visible;
			CanvasPlayerCount.Visibility = Config.Instance.HidePlayerCardCount ? Collapsed : Visible;

			CanvasOpponentChance.Visibility = Config.Instance.HideOpponentDrawChances ? Collapsed : Visible;
			LblOpponentFatigue.Visibility = Config.Instance.HideOpponentFatigueCount ? Collapsed : Visible;
			CanvasOpponentCount.Visibility = Config.Instance.HideOpponentCardCount ? Collapsed : Visible;

			if (_game.IsInMenu && !_uiMovable)
				HideTimers();

			ListViewOpponent.Visibility = Config.Instance.HideOpponentCards ? Collapsed : Visible;
			ListViewPlayer.Visibility = Config.Instance.HidePlayerCards ? Collapsed : Visible;

			SetCardCount(_game.Player.HandCount, _game.Player.DeckCount);

			SetOpponentCardCount(_game.Opponent.HandCount, _game.Opponent.DeckCount);


			LblWins.Visibility = Config.Instance.ShowDeckWins && _game.IsUsingPremade ? Visible : Collapsed;
			LblDeckTitle.Visibility = Config.Instance.ShowDeckTitle && _game.IsUsingPremade ? Visible : Collapsed;
			LblWinRateAgainst.Visibility = Config.Instance.ShowWinRateAgainst && _game.IsUsingPremade
											   ? Visible : Collapsed;

			var showWarning = !_game.IsInMenu && !_game.Player.DrawnCardsMatchDeck;
			StackPanelWarning.Visibility = showWarning ? Visible : Collapsed;
			if (showWarning)
			{
				var drawn = new Deck { Cards = new ObservableCollection<Card>(_game.Player.DrawnCards.Where(c => !c.IsCreated)) };
				var diff = (drawn - DeckList.Instance.ActiveDeckVersion).Where(c => c.Count > 0).ToList();
				if (diff.Count > 0)
				{
					var count = diff.Count > 3 ? 3 : diff.Count;
					LblWarningCards.Text = diff.Take(count).Select(c => c.LocalizedName).Aggregate((c, n) => c + ", " + n);
					if (diff.Count > 3)
						LblWarningCards.Text += ", ...";
				}
			}

			if (_game.IsInMenu)
			{
				if (Config.Instance.AlwaysShowGoldProgress)
				{
					UpdateGoldProgress();
					GoldProgressGrid.Visibility = Visible;
				}
			}
			else
				GoldProgressGrid.Visibility = Collapsed;

			UpdateAttackValues();

			SetDeckTitle();
			SetWinRates();

			UpdateElementSizes();
			UpdateElementPositions();


			if (Core.Windows.PlayerWindow.Visibility == Visible)
				Core.Windows.PlayerWindow.Update();
			if (Core.Windows.OpponentWindow.Visibility == Visible)
				Core.Windows.OpponentWindow.Update();
		}


		private void UpdateAttackValues()
		{
			IconBoardAttackPlayer.Visibility = Config.Instance.HidePlayerAttackIcon || _game.IsInMenu
												   ? Collapsed : Visible;
			IconBoardAttackOpponent.Visibility = Config.Instance.HideOpponentAttackIcon || _game.IsInMenu
													 ? Collapsed : Visible;

			// do the calculation if at least one of the icons is visible
			if (IconBoardAttackPlayer.Visibility == Visible || IconBoardAttackOpponent.Visibility == Visible)
			{
				var board = new BoardState();
				TextBlockPlayerAttack.Text = board.Player.Damage.ToString();
				TextBlockOpponentAttack.Text = board.Opponent.Damage.ToString();
			}
		}

		private void UpdateGoldProgress()
		{
			var region = (int)_game.CurrentRegion - 1;
			if (region < 0)
				return;
			var wins = Config.Instance.GoldProgress[region];
			if (wins >= 0)
				LblGoldProgress.Text = $"Wins: {wins}/3 ({Config.Instance.GoldProgressTotal[region]}/100G)";
		}


		public async void UpdatePosition()
		{
			//hide the overlay depenting on options
			ShowOverlay(
						!((Config.Instance.HideInBackground && !User32.IsHearthstoneInForeground())
						  || (Config.Instance.HideOverlayInSpectator && _game.CurrentGameMode == GameMode.Spectator) || Config.Instance.HideOverlay
						  || ForceHidden));


			var hsRect = User32.GetHearthstoneRect(true);

			//hs window has height 0 if it just launched, screwing things up if the tracker is started before hs is. 
			//this prevents that from happening. 
			if (hsRect.Height == 0 || Visibility != Visible)
				return;

			var prevWidth = Width;
			var prevHeight = Height;
			SetRect(hsRect.Top, hsRect.Left, hsRect.Width, hsRect.Height);
			if (Width != prevWidth)
				OnPropertyChanged(nameof(BoardWidth));
			if (Height != prevHeight)
			{
				OnPropertyChanged(nameof(BoardHeight));
				OnPropertyChanged(nameof(MinionMargin));
				OnPropertyChanged(nameof(MinionWidth));
				OnPropertyChanged(nameof(CardWidth));
				OnPropertyChanged(nameof(CardHeight));
			}

			UpdateElementSizes();
			UpdateElementPositions();

			try
			{
				await UpdateCardTooltip();
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		internal void UpdateTurnTimer(TimerEventArgs timerEventArgs)
		{
			if(!timerEventArgs.Running || (timerEventArgs.PlayerSeconds <= 0 && timerEventArgs.OpponentSeconds <= 0) 
				|| _game.CurrentMode != Mode.GAMEPLAY)
				return;

			ShowTimers();
			LblTurnTime.Text = $"{(timerEventArgs.Seconds / 60) % 60:00}:{timerEventArgs.Seconds % 60:00}";
			LblPlayerTurnTime.Text = $"{(timerEventArgs.PlayerSeconds / 60) % 60:00}:{timerEventArgs.PlayerSeconds % 60:00}";
			LblOpponentTurnTime.Text = $"{(timerEventArgs.OpponentSeconds / 60) % 60:00}:{timerEventArgs.OpponentSeconds % 60:00}";
		}

		public void UpdateScaling()
		{
			StackPanelPlayer.RenderTransform = new ScaleTransform(Config.Instance.OverlayPlayerScaling / 100,
																  Config.Instance.OverlayPlayerScaling / 100);
			StackPanelOpponent.RenderTransform = new ScaleTransform(Config.Instance.OverlayOpponentScaling / 100,
																	Config.Instance.OverlayOpponentScaling / 100);
			StackPanelSecrets.RenderTransform = new ScaleTransform(Config.Instance.SecretsPanelScaling, Config.Instance.SecretsPanelScaling);
		}

		private void UpdateElementPositions()
		{
			Canvas.SetTop(StackPanelPlayer, Height * Config.Instance.PlayerDeckTop / 100);
			Canvas.SetLeft(StackPanelPlayer, Width * Config.Instance.PlayerDeckLeft / 100 - StackPanelPlayer.ActualWidth * Config.Instance.OverlayPlayerScaling / 100);
			Canvas.SetTop(StackPanelOpponent, Height * Config.Instance.OpponentDeckTop / 100);
			Canvas.SetLeft(StackPanelOpponent, Width * Config.Instance.OpponentDeckLeft / 100);
			Canvas.SetTop(StackPanelSecrets, Height * Config.Instance.SecretsTop / 100);
			Canvas.SetLeft(StackPanelSecrets, Width * Config.Instance.SecretsLeft / 100);
			Canvas.SetTop(LblTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 - 5);
			Canvas.SetLeft(LblTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100);
			Canvas.SetTop(LblOpponentTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 - Config.Instance.TimersVerticalSpacing);
			Canvas.SetLeft(LblOpponentTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
			Canvas.SetTop(LblPlayerTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
			Canvas.SetLeft(LblPlayerTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
			Canvas.SetTop(IconBoardAttackPlayer, Height * Config.Instance.AttackIconPlayerVerticalPosition / 100);
			Canvas.SetLeft(IconBoardAttackPlayer, Helper.GetScaledXPos(Config.Instance.AttackIconPlayerHorizontalPosition / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(IconBoardAttackOpponent, Height * Config.Instance.AttackIconOpponentVerticalPosition / 100);
			Canvas.SetLeft(IconBoardAttackOpponent, Helper.GetScaledXPos(Config.Instance.AttackIconOpponentHorizontalPosition / 100, (int)Width, ScreenRatio));
			Canvas.SetTop(RectGoldDisplay, Height - RectGoldDisplay.ActualHeight);
			Canvas.SetLeft(RectGoldDisplay, Width - RectGoldDisplay.ActualWidth - GoldFrameOffset);
			Canvas.SetTop(GoldProgressGrid, Height - RectGoldDisplay.ActualHeight + (GoldFrameHeight - GoldProgressGrid.ActualHeight) / 2);
			Canvas.SetLeft(GoldProgressGrid, Width - RectGoldDisplay.ActualWidth - GoldFrameOffset - GoldProgressGrid.ActualWidth - 10);
			Canvas.SetTop(GridOpponentBoard, Height / 2 - GridOpponentBoard.ActualHeight - Height * 0.045);
			Canvas.SetTop(GridPlayerBoard, Height / 2 - Height * 0.03);
			if (Config.Instance.ShowFlavorText)
			{
				Canvas.SetTop(GridFlavorText, Height - GridFlavorText.ActualHeight - 10);
				Canvas.SetLeft(GridFlavorText, Width - GridFlavorText.ActualWidth - 10);
			}
			var handCount = _game.Opponent.HandCount > 10 ? 10 : _game.Opponent.HandCount;
			for (int i = 0; i < handCount; i++)
			{
				Canvas.SetLeft(_cardMarks[i], Helper.GetScaledXPos(_cardMarkPos[handCount - 1][i].X, (int)Width, ScreenRatio) - _cardMarks[i].ActualWidth / 2);
				Canvas.SetTop(_cardMarks[i], Math.Max(_cardMarkPos[handCount - 1][i].Y * Height - _cardMarks[i].ActualHeight / 3, 5));
			}
		}

		private void UpdateElementSizes()
		{
			var totalPlayerLabelsHeight = CanvasPlayerChance.ActualHeight + CanvasPlayerCount.ActualHeight + LblPlayerFatigue.ActualHeight
										  + LblDeckTitle.ActualHeight + LblWins.ActualHeight;
			if (((Height * Config.Instance.PlayerDeckHeight / (Config.Instance.OverlayPlayerScaling / 100) / 100)
				- (ListViewPlayer.Items.Count * 35 * Scaling + totalPlayerLabelsHeight)) < 1 || Scaling < 1)
			{
				var previousScaling = Scaling;
				Scaling = (Height * Config.Instance.PlayerDeckHeight / (Config.Instance.OverlayPlayerScaling / 100) / 100)
						  / (ListViewPlayer.Items.Count * 35 + totalPlayerLabelsHeight);
				if (Scaling > 1)
					Scaling = 1;

				if (previousScaling != Scaling)
					ListViewPlayer.Items.Refresh();
			}

			var totalOpponentLabelsHeight = CanvasOpponentChance.ActualHeight + CanvasOpponentCount.ActualHeight
											+ LblOpponentFatigue.ActualHeight + LblWinRateAgainst.ActualHeight;
			if (((Height * Config.Instance.OpponentDeckHeight / (Config.Instance.OverlayOpponentScaling / 100) / 100)
				- (ListViewOpponent.Items.Count * 35 * OpponentScaling + totalOpponentLabelsHeight)) < 1 || OpponentScaling < 1)
			{
				var previousScaling = OpponentScaling;
				OpponentScaling = (Height * Config.Instance.OpponentDeckHeight / (Config.Instance.OverlayOpponentScaling / 100) / 100)
								  / (ListViewOpponent.Items.Count * 35 + totalOpponentLabelsHeight);
				if (OpponentScaling > 1)
					OpponentScaling = 1;

				if (previousScaling != OpponentScaling)
					ListViewOpponent.Items.Refresh();
			}

			//Gold progress
			RectGoldDisplay.Height = GoldFrameHeight;
			RectGoldDisplay.Width = GoldFrameWidth;
			GoldProgressGrid.Height = GoldFrameHeight;
			GPLeftCol.Width = new GridLength(GoldFrameHeight);
			GPRightCol.Width = new GridLength(GoldFrameHeight);
			LblGoldProgress.Margin = new Thickness(GoldFrameHeight * 1.2, 0, GoldFrameHeight * 0.8, 0);
			LblGoldProgress.FontSize = Height * 0.017;
			
			//Scale attack icons, with height
			var atkWidth = (int)Math.Round(Height * 0.0695, 0);
			var atkFont = (int)Math.Round(Height * 0.0223, 0);
			IconBoardAttackPlayer.Width = atkWidth;
			IconBoardAttackPlayer.Height = atkWidth;
			TextBlockPlayerAttack.FontSize = atkFont;
			IconBoardAttackOpponent.Width = atkWidth;
			IconBoardAttackOpponent.Height = atkWidth;
			TextBlockOpponentAttack.FontSize = atkFont;

		}

		public double GoldFrameHeight => Height * 25 / 768;
		public double GoldFrameWidth => 6 * GoldFrameHeight;
		public double GoldFrameOffset => 85 / 25 * GoldFrameHeight;

	}
}
