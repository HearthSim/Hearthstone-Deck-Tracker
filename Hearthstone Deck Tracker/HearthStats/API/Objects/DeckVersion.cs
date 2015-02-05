#region

using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class DeckVersion
	{
		public CardObject[] cards;
		public int deck_id;
		public int deck_version_id;
		public string version;

		public Deck ToDeck(Deck latest)
		{
			var clone = (Deck)latest.CloneWithNewId();
			clone.Cards = new ObservableCollection<Card>(cards.Select(c => c.ToCard()));
			clone.HearthStatsDeckVersionId = deck_version_id.ToString();
			clone.HearthStatsIdForUploading = latest.HearthStatsId;
			clone.Version = SerializableVersion.ParseOrDefault(version);
			clone.Versions.Clear();
			return clone;
		}
	}
}