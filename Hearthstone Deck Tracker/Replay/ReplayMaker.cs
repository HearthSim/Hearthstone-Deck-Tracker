
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Documents;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Replay
{
	public static class ReplayMaker
	{
		private static readonly List<ReplayKeyPoint> Points = new List<ReplayKeyPoint>();

		public static void Reset()
		{
			Points.Clear();
		}

		public static void Generate(KeyPointType type, int id, ActivePlayer player)
		{
			Points.Add(new ReplayKeyPoint(Game.Entities.Values.ToArray(), type, id, player));
		}

		public static string SaveToDisk()
		{
			try
			{
				if(!Points.Any())
					return null;
				ResolveCardIds();
				RemoveObsoletePlays();

				var player = Points.Last().Data.First(x => x.IsPlayer);
				var opponent = Points.Last().Data.First(x => x.HasTag(GAME_TAG.PLAYER_ID) && !x.IsPlayer);
				var playerHero =
					Points.Last()
					      .Data.First(
					                  x =>
					                  !string.IsNullOrEmpty(x.CardId) && x.CardId.Contains("HERO") &&
					                  x.IsControlledBy(player.GetTag(GAME_TAG.CONTROLLER)));
				var opponentHero =
					Points.Last()
					      .Data.First(
					                  x =>
					                  !string.IsNullOrEmpty(x.CardId) && x.CardId.Contains("HERO") &&
					                  x.IsControlledBy(opponent.GetTag(GAME_TAG.CONTROLLER)));

				var fileName = string.Format("{0}({1}) vs {2}({3}) {4}", player.Name, CardIds.HeroIdDict[playerHero.CardId], opponent.Name,
				                             CardIds.HeroIdDict[opponentHero.CardId], DateTime.Now.ToString("hhmm-ddMMyy"));


				var path = Helper.GetValidFilePath("Replays", fileName, ".hdtreplay");
				using(var ms = new MemoryStream())
				{
					using(var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
					{
						var json = archive.CreateEntry("replay.json");

						using(var stream = json.Open())
						using(var sw = new StreamWriter(stream))
							sw.Write(JsonConvert.SerializeObject(Points));
					}

					using(var fileStream = new FileStream(path, FileMode.Create))
					{
						ms.Seek(0, SeekOrigin.Begin);
						ms.CopyTo(fileStream);
					}
				}
				return path;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString());
				return null;
			}


		}

		private static void ResolveCardIds()
		{
			var lastKeyPoint = Points.Last();
			foreach(var kp in Points)
			{
				foreach(var entity in lastKeyPoint.Data)
				{
					if(!string.IsNullOrEmpty(entity.CardId))
					{
						var e2 = kp.Data.FirstOrDefault(x => x.Id == entity.Id);
						if(e2 != null)
						{
							e2.CardId = entity.CardId;
							e2.Name = entity.Name;
						}
					}
				}
			}
		}

		private static void RemoveObsoletePlays()
		{
			var spellsWithTarget = Points.Where(x => x.Type == KeyPointType.PlaySpell).Select(x => x.Id);
			var obsoletePlays = Points.Where(x => x.Type == KeyPointType.Play && spellsWithTarget.Any(id => x.Id == id)).ToList();
			foreach(var play in obsoletePlays)
				Points.Remove(play);
		}
	}
}
