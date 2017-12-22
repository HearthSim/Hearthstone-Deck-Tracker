#region

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private BatteryMonitor _batteryMonitor;

		public Visibility BatteryStatusVisualVisibility => _batteryMonitor?.BatteryStatusVisualVisibility ?? Visibility.Collapsed;
		public Visibility BatteryStatusTextVisibility => Config.Instance.ShowBatteryLifePercent ? Visibility.Visible : Visibility.Collapsed;
		public string BatteryStatusPercent => _batteryMonitor?.BatteryStatusPercent;
		public Visual BatteryStatusVisual => _batteryMonitor?.BatteryStatusVisual;

		public void EnableBatteryMonitor()
		{
			if(_batteryMonitor != null)
				return;
			_batteryMonitor = new BatteryMonitor();
			_batteryMonitor.PropertyChanged += BatteryMonitorOnPropertyChanged;
			UpdateBatteryStatus();
		}

		public void DisableBatteryMonitor()
		{
			if(_batteryMonitor == null)
				return;
			_batteryMonitor.PropertyChanged -= BatteryMonitorOnPropertyChanged;
			_batteryMonitor.Stop();
			_batteryMonitor = null;
			UpdateBatteryStatus();
		}

		private void BatteryMonitorOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(_batteryMonitor.BatteryStatusVisual))
				OnPropertyChanged(nameof(BatteryStatusVisual));
			if(e.PropertyName == nameof(_batteryMonitor.BatteryStatusVisualVisibility))
				OnPropertyChanged(nameof(BatteryStatusVisualVisibility));
			if(e.PropertyName == nameof(_batteryMonitor.BatteryStatusPercent))
				OnPropertyChanged(nameof(BatteryStatusPercent));
		}

		public void UpdateBatteryStatus()
		{
			OnPropertyChanged(nameof(BatteryStatusPercent));
			OnPropertyChanged(nameof(BatteryStatusTextVisibility));
			OnPropertyChanged(nameof(BatteryStatusVisualVisibility));
			OnPropertyChanged(nameof(BatteryStatusVisual));
		}
	}
}
