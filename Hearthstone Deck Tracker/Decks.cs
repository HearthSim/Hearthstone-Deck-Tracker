#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class Decks
	{
		[XmlArray(ElementName = "Tags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> AllTags;

		[XmlElement(ElementName = "Deck")]
		public ObservableCollection<Deck> DecksList;

		public List<DeckInfo> LastDeckClass;
	}

	public class DeckInfo
	{
		public string Class;
		public string Name;
		public Guid Id;
	}
}