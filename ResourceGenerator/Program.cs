using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using HearthDb;
using HearthDb.Enums;
using HearthMirror;
using System.Linq;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.Utility.RemoteConfig.ConfigData;

namespace ResourceGenerator
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			switch(args[0])
			{
				case "tiles":
					GenerateTiles(args);
					break;
				case "whizbang":
					GenerateWhizbangDecks(args);
					break;
			}
		}

		private static void GenerateWhizbangDecks(string[] args)
		{
			var file = args[1];
			Console.WriteLine("Ensure Hearthstone is running. Press any key to continue...");
			Console.ReadKey();
			Console.WriteLine("Reading decks from memory...");
			Console.WriteLine("(this make take a while)");
			var templateDecks = Reflection.GetTemplateDecks();
			var whizbangDecks = templateDecks.Where(d => d.SortOrder < 2)
				.Select(d => new WhizbangDeck
				{
					Title = d.Title,
					Class = (CardClass)d.Class,
					DeckId = d.DeckId,
					Cards = d.Cards.GroupBy(c => c).Select(x => new RemoteConfigCard{ DbfId = x.Key, Count = x.Count() }).ToList(),
				});
			using(var sw = new StreamWriter(file))
				sw.WriteLine(JsonConvert.SerializeObject(whizbangDecks, Formatting.Indented));
			Console.WriteLine("Saved to " + file);
			Console.ReadKey();
		}

		private static void GenerateTiles(string[] args)
		{
			var dir = args[1];
			var tilesDir = Path.Combine(dir, args[2]);
			var genDir = Path.Combine(dir, "Generated", args[2]);
			var dict = new Dictionary<string, List<Card>>();

			Directory.CreateDirectory(genDir);

			foreach(var card in Cards.All)
			{
				if(card.Value.Set == CardSet.CHEAT || card.Value.Set == CardSet.SLUSH)
					continue;
				var key = card.Value.Set + (card.Value.Collectible ? "" : "_NC");
				if(!dict.ContainsKey(key))
					dict[key] = new List<Card>();
				dict[key].Add(card.Value);
			}

			foreach(var set in dict.Keys)
			{
				var file = $"{genDir}\\{set}.res";
				Console.WriteLine($"Generating {file}...");
				using(var rw = new ResourceWriter(file))
				{
					foreach(var card in dict[set])
					{
						var img = new FileInfo($"{tilesDir}\\{card.Id}.png");
						if(!img.Exists && (card.Collectible || card.Type == CardType.MINION || card.Type == CardType.SPELL || card.Type == CardType.WEAPON))
							Console.WriteLine($"missing {card.Id}");
						if(!img.Exists)
							continue;
						rw.AddResource(card.Id, new Bitmap(img.FullName));
					}
				}
			}
		}
	}
}
