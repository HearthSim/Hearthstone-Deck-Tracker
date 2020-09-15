using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for BattlegroundsMinion.xaml
	/// </summary>
	public partial class BattlegroundsMinion
	{
		public static readonly DependencyProperty HasTooltipProperty = DependencyProperty.Register("HasTooltip", typeof(bool), typeof(Card), new PropertyMetadata(Config.Instance.TrackerCardToolTips));

		public Visibility PoisonousVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility DivineShieldVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility TauntVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility PremiumTauntVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}
		public Visibility DeathrattleVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility LegendaryBorderVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility PremiumLegendaryBorderVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility PremiumBorderVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public Visibility BorderVisibility
		{
			get
			{
				return Visibility.Hidden;
			}
		}

		public BattlegroundsMinion()
		{
			InitializeComponent();
		}

		
	}
}
