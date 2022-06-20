using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public class BattlegroundsGameViewModel : ViewModel
	{
		public List<Entity> FinalBoardMinions { get; set; } = new List<Entity>();
		private readonly GameItem _gameItem;

		public BattlegroundsGameViewModel(GameItem gameItem)
		{
			_gameItem = gameItem;
			StartTime = gameItem.StartTime;
			Placement = gameItem.Placement;

			var heroCardId = gameItem.Hero ?? "";
			var heroCard = Database.GetCardFromId(heroCardId);
			if(heroCard?.BattlegroundsSkinParentId > 0)
				heroCard = Database.GetCardFromDbfId(heroCard.BattlegroundsSkinParentId, false);

			CardImage = new CardAssetViewModel(heroCard, Utility.Assets.CardAssetType.Tile);

			var heroShortNameMap = Remote.Config.Data?.BattlegroundsShortNames
				?.Find(sn => sn.DbfId == heroCard?.DbfId);
			HeroName = heroShortNameMap?.ShortName ?? heroCard?.Name ?? "-";

			PlacementText = LocUtil.GetPlacement(gameItem.Placement);

			MMRDelta = gameItem.RatingAfter - gameItem.Rating;
			var signal = MMRDelta > 0 ? "+" : "";
			MMRDeltaText = Math.Abs(MMRDelta) > 500 || gameItem.FriendlyGame ? "-" : $"{signal}{MMRDelta}";

			CrownVisibility = gameItem.Placement == 1 ? Visibility.Visible : Visibility.Hidden;

			if(gameItem.FinalBoard != null && gameItem.FinalBoard.FinalBoard != null)
				foreach(var fb in gameItem.FinalBoard.FinalBoard)
				{
					TagItem[] tags = fb.Tags?.ToArray() ?? Array.Empty<TagItem>();
					FinalBoardMinions.Add(new Entity()
					{
						CardId = fb.CardId,
						Tags = tags.ToDictionary(p => (GameTag)p.Tag, p => p.Value),
					});
				}

			FinalBoardVisibility = Visibility.Hidden;
			FinalBoardEmptyLabelVisibility = FinalBoardMinions.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
		}

		public void OnMouseEnter(double finalBoardContainerActualWidth)
		{
			var battlegroundsSessionPanelLeft = Canvas.GetLeft(Core.Overlay.BattlegroundsSessionStackPanel);
			var tooltipToRight = battlegroundsSessionPanelLeft < (Core.Overlay.Width / 2);

			FinalBoardCanvasLeft = tooltipToRight ? 227 : (int)(finalBoardContainerActualWidth * -0.6) - 10;
			FinalBoardCanvasTop = FinalBoardMinions.Count > 0 ? -70 : -30;
			FinalBoardArrowCanvasLeft = tooltipToRight ? 0 : (int)finalBoardContainerActualWidth + 2;
			FinalBoardArrowCanvasTop = FinalBoardMinions.Count > 0 ? 135 : 70; 
			FinalBoardArrowBorderThickness = tooltipToRight ? new Thickness(1, 0, 0, 1) : new Thickness(0, 1, 1, 0);
			FinalBoardVisibility = Visibility.Visible;
		}

		public void OnMouseLeave()
		{
			FinalBoardVisibility = Visibility.Hidden;
		}

		public SolidColorBrush PlacementTextBrush
		{
			get
			{
				return new SolidColorBrush(Placement <= 4 ? Color.FromRgb(109, 235, 108) : Color.FromRgb(236, 105, 105));
			}
		}

		public SolidColorBrush MMRDeltaTextBrush
		{
			get
			{
				if(MMRDelta == 0 || Math.Abs(MMRDelta) > 500 || _gameItem.FriendlyGame)
					return new SolidColorBrush(Colors.White);
				return new SolidColorBrush(MMRDelta > 0 ? Color.FromRgb(139, 210, 134) : Color.FromRgb(236, 105, 105));
			}
		}

		public string? StartTime { get; }
		public string HeroName { get; }
		public int Placement { get; }
		public string PlacementText { get; }
		public int MMRDelta { get; }
		public string MMRDeltaText { get; }
		public CardAssetViewModel CardImage { get; }
		public Visibility CrownVisibility { get; }

		private Visibility _finalBoardVisibility;
		public Visibility FinalBoardVisibility
		{
			get => _finalBoardVisibility;
			set
			{
				_finalBoardVisibility = value;
				OnPropertyChanged();
			}
		}

		private int _finalBoardCanvasLeft;
		public int FinalBoardCanvasLeft
		{
			get => _finalBoardCanvasLeft;
			set
			{
				_finalBoardCanvasLeft = value;
				OnPropertyChanged();
			}
		}

		private int _finalBoardCanvasTop;
		public int FinalBoardCanvasTop
		{
			get => _finalBoardCanvasTop;
			set
			{
				_finalBoardCanvasTop = value;
				OnPropertyChanged();
			}
		}

		private int _finalBoardArrowCanvasLeft;
		public int FinalBoardArrowCanvasLeft
		{
			get => _finalBoardArrowCanvasLeft;
			set
			{
				_finalBoardArrowCanvasLeft = value;
				OnPropertyChanged();
			}
		}

		private int _finalBoardArrowCanvasTop;
		public int FinalBoardArrowCanvasTop
		{
			get => _finalBoardArrowCanvasTop;
			set
			{
				_finalBoardArrowCanvasTop = value;
				OnPropertyChanged();
			}
		}

		private Thickness _finalBoardArrowBorderThickness;
		public Thickness FinalBoardArrowBorderThickness
		{
			get => _finalBoardArrowBorderThickness;
			set
			{
				_finalBoardArrowBorderThickness = value;
				OnPropertyChanged();
			}
		}

		private Visibility _finalBoardEmptyLabelVisibility;
		public Visibility FinalBoardEmptyLabelVisibility
		{
			get => _finalBoardEmptyLabelVisibility;
			set
			{
				_finalBoardEmptyLabelVisibility = value;
				OnPropertyChanged();
			}
		}
	}
}
