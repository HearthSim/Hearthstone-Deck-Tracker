using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using CardIds = HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class DeckSideboards : UserControl
	{
		public DeckSideboards()
		{
			InitializeComponent();
		}

		public async void Update(List<Sideboard> sideboards, bool reset)
		{
			if(sideboards.Count == 0 || sideboards.All(s => s.Cards.Count == 0))
			{
				Container.Visibility = Visibility.Collapsed;
				return;
			}

			Container.Visibility = Visibility.Visible;
			var etcSideboard =
				sideboards.FirstOrDefault(s => s.OwnerCardId == CardIds.Collectible.Neutral.ETCBandManager);
			if(etcSideboard != null)
			{
				await CardList.UpdateAsync(etcSideboard.Cards, reset);
				if(etcSideboard.Cards.Count > 0)
					ETCContainer.Visibility = Visibility.Visible;
			}
		}
	}

}
