#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		#region CardTooltips

		private void UpdateCardTooltip()
		{
			var pos = User32.GetMousePos();
			var relativePlayerDeckPos = ViewBoxPlayer.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeOpponentDeckPos = ViewBoxOpponent.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeSecretsPos = StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeCardMark = _cardMarks.Select(x => new { Label = x, Pos = x.PointFromScreen(new Point(pos.X, pos.Y)) });
			var visibility = (Config.Instance.OverlayCardToolTips && !Config.Instance.OverlaySecretToolTipsOnly)
								 ? Visible : Hidden;

			var cardMark =
				relativeCardMark.FirstOrDefault(
												x =>
												x.Label.IsVisible && PointInsideControl(x.Pos, x.Label.ActualWidth, x.Label.ActualHeight, new Thickness(3, 1, 7, 1)));

			ToolTipCardBlock.CreatedByVisibility = Collapsed;
			if(!Config.Instance.HideOpponentCardMarks && cardMark != null)
			{
				var index = _cardMarks.IndexOf(cardMark.Label);
				var drawnEntity = _game.Opponent.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == index + 1 && x.Info.GetDrawerId() != null);
				var entity = _game.Opponent.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == index + 1 && x.HasCardId && !x.Info.Hidden);
				var card = entity?.Card;
				var creatorCard = _cardMarks[index].SourceCard;
				if(card != null || creatorCard != null)
				{
					if(creatorCard != null || drawnEntity != null)
					{
						var creatorDescription = "Created By ";
						if(drawnEntity?.Info.GetDrawerId() != null && drawnEntity?.Info.GetDrawerId() > 0)
							creatorDescription = "Drawn By ";
						ToolTipCardBlock.CreatedByText =  $"{creatorDescription}{creatorCard.Name}";
						ToolTipCardBlock.CreatedByVisibility = Visible;
					}
					ToolTipCardBlock.SetCardIdFromCard(card ?? creatorCard);
					var offset = _cardMarks[index].ActualHeight * 1.1;
					var topOffset = Canvas.GetTop(_cardMarks[index]) + offset;
					var leftOffset = Canvas.GetLeft(_cardMarks[index]) + offset;
					Canvas.SetTop(ToolTipCardBlock, topOffset);
					Canvas.SetLeft(ToolTipCardBlock, leftOffset);
					ToolTipCardBlock.Visibility = Config.Instance.OverlayCardMarkToolTips ? Visible : Hidden;
				}
				else
				{
					ToolTipCardBlock.Visibility = Hidden;
				}
			}
			//player card tooltips
			else if(ListViewPlayer.Visibility == Visible && StackPanelPlayer.Visibility == Visible
					&& PointInsideControl(relativePlayerDeckPos, ListViewPlayer.ActualWidth, ListViewPlayer.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = ViewBoxPlayer.ActualHeight / ListViewPlayer.Items.Count;
				var cardIndex = (int)(relativePlayerDeckPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= ListViewPlayer.Items.Count)
					return;

				var card = ListViewPlayer.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				var centeredListOffset = Config.Instance.OverlayCenterPlayerStackPanel ? (BorderStackPanelPlayer.ActualHeight - StackPanelPlayer.ActualHeight) / 2 : 0;
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelPlayer) + centeredListOffset
								+ GetListViewOffset(StackPanelPlayer) + cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100 - ToolTipCardBlock.ActualHeight/2;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);

				SetTooltipPosition(topOffset, BorderStackPanelPlayer);

				ToolTipCardBlock.Visibility = visibility;
			}
			//opponent card tooltips
			else if(ListViewOpponent.Visibility == Visible && StackPanelOpponent.Visibility == Visible
					&& PointInsideControl(relativeOpponentDeckPos, ListViewOpponent.ActualWidth, ListViewOpponent.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = ViewBoxOpponent.ActualHeight / ListViewOpponent.Items.Count;
				var cardIndex = (int)(relativeOpponentDeckPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= ListViewOpponent.Items.Count)
					return;

				var centeredListOffset = Config.Instance.OverlayCenterOpponentStackPanel ? (BorderStackPanelOpponent.ActualHeight - StackPanelOpponent.ActualHeight) / 2 : 0;
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelOpponent) + centeredListOffset
								+ GetListViewOffset(StackPanelOpponent) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100 - ToolTipCardBlock.ActualHeight / 2;
				var card = ListViewOpponent.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);
				SetTooltipPosition(topOffset, BorderStackPanelOpponent);

				ToolTipCardBlock.Visibility = visibility;
			}
			else if(StackPanelSecrets.Visibility == Visible
					&& PointInsideControl(relativeSecretsPos, StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = StackPanelSecrets.ActualHeight / StackPanelSecrets.Children.Count;
				var cardIndex = (int)(relativeSecretsPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= StackPanelSecrets.Children.Count)
					return;

				//offset is affected by scaling
				var topOffset = Canvas.GetTop(StackPanelSecrets) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100 - ToolTipCardBlock.ActualHeight / 2;
				var card = StackPanelSecrets.Children.Cast<Controls.Card>().ElementAt(cardIndex);
				ToolTipCardBlock.SetCardIdFromCard(new Hearthstone.Card() { Id = card.CardId, BaconCard = false });
				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);
				SetTooltipPosition(topOffset, StackPanelSecrets);

				ToolTipCardBlock.Visibility = Config.Instance.OverlaySecretToolTipsOnly ? Visible : visibility;
			}
			else if(BgsTopBar.Visibility == Visible && BattlegroundsMinionsPanel.Visibility == Visible && BattlegroundsMinionsPanel.ActiveTier > 0)
			{
				var found = false;
				foreach(var group in BattlegroundsMinionsPanel.Groups)
				{
					var cardList = group.Cards;
					if(!group.IsVisible || !cardList.IsVisible)
						continue;
					var relativePos = cardList.PointFromScreen(new Point(pos.X, pos.Y));
					if(PointInsideControl(relativePos, cardList.ActualWidth, cardList.ActualHeight))
					{
						var cards = cardList.ItemsControl.Items;
						var cardSize = cardList.ActualHeight / cards.Count;
						var cardIndex = (int)(relativePos.Y / cardSize);
						if(cardIndex < 0 || cardIndex >= cards.Count)
							return;
						var card = cards.GetItemAt(cardIndex) as AnimatedCard;
						if(card == null)
							return;
						ToolTipCardBlock.SetCardIdFromCard(card.Card);
						//offset is affected by scaling
						var cardListPos = cardList.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
						var topOffset = cardListPos.Y + cardIndex * cardSize * AutoScaling - ToolTipCardBlock.ActualHeight / 2;
						topOffset = Math.Max(0, topOffset);
						//prevent tooltip from going outside of the overlay
						if(topOffset + ToolTipCardBlock.ActualHeight > Height)
							topOffset = Height - ToolTipCardBlock.ActualHeight;

						Canvas.SetTop(ToolTipCardBlock, topOffset);
						Canvas.SetLeft(ToolTipCardBlock, cardListPos.X - ToolTipCardBlock.ActualWidth + 22);

						ToolTipCardBlock.Visibility = visibility;
						found = true;
					}
				}

				if(!found)
				{
					ToolTipCardBlock.Visibility = Hidden;
					HideAdditionalToolTips();
					ToolTipCardBlock.SetCardIdFromCard(null);
				}
			}
			else
			{
				ToolTipCardBlock.SetCardIdFromCard(null);
				ToolTipCardBlock.Visibility = Hidden;
				HideAdditionalToolTips();
			}

			if(!Config.Instance.ForceMouseHook)
			{
				if(Config.Instance.ExtraFeatures)
				{
					var relativePos = PointFromScreen(new Point(pos.X, pos.Y));
					if((StackPanelSecrets.IsVisible
						&& (PointInsideControl(StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y)), StackPanelSecrets.ActualWidth,
											   StackPanelSecrets.ActualHeight, new Thickness(20))) || relativePos.X < 170 && relativePos.Y > Height - 120))
					{
						if(_mouseInput == null)
							HookMouse();
					}
					else if(_mouseInput != null && !((_isFriendsListOpen.HasValue && _isFriendsListOpen.Value) || Reflection.IsFriendsListVisible()))
						UnHookMouse();
				}
				else if(_mouseInput != null)
					UnHookMouse();
			}
		}

		private double GetListViewOffset(Panel stackPanel)
		{
			var offset = 0.0;
			foreach(var child in stackPanel.Children)
			{
				if(child is HearthstoneTextBlock text)
					offset += text.ActualHeight;
				else
				{
					if(child is ListView)
						break;
					if(child is StackPanel sp)
						offset += sp.ActualHeight;
				}
			}
			return offset;
		}

		private void HideAdditionalToolTips() => StackPanelAdditionalTooltips.Visibility = Hidden;

		private void SetTooltipPosition(double yOffset, FrameworkElement stackpanel)
		{
			Canvas.SetTop(ToolTipCardBlock, yOffset);

			if(Canvas.GetLeft(stackpanel) < Width / 2)
				Canvas.SetLeft(ToolTipCardBlock, Canvas.GetLeft(stackpanel) + stackpanel.ActualWidth * Config.Instance.OverlayOpponentScaling / 100);
			else
				Canvas.SetLeft(ToolTipCardBlock, Canvas.GetLeft(stackpanel) - ToolTipCardBlock.ActualWidth);
		}

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight)
			=> PointInsideControl(pos, actualWidth, actualHeight, new Thickness(0));

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight, Thickness margin)
			=> pos.X > 0 - margin.Left && pos.X < actualWidth + margin.Right && (pos.Y > 0 - margin.Top && pos.Y < actualHeight + margin.Bottom);

		#endregion

		#region FlavorText


		private Visibility _flavorTextVisibility = Collapsed;
		private string _flavorTextCardName;
		private string _flavorText;

		public string FlavorText
		{
			get
			{
				return string.IsNullOrEmpty(_flavorText) ? "-" : _flavorText;
			}
			set
			{
				if(value != _flavorText)
				{
					_flavorText = value;
					OnPropertyChanged();
				}
			}
		}

		public string FlavorTextCardName
		{
			get { return _flavorTextCardName; }
			set
			{
				if(value != _flavorTextCardName)
				{
					_flavorTextCardName = value;
					OnPropertyChanged();
				}
			}
		}

		public Visibility FlavorTextVisibility
		{
			get { return _flavorTextVisibility; }
			set
			{
				if(value != _flavorTextVisibility)
				{
					_flavorTextVisibility = value;
					OnPropertyChanged();
				}
			}
		}

		private void SetFlavorTextEntity(Entity entity)
		{
			try
			{
				if(!Config.Instance.ShowFlavorText || entity == null)
					return;
				var card = entity.Info.LatestCardId == entity.CardId
					? entity.Card
					: Database.GetCardFromId(entity.Info.LatestCardId);
				if(string.IsNullOrEmpty(card?.FormattedFlavorText))
					return;
				FlavorText = card.FormattedFlavorText;
				FlavorTextCardName = card.LocalizedName;
				FlavorTextVisibility = Visible;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		#endregion
	}
}
