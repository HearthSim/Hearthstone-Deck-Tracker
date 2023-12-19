using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsTier : UserControl, INotifyPropertyChanged
	{
		public BattlegroundsTier()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TierProperty = DependencyProperty.Register(
			nameof(Tier),
			typeof(int),
			typeof(BattlegroundsTier),
			new FrameworkPropertyMetadata(1, OnTierChanged)
		);

		public string ImageSrc => $"/HearthstoneDeckTracker;component/Resources/tier-{Tier}.png";

		public int Tier
		{
			get { return (int)GetValue(TierProperty); }
			set
			{
				SetValue(TierProperty, value);
				OnPropertyChanged(nameof(ImageSrc));
			}
		}

		private static void OnTierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BattlegroundsTier)d).OnPropertyChanged(nameof(ImageSrc));
		}
	}
}
