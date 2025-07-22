#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ArenaRun
	{
		public ArenaRun(Deck deck)
		{
			Deck = deck;
		}

		public Deck Deck { get; }

		public string? Class => Deck.Class;

		public BitmapImage ClassImage => Deck.ClassImage;

		public string StartTimeString => StartTime == DateTime.MinValue ? "-" : StartTime.ToString("dd MMM yyyy HH:mm");

		public DateTime StartTime => Deck.DeckStats.Games.Any() ? Deck.DeckStats.Games.Min(g => g.StartTime) : DateTime.MinValue;

		public DateTime EndTime => Deck.DeckStats.Games.Any() ? Deck.DeckStats.Games.Max(g => g.EndTime) : DateTime.MinValue;

		public int Wins => Deck.DeckStats.Games.Count(x => x.Result == GameResult.Win);

		public int Losses => Deck.DeckStats.Games.Count(x => x.Result == GameResult.Loss);

		public int Gold => Deck.ArenaReward?.Gold ?? 0;

		public bool CrowdsFavor => Deck.ArenaReward?.CrowdsFavor ?? false;

		public int TavernTickets => Deck.ArenaReward?.TavernTickets ?? 0;

		public ArenaRewardPacks[] Packs => Deck.ArenaReward?.Packs ?? new ArenaRewardPacks[0];

		public int PackCount => Deck.ArenaReward?.Packs.Count(x => x != ArenaRewardPacks.None) ?? 0;

		public string PackString
		{
			get
			{
				var packs = Deck.ArenaReward?.Packs.Where(x => x != ArenaRewardPacks.None).ToList();
				return packs != null && packs.Any() ? packs.Select(x => EnumDescriptionConverter.GetDescription(x)).Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int CardCount => Deck.ArenaReward?.Cards.Count(x => !string.IsNullOrEmpty(x?.CardId)) ?? 0;

		public int CardCountGolden => Deck.ArenaReward?.Cards.Count(x => x != null && !string.IsNullOrEmpty(x.CardId) && x.Golden) ?? 0;

		public string CardString
		{
			get
			{
				var cards = Deck.ArenaReward?.Cards.Where(x => !string.IsNullOrEmpty(x?.CardId)).ToList();
				return cards != null && cards.Any()
						   ? cards.Select(x => (Database.GetCardFromId(x?.CardId)?.LocalizedName) + (x?.Golden == true ? " (golden)" : ""))
								  .Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int Duration => Deck.DeckStats.Games.Sum(x => x.SortableDuration);

		public string DurationString => Duration + " min";

		public string Region => Deck.DeckStats.Games.Any() ? Deck.DeckStats.Games.First().Region.ToString() : "UNKNOWN";

		public IEnumerable<GameStats> Games => Deck.DeckStats.Games;

		public override bool Equals(object obj) => Deck.Equals((obj as ArenaRun)?.Deck);

		public override int GetHashCode() => Deck.GetHashCode();
	}
}
