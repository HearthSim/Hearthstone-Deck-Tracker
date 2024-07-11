using System.Security.Cryptography;
using System;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition
{
	public class BattlegroundsCompositionStatsRowViewModel : ViewModel
	{
		private readonly double _firstPlacePercent;
		private readonly double _maxPercentage;
		private readonly double _avgPlacement;

		public BattlegroundsCompositionStatsRowViewModel(string name, int minionDbfId, double firstPlacePercent, double avgPlacement, double maxPercentage)
		{
			Name = name;

			var minionCard = Database.GetCardFromDbfId(minionDbfId, false);
			CardImage = new CardAssetViewModel(minionCard, Utility.Assets.CardAssetType.Tile);

			_firstPlacePercent = Math.Round(firstPlacePercent, 1);
			_maxPercentage = Math.Round(maxPercentage, 1);
			_avgPlacement = Math.Round(avgPlacement, 2);
		}

		public CardAssetViewModel CardImage { get; }

		public string Name { get; }

		public double MaxBarPercentage => _maxPercentage;
		public double FirstPlacePercent => _firstPlacePercent;
		public string AvgPlacement => $"{_avgPlacement:0.00}";

		public string AvgPlacementColor
		{
			get
			{
				var pivot = 4.5f;
				var factor = 1f;

				return Helper.GetColorString(
					Helper.ColorStringMode.BATTLEGROUNDS,
					(pivot - _avgPlacement) * 100f / 3.5f * factor,
					75,
					1.3
				);
			}
		}
	}
}
