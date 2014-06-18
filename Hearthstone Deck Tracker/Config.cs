using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
    public class Config
    {
        [XmlIgnore]
        public readonly string ConfigPath = "config.xml";

        public string LastDeck = "";
        public bool HideInBackground = false;
        public bool HideInMenu = false;
        public bool HideEnemyCards = false;
        public bool HideEnemyCardCount = false;
        public bool HideOpponentDrawChances = false;
        public bool HidePlayerCardCount = false;
        public bool HideDrawChances = false;
        public bool HideOverlay = false;
        public int WindowHeight = 672;
        public string HearthstoneDirectory = "";
        public bool ShowInTaskbar = false;
        public bool HighlightCardsInHand = false;
        public int OffsetX = 0;
        public int OffsetY = 0;
        public int CustomWidth = -1;
        public int CustomHeight = -1;
        public bool VisibleOverlay = false;
        public int PlayerWindowHeight = 0;
        public double PlayerWindowTop = 0;
        public double PlayerWindowLeft = 0;
        public int OpponentWindowHeight = 0;
        public double OpponentWindowTop = 0;
        public double OpponentWindowLeft = 0;
        public bool WindowsTopmost = false;
        public bool WindowsTopmostIfHsForeground = false;
        public bool WindowsOnStartup = false;
        public string WindowsBackgroundHex = "";
        public int UpdateDelay = 100;
        public double PlayerDeckTop = 17;
        public double PlayerDeckHeight = 65;
        public double OpponentDeckHeight = 65;
        public double OpponentDeckTop = 17;
        public double PlayerDeckLeft = 99.5;
        public double OpponentDeckLeft = 0.5;
        public double OverlayOpacity = 100;
        public double OverlayPlayerScaling = 100;
        public double OverlayOpponentScaling = 100;
        public bool UseSameScaling = true;
        public bool KeepDecksVisible = true;
        public bool MinimizeToTray = false;
        public double TimerLeft = 75;
        public bool ShowAllDecks = false;

        [XmlArray(ElementName = "SelectedTags")]
        [XmlArrayItem(ElementName = "Tag")]
        public List<string> SelectedTags = new List<string>();

        public int ClickDelay = 50;
        public int SearchDelay = 100;
        public double CardPosX = 0.15;
        public double Card2PosX = 0.25;
        public double CardPosY = 0.3;
        public double SearchBoxX = 0.5;
        public double SearchBoxY = 0.89;
        public double SearchBoxYFullscreen = 0.92;

        public bool AutoDeckDetection = true;
        public bool AutoSelectDetectedDeck = true;
    }
}