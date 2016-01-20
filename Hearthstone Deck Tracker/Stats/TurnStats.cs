#region

using System.Collections.Generic;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class TurnStats
	{
		[XmlArray(ElementName = "Plays")]
		[XmlArrayItem(ElementName = "Play")]
		public List<Play> Plays;

		public int Turn;

		public TurnStats()
		{
			Plays = new List<Play>();
		}

		public void AddPlay(PlayType type, string cardId) => Plays.Add(new Play(type, cardId));

		public class Play
		{
			public string CardId;
			public PlayType Type;

			public Play()
			{
			}

			public Play(PlayType type, string cardId)
			{
				Type = type;
				CardId = cardId;
			}
		}
	}
}