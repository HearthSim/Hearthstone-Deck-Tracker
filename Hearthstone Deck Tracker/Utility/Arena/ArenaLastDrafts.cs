using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Arena
{
	[XmlRoot("ArenaLastDrafts")]
	public sealed class ArenaLastDrafts
	{
		private static readonly Lazy<ArenaLastDrafts> LazyInstance = new Lazy<ArenaLastDrafts>(Load);

		public static ArenaLastDrafts Instance = LazyInstance.Value;

		[XmlElement("Draft")]
		public List<DraftItem> Drafts { get; set; } = new();

		private static string DataPath => Path.Combine(Config.AppDataPath, "ArenaLastDrafts.xml");

		private static ArenaLastDrafts Load()
		{
			if(!File.Exists(DataPath))
				return new ArenaLastDrafts();
			try
			{
				return XmlManager<ArenaLastDrafts>.Load(DataPath);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return new ArenaLastDrafts();
		}

		private async Task<string?> GetPlayerId()
		{
			var accountId = await Helper.RetryWhileNull(Reflection.Client.GetAccountId, 2, 3000);
			return accountId != null ? $"{accountId.Hi}_{accountId.Lo}" : null;
		}

		public async Task<List<DraftItem>> PlayerDrafts()
		{
			var playerId = await GetPlayerId();
			if (playerId == null)
				return new List<DraftItem>();

			return Drafts.Where(draft => draft.Player == null || draft.Player == playerId).ToList();
		}

		public async void AddPick(
			string startTime,
			string pickedTime,
			string picked,
			string[] choices,
			int slot,
			string[] pickedCards,
			long deckId,
			bool isUnderground,
			string[]? pickedPackage,
			Dictionary<string, string[]>? packages,
			bool overlayVisible,
			bool isOverlayEnabled,
			bool isArenasmithAvailable,
			bool isTrialsActivated,
			Dictionary<string, float>? arenasmithScores,
			bool save = true
		)
		{
			var playerId = await GetPlayerId();
			if(playerId == null)
			{
				Log.Info("Unable to save the game. User account can not found...");
				return;
			}

			var currentDraft = GetOrCreateDraft(startTime, playerId, deckId, isUnderground, isTrialsActivated);

			var start = DateTime.Parse(startTime);
			var end = DateTime.Parse(pickedTime);
			var timeSpent = end - start;

			currentDraft.Picks.Add(
					new PickItem(
						picked,
						choices,
						slot,
						(int)timeSpent.TotalMilliseconds,
						pickedCards,
						pickedPackage,
						packages,
						overlayVisible,
						isOverlayEnabled,
						isArenasmithAvailable,
						arenasmithScores
					)
				);

			if(save)
				Save();
		}

		public async void AddRedraftPick(
			string startTime,
			string pickedTime,
			string picked,
			string[] choices,
			int slot,
			string[] originalDeck,
			string[] redraftPickedCards,
			long originalDeckId,
			long redraftDeckId,
			int losses,
			bool isUnderground,
			bool overlayVisible,
			bool isOverlayEnabled,
			bool isArenasmithAvailable,
			bool isTrialsActivated,
			Dictionary<string, float>? arenasmithScores,
			bool save = true
			)
		{
			var playerId = await GetPlayerId();
			if(playerId == null)
			{
				Log.Info("Unable to save the game. User account can not found...");
				return;
			}

			var currentDraft = GetOrCreateDraft(startTime, playerId, originalDeckId, isUnderground, isTrialsActivated);
			var currentRedraft = GetOrCreateRedraft(currentDraft, startTime, playerId, originalDeckId, redraftDeckId, losses, originalDeck, isUnderground);

			var start = DateTime.Parse(startTime);
			var end = DateTime.Parse(pickedTime);
			var timeSpent = end - start;

			currentRedraft.Picks.Add(
				new RedraftPickItem(
					picked,
					choices,
					slot,
					(int)timeSpent.TotalMilliseconds,
					redraftPickedCards,
					overlayVisible,
					isOverlayEnabled,
					isArenasmithAvailable,
					arenasmithScores
				)
			);

			if(save)
				Save();
		}

		public void RemoveDraft(string player, bool isUnderground, bool save = true)
		{
			// the same player can't have 2 drafts of same type open at same time
			var existingEntry = Drafts.FirstOrDefault(
					x => x.Player != null && x.Player.Equals(player) && x.IsUnderground == isUnderground
				);
			if (existingEntry != null)
				Drafts.Remove(existingEntry);
			if(save)
				Save();
		}

		public static void Save()
		{
			try
			{
				XmlManager<ArenaLastDrafts>.Save(DataPath, Instance);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public void Reset()
		{
			Drafts.Clear();
			Save();
		}

		private DraftItem GetOrCreateDraft(string startTime, string player, long deckId, bool isUnderground, bool isTrialsActivated)
		{
			var draft = Drafts.FirstOrDefault(d => d.DeckId == deckId && d.IsUnderground == isUnderground);
			if(draft != null)
			{
				return draft;
			}

			draft = new DraftItem(startTime, player, deckId, isUnderground, isTrialsActivated);
			RemoveDraft(player, isUnderground, false);
			Drafts.Add(draft);
			return draft;
		}

		private RedraftItem GetOrCreateRedraft(DraftItem currentDraft, string startTime, string player, long originalDeckId, long redraftDeckId, int losses, string[] originalDeck, bool isUnderground)
		{
			var redraft = currentDraft.Redrafts.FirstOrDefault(r => r.RedraftDeckId == redraftDeckId && r.Losses == losses);
			if(redraft != null)
			{
				return redraft;
			}

			redraft = new RedraftItem(startTime, player, originalDeckId, redraftDeckId, losses, originalDeck, isUnderground);
			currentDraft.Redrafts.Add(redraft);
			return redraft;
		}

		public class DraftItem
		{
			public DraftItem(string startTime, string player, long deckId, bool isUnderground, bool isTrialsActivated = false)
			{
				Player = player;
				StartTime = startTime;
				DeckId = deckId;
				IsUnderground = isUnderground;
				IsTrialsActivated = isTrialsActivated;
			}

			public DraftItem()
			{
			}

			[XmlAttribute("Player")]
			public string? Player { get; set; }

			[XmlAttribute("StartTime")]
			public string? StartTime { get; set; }

			[XmlAttribute("DeckId")]
			public long DeckId { get; set; }

			[XmlAttribute("IsUnderground")]
			public bool IsUnderground { get; set; }

			[XmlAttribute("IsTrialsActivated")]
			public bool IsTrialsActivated { get; set; }

			[XmlElement("Pick")]
			public List<PickItem> Picks { get; set; } = new();

			[XmlElement("Redraft")]
			public List<RedraftItem> Redrafts { get; set; } = new();

		}

		public class PickItem
		{

			public PickItem(
				string picked,
				string[] choices,
				int slot,
				int timeOnChoice,
				string[] pickedCards,
				string[]? pickedPackage,
				Dictionary<string, string[]>? packages,
				bool overlayVisible,
				bool isOverlayEnabled,
				bool isArenasmithAvailable,
				Dictionary<string, float>? arenasmithScores)
			{
				Picked = picked;
				Choices = choices;
				Slot = slot;
				TimeOnChoice = timeOnChoice;
				PickedCards = pickedCards;
				PickedPackage = pickedPackage;
				Packages = packages?.Select(p =>
					new CardPackage { KeyCard = p.Key, Cards = p.Value }
				).ToList();
				OverlayVisible = overlayVisible;
				OverlayEnabled = isOverlayEnabled;
				ArenasmithAvailable = isArenasmithAvailable;
				ArenasmithScores = arenasmithScores?.Select(s =>
					new ArenasmithScore {Card = s.Key, Score = s.Value}).ToList();
			}

			public PickItem() { }

			[XmlElement("Slot")]
			public int Slot { get; set; }

			[XmlElement("Picked")]
			public string? Picked { get; set; }

			[XmlElement("Choice")]
			public string[] Choices { get; set; } = { };

			[XmlElement("TimeOnChoice")]
			public int TimeOnChoice { get; set; }

			[XmlElement("OverlayVisible")]
			public bool OverlayVisible { get; set; }

			[XmlElement("OverlayEnabled")]
			public bool OverlayEnabled { get; set; }

			[XmlElement("ArenasmithAvailable")]
			public bool ArenasmithAvailable { get; set; }

			[XmlArray("ArenasmithScores")]
			[XmlArrayItem("ArenasmithScore")]
			public List<ArenasmithScore>? ArenasmithScores { get; set; }

			[XmlElement("PickedCards")]
			public string[] PickedCards { get; set; } = { };

			[XmlElement("PickedPackage")]
			public string[]? PickedPackage { get; set; }

			[XmlArray("Packages")]
			[XmlArrayItem("Package")]
			public List<CardPackage>? Packages { get; set; }
		}

		public class CardPackage
		{
			[XmlAttribute("KeyCard")]
			public string? KeyCard { get; set; }

			[XmlElement("Card")]
			public string[] Cards { get; set; } = { };
		}

		public class ArenasmithScore
		{
			[XmlAttribute("Card")]
			public string Card { get; set; } = "";

			[XmlAttribute("Score")]
			public float Score { get; set; }
		}

		public class RedraftItem
		{
			public RedraftItem(string startTime, string player, long originalDeckId, long redraftDeckId, int losses, string[] originalDeck, bool isUnderground)
			{
				Player = player;
				StartTime = startTime;
				OriginalDeckId = originalDeckId;
				RedraftDeckId = redraftDeckId;
				Losses = losses;
				OriginalDeck = originalDeck;
				IsUnderground = isUnderground;
			}

			public RedraftItem()
			{
			}

			[XmlAttribute("Player")]
			public string? Player { get; set; }

			[XmlAttribute("StartTime")]
			public string? StartTime { get; set; }

			[XmlAttribute("OriginalDeckId")]
			public long OriginalDeckId { get; set; }

			[XmlAttribute("RedraftDeckId")]
			public long RedraftDeckId { get; set; }

			[XmlAttribute("Losses")]
			public int Losses { get; set; }

			[XmlAttribute("IsUnderground")]
			public bool IsUnderground { get; set; }

			[XmlElement("OriginalDeck")]
			public string[] OriginalDeck { get; set; } = { };

			[XmlElement("Pick")]
			public List<RedraftPickItem> Picks { get; set; } = new();

		}

		public class RedraftPickItem
		{
			public RedraftPickItem(
				string picked,
				string[] choices,
				int slot,
				int timeOnChoice,
				string[] redraftPickedCards,
				bool overlayVisible,
				bool isOverlayEnabled,
				bool isArenasmithAvailable,
				Dictionary<string, float>? arenasmithScores
			)
			{
				Picked = picked;
				Choices = choices;
				Slot = slot;
				TimeOnChoice = timeOnChoice;
				RedraftPickedCards = redraftPickedCards;
				OverlayVisible = overlayVisible;
				OverlayEnabled = isOverlayEnabled;
				ArenasmithAvailable = isArenasmithAvailable;
				ArenasmithScores = arenasmithScores?.Select(s =>
					new ArenasmithScore {Card = s.Key, Score = s.Value}).ToList();
			}

			public RedraftPickItem() { }

			[XmlElement("Slot")]
			public int Slot { get; set; }

			[XmlElement("Picked")]
			public string? Picked { get; set; }

			[XmlElement("Choice")]
			public string[] Choices { get; set; } = { };

			[XmlElement("TimeOnChoice")]
			public int TimeOnChoice { get; set; }

			[XmlElement("OverlayVisible")]
			public bool OverlayVisible { get; set; }

			[XmlElement("OverlayEnabled")]
			public bool OverlayEnabled { get; set; }

			[XmlElement("ArenasmithAvailable")]
			public bool ArenasmithAvailable { get; set; }

			[XmlArray("ArenasmithScores")]
			[XmlArrayItem("ArenasmithScore")]
			public List<ArenasmithScore>? ArenasmithScores { get; set; }

			[XmlElement("RedraftPickedCards")]
			public string[] RedraftPickedCards { get; set; } = { };
		}

	}
}


