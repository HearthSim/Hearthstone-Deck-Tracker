using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public class StatsHeaderViewModel : ViewModel
	{
		public int? Tier { get; }
		public string? TierV2 { get; }
		public double? AvgPlacement { get; }
		public double? PickRate { get; }

		public StatsHeaderViewModel(int? tier, double? avgPlacement, double? pickRate)
		{
			Tier = tier;
			AvgPlacement = avgPlacement;
			PickRate = pickRate;
		}

		public StatsHeaderViewModel(string? tier, double? avgPlacement, double? pickRate)
		{
			TierV2 = tier;
			AvgPlacement = avgPlacement;
			PickRate = pickRate;
		}

		public string TierChar => TierV2 != null ? TierV2.ToUpper() : Tier.ToString();

		public Brush TierGradient => TierV2 != null ? TierV2 switch
		{
			"s" => new LinearGradientBrush(Color.FromRgb(0x40, 0x8a, 0xbf), Color.FromRgb(0x38, 0x5F, 0x7a), 0),
			"a" => new LinearGradientBrush(Color.FromRgb(0x6A, 0x9D, 0x36), Color.FromRgb(0x58, 0x79, 0x37), 0),
			"b" => new LinearGradientBrush(Color.FromRgb(0x92, 0xA0, 0x36), Color.FromRgb(0x68, 0x79, 0x37), 0),
			"c" => new LinearGradientBrush(Color.FromRgb(0xA0, 0x7C, 0x36), Color.FromRgb(0x79, 0x5F, 0x37), 0),
			"d" => new LinearGradientBrush(Color.FromRgb(0xA0, 0x48, 0x36), Color.FromRgb(0x79, 0x42, 0x37), 0),
			"f" => new LinearGradientBrush(Color.FromRgb(0xA0, 0x36, 0x36), Color.FromRgb(0x79, 0x37, 0x37), 0),
			_ => new SolidColorBrush(Color.FromRgb(0x14, 0x16, 0x17)),
		} : Tier switch
		{
			1 => new LinearGradientBrush(Color.FromRgb(0x6A, 0x9D, 0x36), Color.FromRgb(0x58, 0x79, 0x37), 0),
			2 => new LinearGradientBrush(Color.FromRgb(0x92, 0xA0, 0x36), Color.FromRgb(0x68, 0x79, 0x37), 0),
			3 => new LinearGradientBrush(Color.FromRgb(0xA0, 0x7C, 0x36), Color.FromRgb(0x79, 0x5F, 0x37), 0),
			4 => new LinearGradientBrush(Color.FromRgb(0xA0, 0x36, 0x36), Color.FromRgb(0x79, 0x37, 0x37), 0),
			_ => new SolidColorBrush(Color.FromRgb(0x14, 0x16, 0x17)),
		};

		public string AvgPlacementColor
		{
			get
			{
				if(AvgPlacement is double avgPlacement)
					return Helper.GetColorString(Helper.ColorStringMode.BATTLEGROUNDS, (4.5f - avgPlacement) * 100f / 3.5f, 75);

				return "#FFFFFF";
			}
		}
	}
}
