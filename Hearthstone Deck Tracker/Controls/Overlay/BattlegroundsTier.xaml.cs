using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsTier : UserControl
	{
		public BattlegroundsTier()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TierProperty = DependencyProperty.Register("Tier", typeof(int), typeof(BattlegroundsTier));
		public static DependencyProperty ActiveProperty = DependencyProperty.Register("Active", typeof(bool), typeof(BattlegroundsTier));

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

		public bool Active
		{
			get { return (bool)GetValue(ActiveProperty); }
			set
			{
				SetValue(ActiveProperty, value);
			}
		}

	}
}
