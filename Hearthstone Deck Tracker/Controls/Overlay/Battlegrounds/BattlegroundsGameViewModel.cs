using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public class BattlegroundsGameViewModel : ViewModel, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public List<Entity> FinalBoardMinions { get; set; } = new List<Entity>();

		public BattlegroundsGameViewModel(GameItem gameItem)
		{
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
			MMRDeltaText = $"{signal}{MMRDelta}";

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

		public void OnMouseEnter(double finalBoardContainerActualWidth, double scale)
		{
			var battlegroundsSessionPanelLeft = Canvas.GetLeft(Core.Overlay.BattlegroundsSessionStackPanel);
			var tooltipToRight = battlegroundsSessionPanelLeft < (Core.Overlay.Width / 2);

			FinalBoardCanvasLeft = tooltipToRight ? 227 : (int)(finalBoardContainerActualWidth * -scale) - 10;
			FinalBoardCanvasTop = FinalBoardMinions.Count > 0 ? -70 : -30;
			FinalBoardArrowCanvasLeft = tooltipToRight ? 0 : (int)finalBoardContainerActualWidth + 2;
			FinalBoardArrowCanvasTop = FinalBoardMinions.Count > 0 ? 135 : 70; 
			FinalBoardArrowBorderThickness = tooltipToRight ? new Thickness(1, 0, 0, 1) : new Thickness(0, 1, 1, 0);
			FinalBoardVisibility = Visibility.Visible;

			OnPropertyChanged(nameof(FinalBoardCanvasLeft));
			OnPropertyChanged(nameof(FinalBoardCanvasTop));
			OnPropertyChanged(nameof(FinalBoardArrowCanvasLeft));
			OnPropertyChanged(nameof(FinalBoardArrowCanvasTop));
			OnPropertyChanged(nameof(FinalBoardArrowBorderThickness));
			OnPropertyChanged(nameof(FinalBoardVisibility));
		}

		public void OnMouseLeave()
		{
			FinalBoardVisibility = Visibility.Hidden;
			OnPropertyChanged(nameof(FinalBoardVisibility));
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
				if(MMRDelta == 0)
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
		public Visibility FinalBoardVisibility { get; set; }
		public int FinalBoardCanvasLeft { get; set; }
		public int FinalBoardCanvasTop { get; set; }
		public int FinalBoardArrowCanvasLeft { get; set; }
		public int FinalBoardArrowCanvasTop { get; set; }
		public Thickness FinalBoardArrowBorderThickness { get; set; }
		public Visibility FinalBoardEmptyLabelVisibility { get; set; }
	}
}
