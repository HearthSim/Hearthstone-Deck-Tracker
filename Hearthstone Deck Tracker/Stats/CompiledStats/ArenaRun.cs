#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ArenaRun
	{
		private readonly Deck _deck;

		public ArenaRun(Deck deck)
		{
			_deck = deck;
		}

		public Deck Deck
		{
			get { return _deck; }
		}

		public string Class
		{
			get { return _deck.Class; }
		}

		public BitmapImage ClassImage
		{
			get { return _deck.ClassImage; }
		}

		public string StartTimeString
		{
			get { return StartTime == DateTime.MinValue ? "-" : StartTime.ToString("dd.MMM HH:mm"); }
		}

		public DateTime StartTime
		{
			get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.Min(g => g.StartTime) : DateTime.MinValue; }
		}

		public DateTime EndTime
		{
			get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.Max(g => g.EndTime) : DateTime.MinValue; }
		}

		public int Wins
		{
			get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Win); }
		}

		public int Losses
		{
			get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Loss); }
		}

		public int Gold
		{
			get { return _deck.ArenaReward.Gold; }
		}

		public int Dust
		{
			get { return _deck.ArenaReward.Dust; }
		}

		public ArenaRewardPacks[] Packs
		{
			get { return _deck.ArenaReward.Packs; }
		}

		public int PackCount
		{
			get { return _deck.ArenaReward.Packs.Count(x => x != ArenaRewardPacks.None); }
		}

		public string PackString
		{
			get
			{
				var packs = _deck.ArenaReward.Packs.Where(x => x != ArenaRewardPacks.None).ToList();
				return packs.Any()
					       ? packs.Select(x => EnumDescriptionConverter.GetDescription(x))
					              .Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int CardCount
		{
			get { return _deck.ArenaReward.Cards.Count(x => x != null && !string.IsNullOrEmpty(x.CardId)); }
		}

		public int CardCountGolden
		{
			get { return _deck.ArenaReward.Cards.Count(x => x != null && !string.IsNullOrEmpty(x.CardId) && x.Golden); }
		}

		public string CardString
		{
			get
			{
				var cards = _deck.ArenaReward.Cards.Where(x => x != null && !string.IsNullOrEmpty(x.CardId)).ToList();
				return cards.Any()
					       ? cards.Select(x => (Database.GetCardFromId(x.CardId).LocalizedName) + (x.Golden ? " (golden)" : ""))
					              .Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int Duration
		{
			get { return _deck.DeckStats.Games.Sum(x => x.SortableDuration); }
		}

		public string DurationString
		{
			get { return Duration + " min"; }
		}

		public string Region
		{
			get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.First().Region.ToString() : "UNKNOWN"; }
		}

		public IEnumerable<GameStats> Games
		{
			get { return _deck.DeckStats.Games; }
		}

		public override bool Equals(object obj)
		{
			var run = obj as ArenaRun;
			return run != null && Deck.Equals(run.Deck);
		}

		public override int GetHashCode()
		{
			return Deck.GetHashCode();
		}
	}
}