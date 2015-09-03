#region

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	[Serializable]
	public class Card : ICloneable, INotifyPropertyChanged
	{
		private ImageBrush _cachedBackground;
		private bool _coloredFrame;
		private int _count;
		private int _inHandCount;
		private bool _isStolen;
		private bool _justDrawn;
		private int _lastCount;
		private bool _loaded;
		private string _localizedName;
		private string _name;
		private string _text;
		private string _englishText;
		private bool _wasDiscarded;
		public string Id;

		/// The mechanics attribute, such as windfury or taunt, comes from the cardDB json file
		[XmlIgnore]
		public string[] Mechanics;

		[XmlIgnore]
		public string PlayerClass;

		[XmlIgnore]
		public string Rarity;

		public Card()
		{
			Count = 1;
		}

		public Card(string id, string playerClass, string rarity, string type, string name, int cost, string localizedName, int inHandCount,
		            int count, string text, string englishText, int attack, int health, string race, string[] mechanics, int? durability, string artist,
		            string set)
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
			EnglishText = englishText;
			Attack = attack;
			Health = health;
			Race = race;
			Durability = durability;
			Mechanics = mechanics;
			Artist = artist;
			Set = set;
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
		public bool Jousted { get; set; }

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
								   .Replace("\\n", "\n") : null;
			}
		}

		[XmlIgnore]
		public string EnglishText
		{
			get { return string.IsNullOrEmpty(_englishText) ? Text : _englishText; }
			set
			{
				_englishText = value != null
							? value.Replace("<b>", "")
								   .Replace("</b>", "")
								   .Replace("<i>", "")
								   .Replace("</i>", "")
								   .Replace("$", "")
								   .Replace("#", "")
								   .Replace("\\n", "\n") : null;
			}
		}

		[XmlIgnore]
		public Visibility ShowIconsInTooltip
		{
			get { return Type == "Spell" || Type == "Enchantment" || Type == "Hero Power" ? Visibility.Hidden : Visibility.Visible; }
		}

		[XmlIgnore]
		public string Set { get; set; }

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

		private readonly Regex _overloadRegex = new Regex(@"Overload:.+?\((?<value>(\d+))\)");
		private int? _overload;
	    

	    [XmlIgnore]
		public int Overload
		{
			get
			{
				if(_overload.HasValue)
					return _overload.Value;
				var overload = -1;
				if(!string.IsNullOrEmpty(EnglishText))
				{
					var match = _overloadRegex.Match(EnglishText);
					if(match.Success)
						int.TryParse(match.Groups["value"].Value, out overload);
					_overload = overload;
				}
				return overload;
			}
		}

		[XmlIgnore]
		public string Artist { get; set; }

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
			get { return (int)Math.Round(OverlayWindow.Scaling * 35, 0); }
		}

		public int OpponentHeight
		{
			get { return (int)Math.Round(OverlayWindow.OpponentScaling * 35, 0); }
		}

		public int PlayerWindowHeight
		{
			get { return (int)Math.Round(PlayerWindow.Scaling * 35, 0); }
		}

		public int OpponentWindowHeight
		{
			get { return (int)Math.Round(OpponentWindow.Scaling * 35, 0); }
		}

		public string GetPlayerClass
		{
			get { return PlayerClass ?? "Neutral"; }
		}

		public SolidColorBrush ColorPlayer
		{
            //TODO: Consider moving this out of the Card class as it shouldn't care about the state of the Game 
			get
			{
				Color color;
				if(_justDrawn)
					color = Colors.Orange;
				else if(InHandCount > 0 && _game.HighlightCardsInHand || IsStolen)
					color = Colors.GreenYellow;
				else if(Count <= 0 || Jousted)
					color = Colors.Gray;
				else if(WasDiscarded && _game.HighlightDiscarded)
					color = Colors.IndianRed;
				else
					color = Colors.White;
				return new SolidColorBrush(color);
			}
		}

		public SolidColorBrush ColorOpponent
		{
			get { return new SolidColorBrush(Colors.White); }
		}

		public string CardFileName
		{
			get
			{
				return Name.ToLowerInvariant()
				           .Replace(' ', '-')
				           .Replace(":", "")
				           .Replace("'", "-")
				           .Replace(".", "")
				           .Replace("!", "")
				           .Replace(",", "");
			}
		}

		public ImageBrush Background
		{
			get
			{
				if(_cachedBackground != null && Count == _lastCount && _coloredFrame == Config.Instance.RarityCardFrames)
					return _cachedBackground;
				_lastCount = Count;
				_coloredFrame = Config.Instance.RarityCardFrames;
				if(Id == null || Name == null)
					return new ImageBrush();
				try
				{
					var cardFileName = CardFileName + ".png";


					//card graphic
					var drawingGroup = new DrawingGroup();

					if(File.Exists("Images/" + cardFileName))
					{
						drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/" + cardFileName, UriKind.Relative)),
						                                           new Rect(104, 1, 110, 34)));
					}

					//frame
					var frame = "Images/frame.png";
					if(Config.Instance.RarityCardFrames)
					{
						switch(Rarity)
						{
							case "Common":
								frame = "Images/frame_rarity_common.png";
								break;
							case "Rare":
								frame = "Images/frame_rarity_rare.png";
								break;
							case "Epic":
								frame = "Images/frame_rarity_epic.png";
								break;
							case "Legendary":
								frame = "Images/frame_rarity_legendary.png";
								break;
						}
					}
					drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(frame, UriKind.Relative)), new Rect(0, 0, 218, 35)));

					//extra info?
					if(Math.Abs(Count) > 1 || Rarity == "Legendary")
					{
						drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_countbox.png", UriKind.Relative)),
						                                           new Rect(189, 6, 25, 24)));

						if(Math.Abs(Count) > 1 && Math.Abs(Count) <= 9)
						{
							drawingGroup.Children.Add(
							                          new ImageDrawing(
								                          new BitmapImage(new Uri("Images/frame_" + Math.Abs(Count) + ".png", UriKind.Relative)),
								                          new Rect(194, 8, 18, 21)));
						}
						else
						{
							drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/frame_legendary.png", UriKind.Relative)),
							                                           new Rect(194, 8, 18, 21)));
						}
					}

					//dark overlay
					if(Count <= 0 || Jousted)
						drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri("Images/dark.png", UriKind.Relative)), new Rect(0, 0, 218, 35)));

					var brush = new ImageBrush {ImageSource = new DrawingImage(drawingGroup)};
					_cachedBackground = brush;
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
			var newcard = new Card(Id, PlayerClass, Rarity, Type, Name, Cost, LocalizedName, InHandCount, Count, Text, EnglishText, Attack, Health, Race,
			                       Mechanics, Durability, Artist, Set);
			return newcard;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override string ToString()
		{
			return Name + "(" + Count + ")";
		}

		public override bool Equals(object card)
		{
			if(!(card is Card))
				return false;
			var c = (Card)card;
			return c.Name == Name;
		}

		public bool EqualsWithCount(Card card)
		{
			return card.Id == Id && card.Count == Count;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}


        //TODO: This is to get around having a static Game class unless we change ColorPlayer
        private static GameV2 _game;
        public static void SetGame(GameV2 game)
        {
            _game = game;
        }

		public void Load()
		{
			if(_loaded)
				return;

			var stats = Database.GetCardFromId(Id);
			PlayerClass = stats.PlayerClass;
			Rarity = stats.Rarity;
			Type = stats.Type;
			Name = stats.Name;
			Cost = stats.Cost;
			LocalizedName = stats.LocalizedName;
			InHandCount = stats.InHandCount;
			Text = stats.Text;
			EnglishText = stats.EnglishText;
			Attack = stats.Attack;
			Health = stats.Health;
			Race = stats.Race;
			Durability = stats.Durability;
			Mechanics = stats.Mechanics;
			Artist = stats.Artist;
			Set = stats.Set;
			_wasDiscarded = false;
			_loaded = true;
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