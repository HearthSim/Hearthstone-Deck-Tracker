using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility;

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
			Turn = 1;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TierProperty = DependencyProperty.Register(
			"Tier",
			typeof(int),
			typeof(BattlegroundsTierTriples)
		);
		public static readonly DependencyProperty QtyProperty = DependencyProperty.Register(
			"Qty",
			typeof(int),
			typeof(BattlegroundsTierTriples),
			new FrameworkPropertyMetadata(1, (d, _) => ((BattlegroundsTierTriples)d).OnQtyChanged())
		);
		public static readonly DependencyProperty TurnProperty = DependencyProperty.Register(
			"Turn",
			typeof(int),
			typeof(BattlegroundsTierTriples),
			new FrameworkPropertyMetadata(1, (d, _) => ((BattlegroundsTierTriples)d).OnTurnChanged())
		);

		public SolidColorBrush BgColor => (SolidColorBrush) _brushConverter.ConvertFrom(Turn > 0 ? "#37393C" : "#282b2e");
		public double TierLeft => Turn > 0 ? 0 : 12;
		public double TierTop => Turn > 0 ? 2 : 7;
		public double TierOpacity => Turn > 0 ? 1 : 0.5;
		public double TripleOpacity => Qty > 0 ? 0 : 0.2;
		public Visibility TripleVisibility => Turn > 0 ? Visibility.Visible : Visibility.Collapsed;
		public string QtyText => Turn > 0 ? $"{Qty}" : "";
		public string TurnText => Turn > 0 ? string.Format(LocUtil.Get("Overlay_Battlegrounds_Turn_Counter"), Turn) : "";

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

		public int Turn
		{
			get { return (int)GetValue(TurnProperty); }
			set
			{
				SetValue(TurnProperty, value);
				OnTurnChanged();
			}
		}

		private void OnQtyChanged()
		{
			OnPropertyChanged(nameof(QtyText));
			OnPropertyChanged(nameof(TripleOpacity));
		}

		private void OnTurnChanged()
		{
			OnPropertyChanged(nameof(TurnText));
			OnPropertyChanged(nameof(QtyText));
			OnPropertyChanged(nameof(BgColor));
			OnPropertyChanged(nameof(TierLeft));
			OnPropertyChanged(nameof(TierTop));
			OnPropertyChanged(nameof(TierOpacity));
			OnPropertyChanged(nameof(TripleVisibility));
		}
	}
}

