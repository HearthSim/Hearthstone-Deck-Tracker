using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan
{
	public class StatsHeaderViewModel : ViewModel
	{
		public int? Rank { get; }
		public double? MulliganWr { get; }
		public double? KeepRate { get; }
		private int? MaxRank { get; }
		private double? BaseWinrate { get; }

		public StatsHeaderViewModel(int? rank, double? mulliganWr, double? keepRate, int? maxRank, double? baseWinrate)
		{
			Rank = rank;
			MulliganWr = mulliganWr;
			KeepRate = keepRate;
			MaxRank = maxRank;
			BaseWinrate = baseWinrate;
		}

		public Brush RankGradient
		{
			get
			{
				if(!Rank.HasValue || !MaxRank.HasValue)
				{
					return new SolidColorBrush(Color.FromRgb(0x14, 0x16, 0x17));
				}

				if(Rank <= MaxRank * 0.25)
					return new LinearGradientBrush(Color.FromRgb(0x6A, 0x9D, 0x36), Color.FromRgb(0x58, 0x79, 0x37), 0);

				if(Rank <= MaxRank * 0.5)
					return new LinearGradientBrush(Color.FromRgb(0x92, 0xA0, 0x36), Color.FromRgb(0x68, 0x79, 0x37), 0);

				if(Rank <= MaxRank * 0.75)
					return new LinearGradientBrush(Color.FromRgb(0xA0, 0x7C, 0x36), Color.FromRgb(0x79, 0x5F, 0x37),0);

				return new LinearGradientBrush(Color.FromRgb(0xA0, 0x36, 0x36), Color.FromRgb(0x79, 0x37, 0x37), 0);
			}
		}

		public string MulliganWrColor
		{
			get
			{
				if(MulliganWr is double mulliganWr)
					return Helper.GetColorString(mulliganWr - (BaseWinrate ?? 50.0f), 75);

				return "White";
			}
		}

		public string HandRankTooltipText
		{
			get => string.Format(LocUtil.Get("ConstructedMulliganGuide_Header_HandRankTooltip_Desc"), Rank, MaxRank);
		}
	}
}
