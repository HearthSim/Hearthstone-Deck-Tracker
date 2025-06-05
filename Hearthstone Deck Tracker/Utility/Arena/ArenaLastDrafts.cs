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
			bool overlayVisible,
			string[] pickedCards,
			long deckId,
			bool isUnderground,
			string[]? pickedPackage,
			Dictionary<string, string[]>? packages,
			bool save = true
		)
		{
			var playerId = await GetPlayerId();
			if(playerId == null)
			{
				Log.Info("Unable to save the game. User account can not found...");
				return;
			}

			var currentDraft = GetOrCreateDraft(startTime, playerId, deckId, isUnderground);

			var start = DateTime.Parse(startTime);
			var end = DateTime.Parse(pickedTime);
			var timeSpent = end - start;

			currentDraft.Picks.Add(
					new PickItem(picked, choices, slot, (int)timeSpent.TotalMilliseconds, overlayVisible, pickedCards, pickedPackage, packages)
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

		private DraftItem GetOrCreateDraft(string startTime, string player, long deckId, bool isUnderground)
		{
			var draft = Drafts.FirstOrDefault(d => d.DeckId == deckId && d.IsUnderground == isUnderground);
			if(draft != null)
			{
				return draft;
			}

			draft = new DraftItem(startTime, player, deckId, isUnderground);
			RemoveDraft(player, isUnderground, false);
			Drafts.Add(draft);
			return draft;
		}

		public class DraftItem
		{
			public DraftItem(string startTime, string player, long deckId, bool isUnderground)
			{
				Player = player;
				StartTime = startTime;
				DeckId = deckId;
				IsUnderground = isUnderground;
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

			[XmlElement("Pick")]
			public List<PickItem> Picks { get; set; } = new();

		}

		public class PickItem
		{

			public PickItem(
				string picked,
				string[] choices,
				int slot,
				int timeOnChoice,
				bool overlayVisible,
				string[] pickedCards,
				string[]? pickedPackage,
				Dictionary<string, string[]>? packages)
			{
				Picked = picked;
				Choices = choices;
				Slot = slot;
				TimeOnChoice = timeOnChoice;
				OverlayVisible = overlayVisible;
				PickedCards = pickedCards;
				PickedPackage = pickedPackage;
				Packages = packages?.Select(p =>
					new CardPackage { KeyCard = p.Key, Cards = p.Value }
				).ToList();
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

	}
}


