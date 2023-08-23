using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7
{
	public class OverlayMessageViewModel : ViewModel
	{
		public string? Text
		{
			get => GetProp<string?>(null);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Visibility));
			}
		}

		public Visibility Visibility => Text == null ? Collapsed : Visible;

		public async void Error()
		{
			var errorText = LocUtil.Get("BattlegroundsOverlayMessage_Error");
			Text = errorText;

			await Task.Delay(5000);
			// Only clear if no other text was set in the meantime
			if(Text == errorText)
			{
				Clear();
			}
		}

		public void Loading() => Text = LocUtil.Get("BattlegroundsOverlayMessage_Loading");

		public void Disabled() => Text = LocUtil.Get("BattlegroundsOverlayMessage_Disabled");

		private static readonly Dictionary<string, int> MmrPercentValues = new()
		{
			{ "TOP_1_PERCENT", 1 },
			{ "TOP_5_PERCENT", 5 },
			{ "TOP_10_PERCENT", 10 },
			{ "TOP_20_PERCENT", 20 },
			{ "TOP_50_PERCENT", 50 },
		};

		public void Mmr(string filterValue, int? minMMR, bool anomalyAdjusted)
		{
			if(MmrPercentValues.TryGetValue(filterValue, out var percent))
			{
				var mmr = Helper.ToPrettyNumber(minMMR ?? 0);
				if(anomalyAdjusted)
				{
					Text = string.Format(LocUtil.Get("BattlegroundsOverlayMessage_MMR_AnomalyAdjusted"), percent, mmr);
				}
				else
				{
					Text = string.Format(LocUtil.Get("BattlegroundsOverlayMessage_MMR"), percent, mmr);
				}
			}
			else
				Clear();
		}

		public void Clear()
		{
			Text = null;
		}
	}
}
