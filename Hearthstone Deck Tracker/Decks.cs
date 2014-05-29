using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
    public class Decks
    {
        [XmlElement(ElementName = "Deck")]
        public List<Deck> DecksList;
    }
    public class Deck
    {
        public string Name;

        [XmlArray(ElementName = "Cards")]
        [XmlArrayItem(ElementName = "Card")]
        public List<Card> Cards;
    }
}
