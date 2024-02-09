using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan
{
	public class ConstructedMulliganGuideViewModel : ViewModel
	{
		public Visibility Visibility
		{
			get => GetProp(Collapsed);
			set
			{
				SetProp(value);
				UpdateMetrics();
			}
		}

		public Visibility StatsVisibility
		{
			get => GetProp(Collapsed);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(VisibilityToggleIcon));
				OnPropertyChanged(nameof(VisibilityToggleText));
				UpdateMetrics();
			}
		}

		public Visual? VisibilityToggleIcon =>
			Application.Current.TryFindResource(StatsVisibility == Visible ? "eye_slash" : "eye") as Visual;
		public string VisibilityToggleText => StatsVisibility == Visible
			? LocUtil.Get("ConstructedMulliganGuide_VisibilityToggle_Hide")
			: LocUtil.Get("ConstructedMulliganGuide_VisibilityToggle_Show");

		public List<ConstructedMulliganSingleCardViewModel>? CardStats
		{
			get => GetProp<List<ConstructedMulliganSingleCardViewModel>?>(null);
			set => SetProp(value);
		}

		public OverlayMessageViewModel Message { get; } = new();

		public void Reset()
		{
			CardStats = null;
			Visibility = Collapsed;
			StatsVisibility = Collapsed;
			Message.Clear();
		}

		public double Scaling { get => GetProp(1.0); set => SetProp(value); }

		public void SetMulliganData(IEnumerable<SingleCardStats>? stats, int? maxRank)
		{
			CardStats = stats?.Select(x => new ConstructedMulliganSingleCardViewModel(x, maxRank)).ToList();

			// TODO values
			//Message.Mmr(scoreData.SelectedParams. stats[0].MmrFilterValue, stats[0].MinMmr, anomalyAdjusted);

			Visibility = Visible;
			StatsVisibility = Config.Instance.ShowMulliganGuideAutomatically ? Visible : Collapsed;

			UpdateMetrics();
		}

		private void UpdateMetrics()
		{
			if(Visibility == Visible && StatsVisibility == Visible)
				Core.Game.Metrics.ConstructedMulliganGuideOverlayDisplayed = true;
		}
	}
}
