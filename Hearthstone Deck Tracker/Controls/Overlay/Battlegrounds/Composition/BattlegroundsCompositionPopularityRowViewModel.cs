using Hearthstone_Deck_Tracker.Hearthstone;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition
{
	public class BattlegroundsCompositionPopularityRowViewModel : ViewModel
	{
		private readonly double _popularity;
		private readonly double _maxPopularity;
		private readonly bool _available;

		public BattlegroundsCompositionPopularityRowViewModel(string name, int minionDbfId, bool available, double popularity, double maxPopularity)
		{
			Name = name;

			var minionCard = Database.GetCardFromDbfId(minionDbfId, false);
			CardImage = new CardAssetViewModel(minionCard, Utility.Assets.CardAssetType.Tile);

			_popularity = popularity;
			_maxPopularity = maxPopularity;
			_available = available;
		}

		public CardAssetViewModel CardImage { get; }

		public string Name { get; }

		public double PopularityBarValue => _popularity / _maxPopularity * 100;
		public double Popularity => _popularity;

		public bool CompositionAvailable => _available;
		public Visibility CompositionUnavailableVisibility => _available ? Visibility.Hidden : Visibility.Visible;
		public double Opacity => _available ? 1.0 : 0.5;
	}
}
