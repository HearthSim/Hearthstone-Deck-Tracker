#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options
{
	public class AdvancedOptions : INotifyPropertyChanged
	{
		public static AdvancedOptions Instance { get; } = new AdvancedOptions();
		public Visibility Visibility => Config.Instance.AdvancedOptions ? Visibility.Visible : Visibility.Collapsed;

		public bool Show
		{
			get { return Config.Instance.AdvancedOptions; }
			set
			{
				Config.Instance.AdvancedOptions = value;
				Config.Save();
				OnPropertyChanged();
				OnPropertyChanged(nameof(Visibility));
			}
		}

		public SolidColorBrush Color => new SolidColorBrush(Colors.YellowGreen);

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
