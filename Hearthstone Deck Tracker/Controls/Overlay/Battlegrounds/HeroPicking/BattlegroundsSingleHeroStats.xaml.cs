
using System.Windows;
using Hearthstone_Deck_Tracker.Utility;
using static Hearthstone_Deck_Tracker.Windows.OverlayWindow;

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

		private void Hero_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(e is not CustomMouseEventArgs)
				return;
			if(DataContext is BattlegroundsSingleHeroViewModel vm && vm.HeroDbfId.HasValue)
				SetSelectedHeroDbfIdCommand?.Execute(vm.HeroDbfId);
		}

		private void Hero_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(e is not CustomMouseEventArgs)
				return;
			SetSelectedHeroDbfIdCommand?.Execute(0);
		}
	}
}
