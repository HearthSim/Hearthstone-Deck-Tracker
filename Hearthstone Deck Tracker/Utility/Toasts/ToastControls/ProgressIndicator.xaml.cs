using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class ProgressIndicator
	{
		public static readonly DependencyProperty ProgressStateProperty = DependencyProperty.Register(
			"ProgressState", typeof(ProgressIndicatorState), typeof(ProgressIndicator),
			new PropertyMetadata(ProgressIndicatorState.Working, ProgressStateChanged));

		private static void ProgressStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ProgressIndicatorViewModel)((ProgressIndicator)d).DataContext).State = (ProgressIndicatorState)e.NewValue;
		}

		public ProgressIndicator()
		{
			InitializeComponent();
			DataContext = new ProgressIndicatorViewModel();
		}

		public ProgressIndicatorState ProgressState
		{
			get { return (ProgressIndicatorState) GetValue(ProgressStateProperty); }
			set
			{
				SetValue(ProgressStateProperty, value);
			}
		}
	}

	public class ProgressIndicatorViewModel : INotifyPropertyChanged
	{
		private ProgressIndicatorState _state = ProgressIndicatorState.Working;

		public Visibility ProgressRingVisibility => State == ProgressIndicatorState.Working ? Visibility.Visible : Visibility.Collapsed;

		public Visibility CheckMarkVisibility => State == ProgressIndicatorState.Success ? Visibility.Visible : Visibility.Collapsed;

		public Visibility ErrorVisibility => State == ProgressIndicatorState.Error ? Visibility.Visible : Visibility.Collapsed;

		public ProgressIndicatorState State
		{
			get { return _state; }
			set
			{
				if(_state == value)
					return;
				_state = value;
				OnPropertyChanged(nameof(ProgressRingVisibility));
				OnPropertyChanged(nameof(CheckMarkVisibility));
				OnPropertyChanged(nameof(ErrorVisibility));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public enum ProgressIndicatorState
	{
		Working,
		Success,
		Error
	}
}
