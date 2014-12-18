﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStats
	{
		[XmlArray(ElementName = "Games")]
		[XmlArrayItem(ElementName = "Game")]
		public List<GameStats> Games;

		public string Name;

		public DeckStats()
		{
			Games = new List<GameStats>();
		}

		public DeckStats(string name)
		{
			Name = name;
			Games = new List<GameStats>();
		}

		public void AddGameResult(GameResult result, string opponentHero, string playerHero)
		{
			Games.Add(new GameStats(result, opponentHero, playerHero));
		}

		public void AddGameResult(GameStats gameStats)
		{
			Games.Add(gameStats);
		}
	}	
}