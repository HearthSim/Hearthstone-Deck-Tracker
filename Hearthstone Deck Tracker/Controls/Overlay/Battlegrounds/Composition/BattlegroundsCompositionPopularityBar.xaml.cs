using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition
{
	public partial class BattlegroundsCompositionPopularityBar : INotifyPropertyChanged
	{

		public BattlegroundsCompositionPopularityBar()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register(
			nameof(Highlight),
			typeof(bool),
			typeof(BattlegroundsCompositionPopularityBar),
			new PropertyMetadata(false, (d, _) => ((BattlegroundsCompositionPopularityBar)d).OnHighlightChanged())
		);
		public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
			nameof(Progress),
			typeof(double),
			typeof(BattlegroundsCompositionPopularityBar),
			new PropertyMetadata(0.0, (d, _) => ((BattlegroundsCompositionPopularityBar)d).OnProgressChanged())
		);

		public SolidColorBrush? BorderColor => Highlight ? Helper.BrushFromHex("#66FFFFFF") : Helper.BrushFromHex("#28FFFFFF");
		public string GradientColorTop => Highlight ? "#CCC58DC9" : "#CC78577A";
		public string GradientColorBottom => Highlight ? "#FFC58DC9" : "#CC78577A";

		public bool Highlight
		{
			get { return (bool)GetValue(HighlightProperty); }
			set
			{
				SetValue(HighlightProperty, value);
				OnPropertyChanged();
				OnHighlightChanged();
			}
		}

		public void OnHighlightChanged()
		{
			OnPropertyChanged(nameof(GradientColorTop));
			OnPropertyChanged(nameof(GradientColorBottom));
		}

		public double Progress
		{
			get { return (double)GetValue(ProgressProperty); }
			set
			{
				SetValue(ProgressProperty, value);
				OnPropertyChanged();
				OnProgressChanged();
			}
		}

		public void OnProgressChanged()
		{
			BarRect.Width = ActualWidth * Progress / 100;
		}
	}
}
