
using System.Windows;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public partial class BattlegroundsSingleHeroStats
	{
		public BattlegroundsSingleHeroStats()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty SetSelectedHeroDbfIdCommandProperty = DependencyProperty.Register(
			nameof(SetSelectedHeroDbfIdCommand),
			typeof(Command<int>),
			typeof(BattlegroundsSingleHeroStats),
			new PropertyMetadata(null)
		);

		public Command<int>? SetSelectedHeroDbfIdCommand
		{
			get { return (Command<int>?)GetValue(SetSelectedHeroDbfIdCommandProperty); }
			set { SetValue(SetSelectedHeroDbfIdCommandProperty, value); }
		}

		private void Hero_MouseIntersectionChanged(object sender, bool intersecting)
		{
			if(intersecting)
			{
				if(DataContext is BattlegroundsSingleHeroViewModel vm && vm.HeroDbfId.HasValue)
					SetSelectedHeroDbfIdCommand?.Execute(vm.HeroDbfId);
			}
			else
			{
				SetSelectedHeroDbfIdCommand?.Execute(0);
			}

		}
	}
}
