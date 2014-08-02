using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Deck : ICloneable
	{
		[XmlArray(ElementName = "Cards")] [XmlArrayItem(ElementName = "Card")] public ObservableCollection<Card> Cards;
		public string Class;
		[XmlIgnore] public bool IsSelectedInGui;
		public DateTime LastEdited;
		public string Name;
		public string Note;

		[XmlArray(ElementName = "Tags")] [XmlArrayItem(ElementName = "Tag")] public List<string> Tags;
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
			foreach (var card in cards)
			{
				Cards.Add((Card) card.Clone());
			}
			Tags = new List<string>(tags);
			Note = note;
			Url = url;
			LastEdited = lastEdited;
		}

		[XmlIgnore]
		public string GetClass
		{
			get { return string.IsNullOrEmpty(Class) ? "(No Class Selected)" : "(" + Class + ")"; }
		}

		[XmlIgnore]
		public string GetName
		{
			get
			{
				var charCount = IsSelectedInGui ? 20 : 25;
				var tmpName = Name.Length > charCount ? string.Join("", Name.Take(charCount)) + "..." : Name;
				return IsSelectedInGui ? string.Format("> {0} <", tmpName) : tmpName;
			}
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
				switch (Class)
				{
					case "Druid":
						return (Color) ColorConverter.ConvertFromString("#FF7D0A");
					case "Death Knight":
						return (Color) ColorConverter.ConvertFromString("#C41F3B");
					case "Hunter":
						return (Color) ColorConverter.ConvertFromString("#ABD473");
					case "Mage":
						return (Color) ColorConverter.ConvertFromString("#69CCF0");
					case "Monk":
						return (Color) ColorConverter.ConvertFromString("#00FF96");
					case "Paladin":
						return (Color) ColorConverter.ConvertFromString("#F58CBA");
					case "Priest":
						return (Color) ColorConverter.ConvertFromString("#FFFFFF");
					case "Rogue":
						return (Color) ColorConverter.ConvertFromString("#FFF569");
					case "Shaman":
						return (Color) ColorConverter.ConvertFromString("#0070DE");
					case "Warlock":
						return (Color) ColorConverter.ConvertFromString("#9482C9");
					case "Warrior":
						return (Color) ColorConverter.ConvertFromString("#C79C6E");
					default:
						return Colors.Gray;
				}
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
			if (deck == null) return false;
			return Name == deck.Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}