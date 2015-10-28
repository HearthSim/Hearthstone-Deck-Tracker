using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena.Charts
{
	/// <summary>
	/// Interaction logic for ChartWinsByClass.xaml
	/// </summary>
	public partial class ChartWinsLossVsClass : INotifyPropertyChanged
	{
		public ChartWinsLossVsClass()
		{
			InitializeComponent();
			ArenaStats.Instance.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == "WinLossVsClass")
					OnPropertyChanged("SeriesSourceWins");
			};
		}

		public IEnumerable<WinChartData> SeriesSourceWins
		{
			get
			{
				return
					Enumerable.Range(0, 9)
							  .Select(
									  n =>
									  new WinChartData
				{
					Index = Enum.GetNames(typeof(HeroClass))[n],
					ItemsSource = ArenaStats.Instance.WinLossVsClass[n]
				});
			}
		}

		public class WinChartData
		{
			public string Index { get; set; }
			public IEnumerable<ChartStats> ItemsSource { get; set; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
