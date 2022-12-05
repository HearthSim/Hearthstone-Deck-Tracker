using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public class StatsHeaderViewModel : ViewModel
	{
		public int? Tier { get; }
		public double? AvgPlacement { get; }
		public double? PickRate { get; }
		
		public StatsHeaderViewModel(int? tier, double? avgPlacement, double? pickRate)
		{
			Tier = tier;
			AvgPlacement = avgPlacement;
			PickRate = pickRate;
		}

		public Brush TierGradient => Tier switch
		{
			1 => new LinearGradientBrush(Color.FromRgb(0x6A, 0x9D, 0x36), Color.FromRgb(0x58, 0x79, 0x37), 0),
			2 => new LinearGradientBrush(Color.FromRgb(0x92, 0xA0, 0x36), Color.FromRgb(0x68, 0x79, 0x37), 0),
			3 => new LinearGradientBrush(Color.FromRgb(0xA0, 0x7C, 0x36), Color.FromRgb(0x79, 0x5F, 0x37), 0),
			4 => new LinearGradientBrush(Color.FromRgb(0xA0, 0x36, 0x36), Color.FromRgb(0x79, 0x37, 0x37), 0),
			_ => new SolidColorBrush(Color.FromRgb(0x14, 0x16, 0x17)),
		};

		public string AvgPlacementColor => Tier switch
		{
			1 => "#6BA036",
			2 => "#92A036",
			3 => "#A07C36",
			4 => "#B44646",
			_ => "#FFFFFF"
		};
	}
}
