using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsOpponentInfo : UserControl
	{
		public BattlegroundsOpponentInfo()
		{
			InitializeComponent();
		}

		public void ShowNotFoughtOpponent()
		{
			BattlegroundsBoard.Children.Clear();
			NotFoughtOpponent.Visibility = Visibility.Visible;
			HeroNoMinionsOnBoard.Visibility = Visibility.Collapsed;
		}

		public void ClearLastKnownBoard()
		{
			BattlegroundsBoard.Children.Clear();
		}

		public bool Update(Entity hero, BoardSnapshot? state, int turnNumber)
		{
			var shouldAppears = true;
			BattlegroundsBoard.Children.Clear();
			NotFoughtOpponent.Visibility = Visibility.Collapsed;
			HeroNoMinionsOnBoard.Visibility = Visibility.Collapsed;
			if(state == null)
			{
				BattlegroundsAge.Text = "";
				if(hero.CardId != Core.Game.Player.Board.FirstOrDefault(x => x.IsHero)!.CardId)
					NotFoughtOpponent.Visibility = Visibility.Visible;
				else
					shouldAppears = false;
			}

			foreach(var e in state!.Entities)
				BattlegroundsBoard.Children.Add(new BattlegroundsMinion(e));
			if(!state.Entities.Any())
				HeroNoMinionsOnBoard.Visibility = Visibility.Visible;
			var age = turnNumber - state.Turn;
			BattlegroundsAge.Text = string.Format(LocUtil.Get("Overlay_Battlegrounds_Turns"), age);
			return shouldAppears;
		}
	}
}

