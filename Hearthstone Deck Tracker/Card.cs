using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
    public class Card : ICloneable
    {
        public Card()
        {
            Count = 1;
        }

        public Card(string id, string playerClass, string rarity, string type, string name, int cost, string localizedName,
                    int inHandCount, int count)
        {
            Id = id;
            PlayerClass = playerClass;
            Rarity = rarity;
            Type = type;
            Name = name;
            Cost = cost;
            LocalizedName = localizedName;
            InHandCount = inHandCount;
            Count = count;
        }
        

        private string _localizedName;
        private string _name;

        public int Count;
        public string Id;

        [XmlIgnore]
        public string PlayerClass;

        [XmlIgnore]
        public string Rarity;

        [XmlIgnore]
        public string Type { get; set; }

        [XmlIgnore]
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    Load();
                }
                return _name;
            }
            set { _name = value; }
        }

        [XmlIgnore]
        public int Cost { get; set; }

        [XmlIgnore]
        public string LocalizedName
        {
            get { return string.IsNullOrEmpty(_localizedName) ? Name : _localizedName; }
            set { _localizedName = value; }
        }

        [XmlIgnore]
        public int InHandCount;

        [XmlIgnore] 
        public bool IsStolen { get; set; }

        public int Height
        {
            get { return (int)(OverlayWindow.Scaling * 35); }
        }
        public int OpponentHeight
        {
            get { return (int)(OverlayWindow.OpponentScaling * 35); }
        }
        public int PlayerWindowHeight
        {
            get { return (int)(PlayerWindow.Scaling * 35); }
        }

        public int OpponentWindowHeight
        {
            get { return (int)(OpponentWindow.Scaling * 35); }
        }


        public string GetPlayerClass
        {
            get { return PlayerClass ?? "Neutral"; }
        }

        public SolidColorBrush ColorPlayer
        {
            get
            {
                return
                    new SolidColorBrush((InHandCount > 0 && Hearthstone.HighlightCardsInHand || IsStolen)
                                            ? Colors.GreenYellow
                                            : (Count != 0) ? Colors.White : Colors.Gray);
            }
        }

       
        public SolidColorBrush ColorEnemy
        {
            get { return new SolidColorBrush(Colors.White); }
        }

        public ImageBrush Background
        {
            get
            {
                if (Id == null || Name == null)
                {
                    return new ImageBrush();
                }
                try
                {
                    string cardFileName = Name.ToLower().Replace(' ', '-').Replace(":", "").Replace("'", "-").Replace(".", "").Replace("!", "") + ".png";

                    
                   //card graphic
                    var group = new DrawingGroup();

                    if (File.Exists("Images/" + cardFileName))
                    {
                        group.Children.Add(
                            new ImageDrawing(new BitmapImage(new Uri("Images/" + cardFileName, UriKind.Relative)),
                                             new Rect(104, 0, 110, 35)));
                    }

                    //frame
                    group.Children.Add(
                        new ImageDrawing(new BitmapImage(new Uri("Images/frame.png", UriKind.Relative)),
                            new Rect(0, 0, 218, 35)));

                    //extra info?
                    if (Count >= 2 || Rarity == "Legendary")
                    {
                        group.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_countbox.png", UriKind.Relative)), new Rect(189, 6, 25, 24)));

                        if (Count >= 2 && Count <= 9)
                        {
                            group.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_" + Count + ".png", UriKind.Relative)), new Rect(194, 8, 18, 21)));
                        }
                        else
                        {
                            group.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_legendary.png", UriKind.Relative)), new Rect(194, 8, 18, 21)));
                        }
                    }

                    //dark overlay
                    if (Count == 0)
                    {
                        group.Children.Add(
                            new ImageDrawing(new BitmapImage(new Uri("Images/dark.png", UriKind.Relative)),
                                             new Rect(0, 0, 218, 35)));
                    }

                    var brush = new ImageBrush();
                    brush.ImageSource = new DrawingImage(group);
                    return brush;
                }
                catch (Exception)
                {
                    return new ImageBrush();
                }
            }
        }
        
        public override string ToString()
        {
            return Name + " (" + Count + ")";
        }

        public override bool Equals(object card)
        {
            if (!(card is Card))
                return false;
            var c = (Card)card;
            return c.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public object Clone()
        {
            return new Card(Id, PlayerClass, Rarity, Type, Name, Cost, LocalizedName, InHandCount, Count);
        }

        private void Load()
        {
            Debug.Assert(Id != null);

            var stats = Hearthstone.GetCardFromId(Id);
            PlayerClass = stats.PlayerClass;
            Rarity = stats.Rarity;
            Type = stats.Type;
            Name = stats.Name;
            Cost = stats.Cost;
            LocalizedName = stats.LocalizedName;
            InHandCount = stats.InHandCount;
        }
    }
}