using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
    public class Decks
    {
        [XmlElement(ElementName = "Deck")]
        public ObservableCollection<Deck> DecksList;
    }
    public class Deck : ICloneable
    {
        public string Name;
        public string Class;
        
        [XmlIgnore]
        public string GetClass
        {
            get { return string.IsNullOrEmpty(Class) ? "(No Class Selected)" : "(" + Class + ")"; }
        }
        
        [XmlIgnore]
        public string GetName
        {
            get { return Name == "" ? "No Name Set" : Name; }
        }

        [XmlIgnore]
        public SolidColorBrush ClassColorBrush
        {
            get
            {
                return new SolidColorBrush(ClassColor);
            }
        }

        [XmlIgnore]
        public Color ClassColor
        {
            get
            {
                switch (Class)
                {
                    case "Druid":
                        return (Color)ColorConverter.ConvertFromString("#FF7D0A");
                        break;
                    case "Death Knight":
                        return (Color)ColorConverter.ConvertFromString("#C41F3B");
                        break;
                    case "Hunter":
                        return (Color)ColorConverter.ConvertFromString("#ABD473");
                        break;
                    case "Mage":
                        return (Color)ColorConverter.ConvertFromString("#69CCF0");
                        break;
                    case "Monk":
                        return (Color)ColorConverter.ConvertFromString("#00FF96");
                        break;
                    case "Paladin":
                        return (Color)ColorConverter.ConvertFromString("#F58CBA");
                        break;
                    case "Priest":
                        return (Color)ColorConverter.ConvertFromString("#FFFFFF");
                        break;
                    case "Rogue":
                        return  (Color)ColorConverter.ConvertFromString("#FFF569");
                        break;
                    case "Shaman":
                        return  (Color)ColorConverter.ConvertFromString("#0070DE");
                        break;
                    case "Warlock":
                        return (Color)ColorConverter.ConvertFromString("#9482C9");
                        break;
                    case "Warrior":
                        return (Color)ColorConverter.ConvertFromString("#C79C6E");
                        break;
                    default:
                        return Colors.Gray;
                        break;
                }
            }
        }

        [XmlArray(ElementName = "Cards")]
        [XmlArrayItem(ElementName = "Card")]
        public ObservableCollection<Card> Cards;

        public Deck()
        {
            Cards = new ObservableCollection<Card>();
        }

        public Deck(string name, string className, ObservableCollection<Card> cards)
        {
            Name = name;
            Class = className;
            Cards = new ObservableCollection<Card>(cards);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Class);
        }

        public object Clone()
        {
            return new Deck(Name, Class, Cards);
        }

        public override bool Equals(object obj)
        {
            var deck = obj as Deck;
            if (deck == null) return false;
            return Name == deck.Name;
        }
    }
}
