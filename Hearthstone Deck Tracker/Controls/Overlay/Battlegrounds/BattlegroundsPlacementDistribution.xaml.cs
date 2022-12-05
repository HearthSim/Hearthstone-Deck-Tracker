using Hearthstone_Deck_Tracker.Annotations;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	/// <summary>
	/// Interaction logic for BattlegroundsPlacementDistribution.xaml
	/// </summary>
	public partial class BattlegroundsPlacementDistribution : INotifyPropertyChanged
	{
		public BattlegroundsPlacementDistribution()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
			nameof(MaxValue),
			typeof(int),
			typeof(BattlegroundsPlacementDistribution),
			new PropertyMetadata(30)
		);

		public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
			nameof(Values),
			typeof(double[]),
			typeof(BattlegroundsPlacementDistribution),
			new PropertyMetadata(new double[] { 0, 0, 0, 0, 0, 0, 0, 0 }, (d, _) => ((BattlegroundsPlacementDistribution)d).OnValuesChanged())
		);

		public int MaxValue
		{
			get { return (int)GetValue(MaxValueProperty); }
			set
			{
				SetValue(MaxValueProperty, value);
				OnPropertyChanged();
				OnValuesChanged();
			}
		}

		public double[] Values
		{
			get { return (double[])GetValue(ValuesProperty); }
			set
			{
				SetValue(ValuesProperty, value);
				OnPropertyChanged();
				OnValuesChanged();
			}
		}

		private void OnValuesChanged()
		{
			var maxFromValues = Values.Max();
			if(MaxValue < maxFromValues)
				MaxValue = (int)Math.Ceiling(maxFromValues);
			OnPropertyChanged(nameof(HasData));
		}

		public bool HasData => Values.Any(x => x > 0);

		public string Localized4th => LocUtil.GetPlacement(4);
		public string Localized8th => LocUtil.GetPlacement(8);
	}
}
