using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsTierTriples : INotifyPropertyChanged
	{
		private readonly BrushConverter _brushConverter = new();

		public BattlegroundsTierTriples()
		{
			InitializeComponent();
			Tier = 1;
			Qty = 0;
			TextColor = (SolidColorBrush) _brushConverter.ConvertFrom("#74FFFFFF");
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TierProperty = DependencyProperty.Register("Tier", typeof(int), typeof(BattlegroundsTierTriples));
		public static readonly DependencyProperty QtyProperty = DependencyProperty.Register(
			"Qty",
			typeof(int),
			typeof(BattlegroundsTierTriples),
			new FrameworkPropertyMetadata(1, OnQtyPropChanged)
		);

		private static void OnQtyPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BattlegroundsTierTriples)d).OnQtyChanged();
		}

		public string QtyText => $"x{Qty}";
		public SolidColorBrush TextColor { get; set; }

		public int Tier
		{
			get { return (int)GetValue(TierProperty); }
			set
			{
				SetValue(TierProperty, value);
			}
		}

		public int Qty
		{
			get { return (int)GetValue(QtyProperty); }
			set
			{
				SetValue(QtyProperty, value);
				OnQtyChanged();
			}
		}

		private void OnQtyChanged()
		{
			OnPropertyChanged(nameof(QtyText));

			TextColor = (SolidColorBrush) _brushConverter.ConvertFrom(
				Qty > 0 ? "#FFFFFF" : "#747474"
			);
			OnPropertyChanged(nameof(TextColor));
		}
	}
}

