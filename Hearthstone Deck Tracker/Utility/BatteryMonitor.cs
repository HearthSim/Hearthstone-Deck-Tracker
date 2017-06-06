#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class BatteryMonitor : INotifyPropertyChanged
	{
		public readonly TimeSpan CheckDelay = TimeSpan.FromSeconds(60);

		private float _percentLastCheck = 1f;
		private BatteryChargeStatus _statusLastCheck = BatteryChargeStatus.Unknown;
		private bool _stop;

		public BatteryMonitor()
		{
			CheckBatteryStatusAsync();
		}

		public Visual BatteryStatusVisual => (Visual)Core.MainWindow.FindResource(GetBatteryStatusVisualResourceName());

		public Visibility BatteryStatusVisualVisibility
			=>
				SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery
					? Visibility.Collapsed : Visibility.Visible;

		public string BatteryStatusPercent => Math.Round(SystemInformation.PowerStatus.BatteryLifePercent * 100, 0) + "%";

		public event PropertyChangedEventHandler PropertyChanged;

		private async void CheckBatteryStatusAsync()
		{
			while(!_stop)
			{
				if(_statusLastCheck != SystemInformation.PowerStatus.BatteryChargeStatus
				   || Math.Abs(_percentLastCheck - SystemInformation.PowerStatus.BatteryLifePercent) >= 0.01)
				{
					_statusLastCheck = SystemInformation.PowerStatus.BatteryChargeStatus;
					_percentLastCheck = SystemInformation.PowerStatus.BatteryLifePercent;
					OnPropertyChanged(nameof(BatteryStatusVisual));
					OnPropertyChanged(nameof(BatteryStatusVisualVisibility));
					OnPropertyChanged(nameof(BatteryStatusPercent));
				}
				await Task.Delay(CheckDelay);
			}
		}

		private string GetBatteryStatusVisualResourceName()
		{
			var percent = SystemInformation.PowerStatus.BatteryLifePercent;
			if(percent > 0.66)
				return "appbar_battery_3";
			if(percent > 0.33)
				return "appbar_battery_2";
			if(percent > 0.10)
				return "appbar_battery_1";
			return "appbar_battery_0";
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Stop() => _stop = true;
	}
}
