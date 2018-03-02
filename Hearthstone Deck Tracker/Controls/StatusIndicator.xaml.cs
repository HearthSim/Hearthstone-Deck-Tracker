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
			"Success", typeof(bool), typeof(StatusIndicator), new FrameworkPropertyMetadata(OnDependencyPropChanged));

		public static readonly DependencyProperty SuccessColorProperty = DependencyProperty.Register("SuccessColor",
			typeof(SolidColorBrush), typeof(StatusIndicator), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Green),
				FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropChanged));

		public static readonly DependencyProperty ErrorColorProperty = DependencyProperty.Register("ErrorColor",
			typeof(SolidColorBrush), typeof(StatusIndicator), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Red),
				FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropChanged));

		public StatusIndicator()
		{
			InitializeComponent();
		}

		public bool Success
		{
			get => (bool) GetValue(SuccessProperty);
			set => SetValue(SuccessProperty, value);
		}

		public SolidColorBrush SuccessColor
		{
			get => (SolidColorBrush) GetValue(SuccessColorProperty);
			set => SetValue(SuccessColorProperty, value);
		}

		public SolidColorBrush ErrorColor
		{
			get => (SolidColorBrush) GetValue(ErrorColorProperty);
			set => SetValue(ErrorColorProperty, value);
		}

		public SolidColorBrush Color => Success ? SuccessColor : ErrorColor;

		public string Icon => Success ? "✔" : "✖";

		public event PropertyChangedEventHandler PropertyChanged;

		private static void OnDependencyPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is StatusIndicator s)
				s.Update();
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
