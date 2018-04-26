using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using HearthDb;
using HearthDb.Enums;

namespace ResourceGenerator
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			var dir = args[0];
			var tilesDir = Path.Combine(dir, args[1]);
			var genDir = Path.Combine(dir, "Generated", args[1]);
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
