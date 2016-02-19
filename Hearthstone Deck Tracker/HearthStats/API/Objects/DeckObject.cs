#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class DeckObject
	{
		private const string noteUrlRegex = @"\[(HDT-)?source=(?<url>(.*?))\]";
		private const string noteArchived = "[HDT-archived]";
		public int deck_version_id;
		public int id;
		public int? klass_id;
		public string name;
		public string notes;
		public DateTime updated_at;

		public Deck ToDeck(CardObject[] cards, string[] rawTags, DeckVersion[] versions, string version)
		{
			if(!klass_id.HasValue)
				return null;
			try
			{
				var url = "";
				var archived = false;
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
					notes = notes.Trim();
				}


				//tags are returned all lowercase, find matching tag
				var tags =
					rawTags.Select(
					               tag =>
					               DeckList.Instance.AllTags.FirstOrDefault(t => string.Equals(t, tag, StringComparison.InvariantCultureIgnoreCase))
					               ?? tag);
				var deck = new Deck(name ?? "", Dictionaries.HeroDict[klass_id.Value],
				                    cards?.Where(x => x?.count != null && x.id != null)
										  .Select(x => x.ToCard())
										  .Where(x => x != null)
										  .ToList() ?? new List<Card>(), tags, notes ?? "", url, DateTime.Now, archived, new List<Card>(),
				                    SerializableVersion.ParseOrDefault(version), new List<Deck>(), true, id.ToString(), Guid.NewGuid(),
				                    deck_version_id.ToString());
				deck.LastEdited = updated_at.ToLocalTime();
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
				Log.Error("(HearthStatsAPI) error converting DeckObject: " + e);
				return null;
			}
		}
	}
}