#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Replay
{
	public static class ReplayMaker
	{
		private static readonly List<ReplayKeyPoint> Points = new List<ReplayKeyPoint>();

		public static void Reset() => Points.Clear();

		public static void Generate(KeyPointType type, int id, ActivePlayer player, IGame game) => Points.Add(new ReplayKeyPoint(game.Entities.Values.ToArray(), type, id, player));

		public static string SaveToDisk(List<string> powerLog)
		{
			try
			{
				if(!Points.Any())
					return null;
				ResolveZonePos();
				ResolveCardIds();
				RemoveObsoletePlays();

				var player = Points.Last().Data.First(x => x.IsPlayer);
				var opponent = Points.Last().Data.First(x => x.HasTag(GameTag.PLAYER_ID) && !x.IsPlayer);
				var playerHero =
					Points.Last()
					      .Data.First(
					                  x =>
					                  x.GetTag(GameTag.CARDTYPE) == (int)CardType.HERO && x.IsControlledBy(player.GetTag(GameTag.CONTROLLER)));
				var opponentHero =
					Points.Last()
					      .Data.FirstOrDefault(
					                           x =>
					                           x.GetTag(GameTag.CARDTYPE) == (int)CardType.HERO
					                           && x.IsControlledBy(opponent.GetTag(GameTag.CONTROLLER)));
				if(opponentHero == null)
				{
					//adventure bosses
					opponentHero =
						Points.Last()
						      .Data.First(
						                  x =>
						                  !string.IsNullOrEmpty(x.CardId)
						                  && ((x.CardId.StartsWith("NAX") && x.CardId.Contains("_01")) || x.CardId.StartsWith("BRMA"))
						                  && Database.GetHeroNameFromId(x.CardId) != null);

					ResolveOpponentName(Database.GetHeroNameFromId(opponentHero.CardId));
				}

				var fileName =
					$"{player.Name}({Database.GetHeroNameFromId(playerHero.CardId)}) vs {opponent.Name}({Database.GetHeroNameFromId(opponentHero.CardId)}) {DateTime.Now.ToString("HHmm-ddMMyy")}";


				if(!Directory.Exists(Config.Instance.ReplayDir))
					Directory.CreateDirectory(Config.Instance.ReplayDir);
				var path = Helper.GetValidFilePath(Config.Instance.ReplayDir, fileName, ".hdtreplay");
				using(var ms = new MemoryStream())
				{
					using(var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
					{
						var json = archive.CreateEntry("replay.json");
						using(var stream = json.Open())
						using(var sw = new StreamWriter(stream))
							sw.Write(JsonConvert.SerializeObject(Points));

						var hsLog = archive.CreateEntry("output_log.txt");
						using(var logStream = hsLog.Open())
						using(var swLog = new StreamWriter(logStream))
							powerLog?.ForEach(swLog.WriteLine);
					}

					using(var fileStream = new FileStream(path, FileMode.Create))
					{
						ms.Seek(0, SeekOrigin.Begin);
						ms.CopyTo(fileStream);
					}
				}
				return fileName + ".hdtreplay";
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		private static void ResolveOpponentName(string opponentName)
		{
			if(opponentName == null)
				return;
			foreach(var kp in Points)
			{
				var opponent = kp.Data.FirstOrDefault(x => x.HasTag(GameTag.PLAYER_ID) && !x.IsPlayer);
				if(opponent != null)
					opponent.Name = opponentName;
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

		private static void ResolveZonePos()
		{
			//ZONE_POSITION changes happen after draws, meaning drawn card will not appear. 
			var handPos = new Dictionary<int, int>();
			var boardPos = new Dictionary<int, int>();
			Points.Reverse();
			foreach(var kp in Points)
			{
				if(kp.Type == KeyPointType.HandPos)
				{
					var pos = kp.Data.First(x => x.Id == kp.Id).GetTag(GameTag.ZONE_POSITION);
					if(!handPos.ContainsKey(kp.Id))
						handPos.Add(kp.Id, pos);
					else
						handPos[kp.Id] = pos;
				}
				else if(kp.Type == KeyPointType.BoardPos)
				{
					var pos = kp.Data.First(x => x.Id == kp.Id).GetTag(GameTag.ZONE_POSITION);
					if(!boardPos.ContainsKey(kp.Id))
						boardPos.Add(kp.Id, pos);
					else
						boardPos[kp.Id] = pos;
				}
				else if(kp.Type == KeyPointType.Draw || kp.Type == KeyPointType.Obtain)
				{
					int zp;
					if(handPos.TryGetValue(kp.Id, out zp))
					{
						kp.Data.First(x => x.Id == kp.Id).SetTag(GameTag.ZONE_POSITION, zp);
						handPos.Remove(zp);
					}
				}
				else if(kp.Type == KeyPointType.Summon || kp.Type == KeyPointType.Play)
				{
					int zp;
					if(boardPos.TryGetValue(kp.Id, out zp))
					{
						kp.Data.First(x => x.Id == kp.Id).SetTag(GameTag.ZONE_POSITION, zp);
						boardPos.Remove(zp);
					}
				}
			}
			var toRemove = new List<ReplayKeyPoint>(Points.Where(x => x.Type == KeyPointType.BoardPos || x.Type == KeyPointType.HandPos));
			foreach(var kp in toRemove)
				Points.Remove(kp);


			//this one is still needed for hand zonepos I think...
			var occupiedZonePos = new List<int>();
			var noUniqueZonePos = new List<Entity>();
			foreach(var kp in Points)
			{
				var currentEntity = kp.Data.FirstOrDefault(x => x.Id == kp.Id);
				if(currentEntity == null || !currentEntity.HasTag(GameTag.ZONE_POSITION))
					continue;

				occupiedZonePos.Clear();
				noUniqueZonePos.Clear();
				noUniqueZonePos.Add(currentEntity);
				foreach(var entity in kp.Data.Where(x => x.Id != kp.Id && x.HasTag(GameTag.ZONE_POSITION)))
				{
					var zonePos = entity.GetTag(GameTag.ZONE_POSITION);
					if(entity.GetTag(GameTag.ZONE) == currentEntity.GetTag(GameTag.ZONE)
					   && entity.GetTag(GameTag.CONTROLLER) == currentEntity.GetTag(GameTag.CONTROLLER))
					{
						if(!occupiedZonePos.Contains(zonePos))
							occupiedZonePos.Add(zonePos);
						else
							noUniqueZonePos.Add(entity);
					}
				}
				foreach(var entity in noUniqueZonePos)
				{
					if(occupiedZonePos.Contains(entity.GetTag(GameTag.ZONE_POSITION)))
					{
						var targetPos = occupiedZonePos.Max() + 1;
						currentEntity.SetTag(GameTag.ZONE_POSITION, targetPos);
						occupiedZonePos.Add(targetPos);
					}
					else
						occupiedZonePos.Add(entity.GetTag(GameTag.ZONE_POSITION));
				}
			}

			var onBoard = new List<Entity>();
			foreach(var kp in Points)
			{
				var currentBoard =
					kp.Data.Where(
					              x =>
					              x.IsInZone(Zone.PLAY) && x.HasTag(GameTag.HEALTH) && !string.IsNullOrEmpty(x.CardId)
					              && !x.CardId.Contains("HERO")).ToList();
				if(onBoard.All(e => currentBoard.Any(e2 => e2.Id == e.Id)) && currentBoard.All(e => onBoard.Any(e2 => e2.Id == e.Id)))
				{
					foreach(var entity in currentBoard)
						entity.SetTag(GameTag.ZONE_POSITION, onBoard.First(e => e.Id == entity.Id).GetTag(GameTag.ZONE_POSITION));
				}
				else
					onBoard = new List<Entity>(currentBoard);
			}


			//re-reverse
			Points.Reverse();
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