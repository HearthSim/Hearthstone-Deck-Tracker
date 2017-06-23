#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena.Charts
{
	/// <summary>
	/// Interaction logic for ChartWinsByClass.xaml
	/// </summary>
	public partial class ChartWinsByClass : INotifyPropertyChanged
	{
		public ChartWinsByClass()
		{
			InitializeComponent();
			ArenaStats.Instance.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == "WinsByClass")
					OnPropertyChanged(nameof(SeriesSourceWins));
			};
		}

		public IEnumerable<WinChartData> SeriesSourceWins
		{
			get
			{
				return
					Enumerable.Range(0, 13).Select(n => new WinChartData {Index = n.ToString(), ItemsSource = ArenaStats.Instance.WinsByClass[n]});
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public class WinChartData
		{
			public string Index { get; set; }
			public IEnumerable<ChartStats> ItemsSource { get; set; }
		}
	}
}
