using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsSession : UserControl
	{
		private Lazy<BattlegroundsDb> _db = new Lazy<BattlegroundsDb>();

		public BattlegroundsSession()
		{
			InitializeComponent();
		}

		private void Update()
		{
			var allRaces = _db.Value.Races;
			var availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId) ?? allRaces;
			var unavailableRaces = allRaces.Where(x => !availableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL).ToList();

			if(unavailableRaces.Count() >= 3)
			{
				BgTribe1.Tribe = unavailableRaces[0];
				BgTribe2.Tribe = unavailableRaces[1];
				BgTribe3.Tribe = unavailableRaces[2];
				if(unavailableRaces.Count() == 4)
				{
					BgTribe4.Tribe = unavailableRaces[3];
				} else
				{
					BgBannedTribes.Children.Remove(BgTribe4);
					BgTribe2.Margin = new Thickness(15, 0, 0, 0);
					BgTribe3.Margin = new Thickness(15, 0, 0, 0);
				}
			}
		}

		public void Show()
		{
			if (Visibility == Visibility.Visible)
			{
				return;
			}
			Update();
			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			Visibility = Visibility.Hidden;
		}
	}
}
