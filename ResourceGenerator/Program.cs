using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Resources;
using HearthDb;
using HearthDb.Enums;
using nQuant;

namespace ResourceGenerator
{
	internal class Program
	{
		private static WebClient _webClient;
		private static WebClient WebClient => _webClient ?? (_webClient = new WebClient());
		private static void Main(string[] args)
		{
			var dir = args[0];
			var tilesDir = Path.Combine(dir, args[1]);
			var genDir = Path.Combine(dir, "Generated", args[1]);
			var dict = new Dictionary<string, List<Card>>();

			Directory.CreateDirectory(genDir);

			foreach(var card in Cards.All)
			{
				if(card.Value.Set == CardSet.CHEAT)
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
						{
							DownloadTile(card, img);
							img = new FileInfo($"{tilesDir}\\{card.Id}.png");
						}
						if(!img.Exists)
							continue;
						rw.AddResource(card.Id, new Bitmap(img.FullName));
					}
				}
			}
		}

		private static void DownloadTile(Card card, FileInfo img)
		{
			Console.WriteLine($"Downloading missing image data for {card.Name} ({card.Id})");
			try
			{
				var data = WebClient.DownloadData($"https://art.hearthstonejson.com/v1/tiles/{card.Id}.png");
				Bitmap src;
				using(var ms = new MemoryStream(data))
					src = new Bitmap(new Bitmap(ms), 148, 34);
				var crop = new Rectangle(0, 0, 130, 34);
				var target = new Bitmap(crop.Width, crop.Height);
				using(var g = Graphics.FromImage(target))
					g.DrawImage(src, crop, new Rectangle(src.Width - crop.Width - 4, 0, crop.Width, src.Height), GraphicsUnit.Pixel);

				var quantizer = new WuQuantizer();
				using(var quantized = quantizer.QuantizeImage(target, 0, 0))
					quantized.Save(img.FullName, ImageFormat.Png);
			}
			catch(WebException ex)
			{
				Console.WriteLine("Error! " + ex.Message);
			}
		}
	}
}
