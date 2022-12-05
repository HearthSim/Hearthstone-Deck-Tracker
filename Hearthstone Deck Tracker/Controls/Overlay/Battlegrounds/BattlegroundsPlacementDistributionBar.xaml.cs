using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	/// <summary>
	/// Interaction logic for BattlegroundsPlacementDistributionBar.xaml
	/// </summary>
	public partial class BattlegroundsPlacementDistributionBar : INotifyPropertyChanged
	{

		public BattlegroundsPlacementDistributionBar()
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
			typeof(BattlegroundsPlacementDistributionBar),
			new PropertyMetadata(false, (d, _) => ((BattlegroundsPlacementDistributionBar)d).OnHighlightChanged())
		);
		public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
			nameof(Placement),
			typeof(int),
			typeof(BattlegroundsPlacementDistributionBar),
			new PropertyMetadata(1)
		);
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(BattlegroundsPlacementDistributionBar),
			new PropertyMetadata(0.0, (d, _) => ((BattlegroundsPlacementDistributionBar)d).OnValueChanged())
		);
		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
			nameof(MaxValue),
			typeof(int),
			typeof(BattlegroundsPlacementDistributionBar),
			new PropertyMetadata(30, (d, _) => ((BattlegroundsPlacementDistributionBar)d).OnValueChanged())
		);

		public int Placement
		{
			get { return (int)GetValue(PlacementProperty); }
			set
			{
				SetValue(PlacementProperty, value);
				OnPropertyChanged();
			}
		}

		public SolidColorBrush? BorderColor => Highlight ? Helper.BrushFromHex("#66FFFFFF") : Helper.BrushFromHex("#28FFFFFF");
		public string GradientColorTop => Highlight ? "#CCC58DC9" : "#CC78577A";
		public string GradientColorBottom => Highlight ? "#FFC58DC9" : "#CC78577A";

		public bool Highlight
		{
			get { return (bool)GetValue(HighlightProperty); }
			set { SetValue(HighlightProperty, value); }
		}

		public void OnHighlightChanged()
		{
			OnPropertyChanged(nameof(GradientColorTop));
			OnPropertyChanged(nameof(GradientColorBottom));
		}

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set
			{
				SetValue(ValueProperty, value);
				OnPropertyChanged();
				OnValueChanged();
			}
		}
		public int MaxValue
		{
			get { return (int)GetValue(MaxValueProperty); }
			set
			{
				SetValue(MaxValueProperty, value);
				OnPropertyChanged();
				OnValueChanged();
			}
		}

		public void OnValueChanged()
		{
			var progress = Value / MaxValue * 100;
			BarRect.Height = ActualHeight * progress / 100;
		}
	}
}
