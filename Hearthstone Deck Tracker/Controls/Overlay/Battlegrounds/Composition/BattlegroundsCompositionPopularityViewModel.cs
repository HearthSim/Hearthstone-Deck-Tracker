using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition
{
	public class BattlegroundsCompositionPopularityViewModel : ViewModel
	{
		public BattlegroundsCompositionPopularityViewModel(IEnumerable<BattlegroundsComposition> compsData)
		{
			var top3Comps = compsData.OrderByDescending(c => c.Popularity).Take(3).ToList();
			var max = Math.Max(Math.Ceiling(top3Comps[0].Popularity), 40);
			Top3Compositions = top3Comps.Select(compData => new BattlegroundsCompositionPopularityRowViewModel(
				compData.Name,
				compData.KeyMinionsTop3[0],
				compData.IsValid,
				compData.Popularity,
				max
			));
		}
		public IEnumerable<BattlegroundsCompositionPopularityRowViewModel> Top3Compositions { get; }
	}
}
