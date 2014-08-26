using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Deck : ICloneable
	{
		[XmlArray(ElementName = "Cards")]
		[XmlArrayItem(ElementName = "Card")]
		public ObservableCollection<Card> Cards;

		public string Class;

		[XmlIgnore]
		public bool IsSelectedInGui;

		public DateTime LastEdited;
		public string Name;
		public string Note;

		[XmlArray(ElementName = "Tags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> Tags;

		public string Url;

		public Deck()
		{
			Cards = new ObservableCollection<Card>();
			Tags = new List<string>();
			Note = string.Empty;
			Url = string.Empty;                       
		}


		public Deck(string name, string className, IEnumerable<Card> cards, IEnumerable<string> tags, string note, string url,
		            DateTime lastEdited)
		{
			Name = name;
			Class = className;
			Cards = new ObservableCollection<Card>();
            foreach(var card in cards)
                Cards.Add((Card)card.Clone());
			Tags = new List<string>(tags);
			Note = note;
			Url = url;
			LastEdited = lastEdited;
		}

        /// returns the number of cards in the deck with mechanics matching the newmechanic.
        /// The mechanic attribute, such as windfury or taunt, comes from the cardDB json file
        public int getMechanicCount(String newmechanic)
        {
            int count;

            count = 0;
            foreach (var card in Cards)
            {
                if (card.Mechanics != null)
                {
                    foreach (String mechanic in card.Mechanics)
                    {
                        if (mechanic.Equals(newmechanic))
                        {
                            count++;
                        }
                    }
                }
            }

            Console.WriteLine(newmechanic + count.ToString() + "\n");
            return count;
        }

        public int getNumTaunt()
        {
            return getMechanicCount("Taunt");
        }

        public int getNumBattlecry()
        {
            return getMechanicCount("Battlecry");
        }

        public int getNumImmuneToSpellpower()
        {
            return getMechanicCount("ImmuneToSpellpower");
        }

        public int getNumSpellpower()
        {
            return getMechanicCount("Spellpower");
        }

        public int getNumOneTurnEffect()
        {
            return getMechanicCount("OneTurnEffect");
        }

        public int getNumCharge()
        {
            return getMechanicCount("Charge") + getMechanicCount("GrantCharge");
        }

        public int getNumFreeze()
        {
            return getMechanicCount("Freeze");
        }

        public int getNumAdjacentBuff()
        {
            return getMechanicCount("AdjacentBuff");
        }

        public int getNumSecret()
        {
            return getMechanicCount("Secret");
        }

        public int getNumDeathrattle()
        {
            return getMechanicCount("Deathrattle");
        }

        public int getNumWindfury()
        {
            return getMechanicCount("Windfury");
        }

        public int getNumDivineShield()
        {
            return getMechanicCount("Divine Shield");
        }

        public int getNumCombo()
        {
            return getMechanicCount("Combo");
        }

		[XmlIgnore]
		public string WinPercentString
		{
			get
			{
				if(DeckStats.Games.Count == 0) return "-%";
				return Math.Round(WinPercent, 0) + "%";
			}
		}

		[XmlIgnore]
		public double WinPercent
		{
			get
			{

				if(DeckStats.Games.Count == 0) return 0.0;
				return 100.0 * DeckStats.Games.Count(g => g.Result == GameResult.Win) / DeckStats.Games.Count;
			}
		}

		[XmlIgnore]
		public string GetClass
		{
			get { return string.IsNullOrEmpty(Class) ? "(No Class Selected)" : "(" + Class + ")"; }
		}

		[XmlIgnore]
		public FontWeight GetFontWeight
		{
			get { return IsSelectedInGui ? FontWeights.Black : FontWeights.Bold; }
		}

		[XmlIgnore]
		public string TagList
		{
			get { return Tags.Count > 0 ? "[" + Tags.Aggregate((t, n) => t + ", " + n) + "]" : ""; }
		}

		[XmlIgnore]
		public SolidColorBrush ClassColorBrush
		{
			get { return new SolidColorBrush(ClassColor); }
		}

		[XmlIgnore]
		public Color ClassColor
		{
			get
			{
				switch(Class)
				{
					case "Druid":
						return (Color)ColorConverter.ConvertFromString("#FF7D0A");
					case "Death Knight":
						return (Color)ColorConverter.ConvertFromString("#C41F3B");
					case "Hunter":
						return (Color)ColorConverter.ConvertFromString("#ABD473");
					case "Mage":
						return (Color)ColorConverter.ConvertFromString("#69CCF0");
					case "Monk":
						return (Color)ColorConverter.ConvertFromString("#00FF96");
					case "Paladin":
						return (Color)ColorConverter.ConvertFromString("#F58CBA");
					case "Priest":
						return (Color)ColorConverter.ConvertFromString("#FFFFFF");
					case "Rogue":
						return (Color)ColorConverter.ConvertFromString("#FFF569");
					case "Shaman":
						return (Color)ColorConverter.ConvertFromString("#0070DE");
					case "Warlock":
						return (Color)ColorConverter.ConvertFromString("#9482C9");
					case "Warrior":
						return (Color)ColorConverter.ConvertFromString("#C79C6E");
					default:
						return Colors.Gray;
				}
			}
		}

		public DeckStats DeckStats
		{
			get
			{
				var deckStats = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.Name == Name);
				if(deckStats == null)
				{
					deckStats = new DeckStats(Name);
					DeckStatsList.Instance.DeckStats.Add(deckStats);
				}
				return deckStats;
			}
		}

		public object Clone()
		{
			return new Deck(Name, Class, Cards, Tags, Note, Url, LastEdited);
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, Class);
		}

		public override bool Equals(object obj)
		{
			var deck = obj as Deck;
			if(deck == null) return false;
			return Name == deck.Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}

}