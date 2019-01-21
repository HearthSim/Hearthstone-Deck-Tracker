#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		#region CardTooltips

		public void ShowAdditionalToolTips()
		{
			if(!Config.Instance.AdditionalOverlayTooltips)
				return;
			var card = ToolTipCard.DataContext as Card;
			if(card == null)
				return;
			if(card.EntourageCardIds.Length == 0)
			{
				HideAdditionalToolTips();
				return;
			}

			StackPanelAdditionalTooltips.Children.Clear();
			foreach(var id in card.EntourageCardIds)
			{
				var tooltip = new CardToolTipControl();
				tooltip.SetValue(DataContextProperty, Database.GetCardFromId(id));
				tooltip.CardSetToolTip.Visibility = Config.Instance.OverlaySetToolTips ? Visible : Collapsed;
				StackPanelAdditionalTooltips.Children.Add(tooltip);
			}

			StackPanelAdditionalTooltips.UpdateLayout();

			//set position
			var tooltipLeft = Canvas.GetLeft(ToolTipCard);
			var left = tooltipLeft < Width / 2 ? tooltipLeft + ToolTipCard.ActualWidth : tooltipLeft - StackPanelAdditionalTooltips.ActualWidth;

			Canvas.SetLeft(StackPanelAdditionalTooltips, left);
			var top = Canvas.GetTop(ToolTipCard) - (StackPanelAdditionalTooltips.ActualHeight / 2 - ToolTipCard.ActualHeight / 2);
			if(top < 0)
				top = 0;
			else if(top + StackPanelAdditionalTooltips.ActualHeight > Height)
				top = Height - StackPanelAdditionalTooltips.ActualHeight;
			Canvas.SetTop(StackPanelAdditionalTooltips, top);

			StackPanelAdditionalTooltips.Visibility = Visible;
		}

		private void UpdateCardTooltip()
		{
			var pos = User32.GetMousePos();
			var relativePlayerDeckPos = ViewBoxPlayer.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeOpponentDeckPos = ViewBoxOpponent.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeSecretsPos = StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeCardMark = _cardMarks.Select(x => new {Label = x, Pos = x.PointFromScreen(new Point(pos.X, pos.Y))});
			var visibility = (Config.Instance.OverlayCardToolTips && !Config.Instance.OverlaySecretToolTipsOnly)
								 ? Visible : Hidden;
			ToolTipCard.CardSetToolTip.Visibility = Config.Instance.OverlaySetToolTips ? Visible : Collapsed;

			var cardMark =
				relativeCardMark.FirstOrDefault(
											    x =>
												x.Label.IsVisible && PointInsideControl(x.Pos, x.Label.ActualWidth, x.Label.ActualHeight, new Thickness(3, 1, 7, 1)));
			if(!Config.Instance.HideOpponentCardMarks && cardMark != null)
			{
				var index = _cardMarks.IndexOf(cardMark.Label);
				var card = _game.Opponent.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == index + 1 && x.HasCardId && !x.Info.Hidden)?.Card;
				if(card != null)
				{
					ToolTipCard.SetValue(DataContextProperty, card);
					var topOffset = Canvas.GetTop(_cardMarks[index]) + _cardMarks[index].ActualHeight;
					var leftOffset = Canvas.GetLeft(_cardMarks[index]) + _cardMarks[index].ActualWidth * index;
					Canvas.SetTop(ToolTipCard, topOffset);
					Canvas.SetLeft(ToolTipCard, leftOffset);
					ToolTipCard.Visibility = Config.Instance.OverlayCardMarkToolTips ? Visible : Hidden;
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

				ToolTipCard.SetValue(DataContextProperty, ListViewPlayer.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card);

				var centeredListOffset = Config.Instance.OverlayCenterPlayerStackPanel ? (BorderStackPanelPlayer.ActualHeight - StackPanelPlayer.ActualHeight) / 2 : 0;
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelPlayer) + centeredListOffset
								+ GetListViewOffset(StackPanelPlayer) + cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCard.ActualHeight > Height)
					topOffset = Height - ToolTipCard.ActualHeight;

				SetTooltipPosition(topOffset, BorderStackPanelPlayer);

				ToolTipCard.Visibility = visibility;
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

				ToolTipCard.SetValue(DataContextProperty, ListViewOpponent.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card);

				var centeredListOffset = Config.Instance.OverlayCenterOpponentStackPanel ? (BorderStackPanelOpponent.ActualHeight - StackPanelOpponent.ActualHeight) / 2 : 0;
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelOpponent) + centeredListOffset
								+ GetListViewOffset(StackPanelOpponent) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCard.ActualHeight > Height)
					topOffset = Height - ToolTipCard.ActualHeight;

				SetTooltipPosition(topOffset, BorderStackPanelOpponent);

				ToolTipCard.Visibility = visibility;
			}
			else if(StackPanelSecrets.Visibility == Visible
					&& PointInsideControl(relativeSecretsPos, StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = StackPanelSecrets.ActualHeight / StackPanelSecrets.Children.Count;
				var cardIndex = (int)(relativeSecretsPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= StackPanelSecrets.Children.Count)
					return;

				ToolTipCard.SetValue(DataContextProperty, StackPanelSecrets.Children[cardIndex].GetValue(DataContextProperty));

				//offset is affected by scaling
				var topOffset = Canvas.GetTop(StackPanelSecrets) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCard.ActualHeight > Height)
					topOffset = Height - ToolTipCard.ActualHeight;

				SetTooltipPosition(topOffset, StackPanelSecrets);

				ToolTipCard.Visibility = Config.Instance.OverlaySecretToolTipsOnly ? Visible : visibility;
			}
			else
			{
				ToolTipCard.Visibility = Hidden;
				HideAdditionalToolTips();
			}

			if(ToolTipCard.Visibility == Visible)
			{
				if(ToolTipCard.GetValue(DataContextProperty) is Card card)
				{
					if(_lastToolTipCardId != card.Id)
					{
						_lastToolTipCardId = card.Id;
						ShowAdditionalToolTips();
					}
				}
				else
					HideAdditionalToolTips();
			}
			else
			{
				HideAdditionalToolTips();
				_lastToolTipCardId = string.Empty;
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

			if(!Config.Instance.AlwaysShowGoldProgress)
			{
				if(_game.IsInMenu
				   && PointInsideControl(RectGoldDisplay.PointFromScreen(new Point(pos.X, pos.Y)), RectGoldDisplay.ActualWidth,
										 RectGoldDisplay.ActualHeight))
				{
					UpdateGoldProgress();
					GoldProgressGrid.Visibility = Visible;
				}
				else
					GoldProgressGrid.Visibility = Hidden;
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
			Canvas.SetTop(ToolTipCard, yOffset);

			if(Canvas.GetLeft(stackpanel) < Width / 2)
				Canvas.SetLeft(ToolTipCard, Canvas.GetLeft(stackpanel) + stackpanel.ActualWidth * Config.Instance.OverlayOpponentScaling / 100);
			else
				Canvas.SetLeft(ToolTipCard, Canvas.GetLeft(stackpanel) - ToolTipCard.ActualWidth);
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
				if(!Config.Instance.ShowFlavorText || string.IsNullOrEmpty(entity?.Card?.FormattedFlavorText))
					return;
				FlavorText = entity.Card.FormattedFlavorText;
				FlavorTextCardName = entity.Card.LocalizedName;
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
