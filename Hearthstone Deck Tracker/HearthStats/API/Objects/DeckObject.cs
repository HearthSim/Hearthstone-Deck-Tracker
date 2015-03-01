#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class DeckObject
	{
		private const string noteUrlRegex = @"\[(HDT-)?source=(?<url>(.*?))\]";
		private const string noteArchived = "[HDT-archived]";
		public int deck_version_id;
		public int id;
		public int klass_id;
		public string name;
		public string notes;
		public string[] tags;

		public Deck ToDeck(CardObject[] cards, DeckVersion[] versions, string version)
		{
			try
			{
				var url = "";
				bool archived = false;
				if(!string.IsNullOrEmpty(notes))
				{
					var match = Regex.Match(notes, noteUrlRegex);
					if(match.Success)
					{
						url = match.Groups["url"].Value;
						notes = Regex.Replace(notes, noteUrlRegex, "");
					}

					if(notes.Contains(noteArchived))
					{
						archived = true;
						notes = notes.Replace(noteArchived, "");
					}
				}

				notes = notes.Trim();

				var deck = new Deck(name ?? "", Dictionaries.HeroDict[klass_id],
				                    cards == null
					                    ? new List<Card>()
					                    : cards.Where(x => x != null && x.count != null && x.id != null)
					                           .Select(x => x.ToCard())
					                           .Where(x => x != null)
					                           .ToList(), tags ?? new string[0], notes ?? "", url, DateTime.Now, archived, new List<Card>(),
				                    SerializableVersion.ParseOrDefault(version), new List<Deck>(), true, id.ToString(), Guid.NewGuid(),
				                    deck_version_id.ToString());
				if(versions.Length > 0)
					deck.Versions = versions.Where(v => v.version != version).Select(v => v.ToDeck(deck)).ToList();
				var current = versions.FirstOrDefault(v => v.version == version);
				if(current != null)
					deck.HearthStatsDeckVersionId = current.deck_version_id.ToString();
				deck.HearthStatsIdsAlreadyReset = true;
				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine("error converting DeckObject: " + e, "HearthStatsAPI");
				return null;
			}
		}
	}
}