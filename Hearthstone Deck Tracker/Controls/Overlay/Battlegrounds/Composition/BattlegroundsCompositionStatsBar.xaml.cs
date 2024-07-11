using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition
{
	public partial class BattlegroundsCompositionStatsBar : UserControl
	{
		public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(
			"Percent", typeof(double), typeof(BattlegroundsCompositionStatsBar), new PropertyMetadata(0.0, OnPercentChanged));

		public static readonly DependencyProperty MaxPercentProperty = DependencyProperty.Register(
			"MaxPercent", typeof(double), typeof(BattlegroundsCompositionStatsBar),
			new PropertyMetadata(100.0, OnMaxPercentChanged));

		public double MaxPercent
		{
			get { return (double)GetValue(MaxPercentProperty); }
			set { SetValue(MaxPercentProperty, value); }
		}

		public double Percent
		{
			get { return (double)GetValue(PercentProperty); }
			set { SetValue(PercentProperty, value); }
		}

		public BattlegroundsCompositionStatsBar()
		{
			InitializeComponent();
		}

		private static void OnPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as BattlegroundsCompositionStatsBar;
			if(control != null)
			{
				double newPercent = (double)e.NewValue;
				// Ensure MaxPercent is not zero to avoid division by zero
				double safeMaxPercent = control.MaxPercent == 0 ? 100.0 : control.MaxPercent;
				control.progressBar.Width = (control.ActualWidth * newPercent) / safeMaxPercent;
				control.percentageText.Text = $"{newPercent:0.0}%";
			}
		}

		private static void OnMaxPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as BattlegroundsCompositionStatsBar;
			if(control != null)
			{
				// Reuse the existing OnPercentChanged logic to recalculate the progress bar
				// This ensures that changes to MaxPercent immediately affect the layout
				OnPercentChanged(d, new DependencyPropertyChangedEventArgs(PercentProperty, control.Percent, control.Percent));
			}
		}
	}
}
