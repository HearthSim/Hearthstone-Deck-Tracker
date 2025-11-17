using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.MinionPinning
{
	public class BattlegroundsMinionPinningCardViewModel : ViewModel
	{
		public bool IsMinionPinned
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public bool IsSlotOccupied
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public string? CardId
		{
			get => GetProp<string?>(null);
			set => SetProp(value);
		}

		public bool IsHovered
		{
			get => GetProp(false);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(ShouldShowRecommendedSection));
			}
		}

		public bool IsTribePinned
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public Race TribeIconRace
		{
			get => GetProp(Race.INVALID);
			set => SetProp(value);
		}

		public bool IsRecommendedPinned
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public List<BattlegroundsCompGuideViewModel>? RecommendedComps
		{
			get => GetProp<List<BattlegroundsCompGuideViewModel>?>(null);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(ShouldShowRecommendedSection));
			}
		}

		public bool ShouldShowRecommendedSection => RecommendedComps is { Count: > 0 } && IsHovered;

		public int RecommendedSectionCanvasLeft
		{
			get => GetProp(0);
			set => SetProp(value);
		}
	}
}
