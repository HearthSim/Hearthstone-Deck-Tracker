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
				Update(!value);
			}
		}

		private bool _faded;

		public void SetFaded(bool faded)
		{
			if(faded == _faded || Active)
				return;
			_faded = faded;
			Update(true);
		}

		private void Update(bool updateRemove)
		{
			if(updateRemove)
				ImageTierRemove.Visibility = _hovering && Active ? Visibility.Visible : Visibility.Collapsed;
			Glow.Visibility = Active || _hovering ? Visibility.Visible : Visibility.Collapsed;
			Glow.Opacity = Active ? 1 : 0.5;
			ImageTier.Opacity = GetOpacity();
		}

		private double GetOpacity()
		{
			if(Active || _hovering || !_faded)
				return 1;
			return 0.3;
		}

		private bool _hovering;

		private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_hovering = true;
			Update(true);
		}

		private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_hovering = false;
			Update(true);
		}
	}
}
