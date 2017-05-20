using System.Collections.Generic;
using System.ComponentModel;
using LiveCharts.Wpf;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public partial class LocalizedChartLegend : IChartLegend
	{
		public LocalizedChartLegend()
		{
			InitializeComponent();
			DataContext = this;
		}

		private List<SeriesViewModel> _series;

		public List<SeriesViewModel> Series
		{
			get => _series;
			set
			{
				_series = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
