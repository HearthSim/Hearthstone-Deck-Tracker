using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Card : ICloneable, INotifyPropertyChanged
	{
		public string Id;

		[XmlIgnore]
		public string PlayerClass;

		[XmlIgnore]
		public string Rarity;

		private int _count;
		private int _inHandCount;
		private bool _isStolen;
		private bool _justDrawn;
		private string _localizedName;
		private string _name;
		private string _text;
		private bool _wasDiscarded;

        /// The mechanics attribute, such as windfury or taunt, comes from the cardDB json file
        [XmlIgnore]
        public string[] Mechanics;

		public Card()
		{
			Count = 1;
		}

		public Card(string id, string playerClass, string rarity, string type, string name, int cost, string localizedName,
		            int inHandCount, int count, string text, int attack, int health, string race, string [] mechanics, int? durability)
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
			Text = text;
			Attack = attack;
			Health = health;
			Race = race;
			Durability = durability;
            Mechanics = mechanics;
            
		}

		public int Count
		{
			get { return _count; }
			set
			{
				_count = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public int Attack { get; set; }

		[XmlIgnore]
		public int Health { get; set; }

		[XmlIgnore]
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value != null
					        ? value.Replace("<b>", "")
					               .Replace("</b>", "")
					               .Replace("<i>", "")
					               .Replace("</i>", "")
					               .Replace("$", "")
					               .Replace("#", "")
					               .Replace("\\n", "\n")
					        : null;
			}
		}

		[XmlIgnore]
		public Visibility ShowIconsInTooltip
		{
			get { return Type == "Spell" || Type == "Enchantment" || Type == "Hero Power" ? Visibility.Hidden : Visibility.Visible; }
		}

		[XmlIgnore]
		public string Race { get; set; }

		[XmlIgnore]
		public string RaceOrType
		{
			get { return Race ?? Type; }
		}

		[XmlIgnore]
		public int? Durability { get; set; }

		[XmlIgnore]
		public int DurabilityOrHealth
		{
			get { return Durability ?? Health; }
		}

		[XmlIgnore]
		public string Type { get; set; }

		[XmlIgnore]
		public string Name
		{
			get
			{
				if(_name == null)
					Load();
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
		public int InHandCount
		{
			get { return _inHandCount; }
			set
			{
				_inHandCount = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public bool IsClassCard
		{
			get { return GetPlayerClass != "Neutral"; }
		}

		[XmlIgnore]
		public bool IsStolen
		{
			get { return _isStolen; }
			set
			{
				_isStolen = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public bool WasDiscarded
		{
			get { return _wasDiscarded; }
			set
			{
				_wasDiscarded = value;
				OnPropertyChanged();
			}
		}

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
				Color color;
				if(_justDrawn)
					color = Colors.Orange;
				else if(InHandCount > 0 && Game.HighlightCardsInHand || IsStolen)
					color = Colors.GreenYellow;
				else if(Count == 0)
					color = Colors.Gray;
				else if(WasDiscarded && Game.HighlightDiscarded)
					color = Colors.IndianRed;
				else
					color = Colors.White;
				return
					new SolidColorBrush(color);
			}
		}


		public SolidColorBrush ColorOpponent
		{
			get { return new SolidColorBrush(Colors.White); }
		}

		public ImageBrush Background
		{
			get
			{
				if(Id == null || Name == null)
					return new ImageBrush();
				try
				{
					var cardFileName =
						Name.ToLowerInvariant().Replace(' ', '-').Replace(":", "").Replace("'", "-").Replace(".", "").Replace("!", "") +
						".png";


					//card graphic
					var drawingGroup = new DrawingGroup();

					if(File.Exists("Images/" + cardFileName))
					{
						drawingGroup.Children.Add(
							new ImageDrawing(new BitmapImage(new Uri("Images/" + cardFileName, UriKind.Relative)),
							                 new Rect(104, 1, 110, 34)));
					}

					//frame
					drawingGroup.Children.Add(
						new ImageDrawing(new BitmapImage(new Uri("Images/frame.png", UriKind.Relative)),
						                 new Rect(0, 0, 218, 35)));

					//extra info?
					if(Count >= 2 || Rarity == "Legendary")
					{
						drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_countbox.png", UriKind.Relative)),
						                                           new Rect(189, 6, 25, 24)));

						if(Count >= 2 && Count <= 9)
						{
							drawingGroup.Children.Add(new ImageDrawing(
								                          new BitmapImage(new Uri("Images/frame_" + Count + ".png", UriKind.Relative)),
								                          new Rect(194, 8, 18, 21)));
						}
						else
						{
							drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_legendary.png", UriKind.Relative)),
							                                           new Rect(194, 8, 18, 21)));
						}
					}

					//dark overlay
					if(Count == 0)
					{
						drawingGroup.Children.Add(
							new ImageDrawing(new BitmapImage(new Uri("Images/dark.png", UriKind.Relative)),
							                 new Rect(0, 0, 218, 35)));
					}

					var brush = new ImageBrush {ImageSource = new DrawingImage(drawingGroup)};
					return brush;
				}
				catch(Exception)
				{
					return new ImageBrush();
				}
			}
		}

		public object Clone()
		{
			Card newcard = new Card(Id, PlayerClass, Rarity, Type, Name, Cost, LocalizedName, InHandCount, Count, Text, Attack, Health,
			                Race, Mechanics, Durability);
            return newcard;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override string ToString()
		{
			return Name + " (" + Count + ")";
		}

		public override bool Equals(object card)
		{
			if(!(card is Card))
				return false;
			var c = (Card)card;
			return c.Name == Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public void Load()
		{
			Debug.Assert(Id != null);

			var stats = Game.GetCardFromId(Id);
			PlayerClass = stats.PlayerClass;
			Rarity = stats.Rarity;
			Type = stats.Type;
			Name = stats.Name;
			Cost = stats.Cost;
			LocalizedName = stats.LocalizedName;
			InHandCount = stats.InHandCount;
			Text = stats.Text;
			Attack = stats.Attack;
			Health = stats.Health;
			Race = stats.Race;
			Durability = stats.Durability;
            Mechanics = stats.Mechanics;
			_wasDiscarded = false;
			OnPropertyChanged();
		}

		public async Task JustDrawn()
		{
			if(!Config.Instance.HighlightLastDrawn)
				return;

			_justDrawn = true;
			OnPropertyChanged("ColorPlayer");
			await Task.Delay(4000);
			_justDrawn = false;
			OnPropertyChanged("ColorPlayer");
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

