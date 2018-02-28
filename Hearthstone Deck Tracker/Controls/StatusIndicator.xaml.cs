using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class StatusIndicator : INotifyPropertyChanged
	{
		public static readonly DependencyProperty SuccessProperty = DependencyProperty.Register(
			"Success", typeof(bool), typeof(StatusIndicator), new FrameworkPropertyMetadata(OnSuccessChanged));

		public StatusIndicator()
		{
			InitializeComponent();
		}

		public bool Success
		{
			get => (bool) GetValue(SuccessProperty);
			set => SetValue(SuccessProperty, value);
		}

		public SolidColorBrush Color => new SolidColorBrush(Success ? Colors.Green : Colors.Red);
		public string Icon => Success ? "✔" : "✖";

		public event PropertyChangedEventHandler PropertyChanged;

		private static void OnSuccessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is StatusIndicator s) s.Update();
		}

		private void Update()
		{
			OnPropertyChanged(nameof(Color));
			OnPropertyChanged(nameof(Icon));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
