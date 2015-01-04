
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
				ResolveZonePos();
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
		private static void ResolveZonePos()
		{
			//ZONE_POSITION changes happen after draws, meaning drawn card will not appear. 

			var playerController = Points[0].Data.First(x => x.IsPlayer).GetTag(GAME_TAG.CONTROLLER);
			var handPos = new Dictionary<int, int>();
			var boardPos = new Dictionary<int, int>();
			Points.Reverse();
			foreach(var kp in Points)
			{
				if(kp.Type == KeyPointType.HandPos)
				{
					var pos = kp.Data.First(x => x.Id == kp.Id).GetTag(GAME_TAG.ZONE_POSITION);
					if(!handPos.ContainsKey(kp.Id))
						handPos.Add(kp.Id, pos);
					else
						handPos[kp.Id] = pos;
				}
				else if(kp.Type == KeyPointType.BoardPos)
				{
					var pos = kp.Data.First(x => x.Id == kp.Id).GetTag(GAME_TAG.ZONE_POSITION);
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
						kp.Data.First(x => x.Id == kp.Id).SetTag(GAME_TAG.ZONE_POSITION, zp);
						handPos.Remove(zp);
					}
				}
				else if(kp.Type == KeyPointType.Summon || kp.Type == KeyPointType.Play)
				{
					int zp;
					if(boardPos.TryGetValue(kp.Id, out zp))
					{
						kp.Data.First(x => x.Id == kp.Id).SetTag(GAME_TAG.ZONE_POSITION, zp);
						boardPos.Remove(zp);
					}
				}
			}
			var toRemove = new List<ReplayKeyPoint>(Points.Where(x => x.Type == KeyPointType.BoardPos || x.Type == KeyPointType.HandPos));
			foreach(var kp in toRemove)
				Points.Remove(kp);

			var occupiedPlayerHandZonePos = new List<int>();
			var occupiedPlayerBoardZonePos = new List<int>();
			var occupiedOpponentHandZonePos = new List<int>();
			var occupiedOpponentBoardZonePos = new List<int>();
			foreach(var kp in Points)
			{
				occupiedPlayerHandZonePos.Clear();
				occupiedPlayerBoardZonePos.Clear();
				occupiedOpponentHandZonePos.Clear();
				occupiedOpponentBoardZonePos.Clear();
				foreach(var entity in kp.Data)
				{
					if(entity.HasTag(GAME_TAG.ZONE_POSITION))
					{
						var zonePos = entity.GetTag(GAME_TAG.ZONE_POSITION);
						if(entity.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.HAND)
						{
							if(entity.GetTag(GAME_TAG.CONTROLLER) == playerController)
							{
								if(!occupiedPlayerHandZonePos.Contains(zonePos))
									occupiedPlayerHandZonePos.Add(zonePos);
								else
								{
									for(int i = 1; i <= 10; i++)
									{
										if(!occupiedPlayerHandZonePos.Contains(i))
										{
											entity.SetTag(GAME_TAG.ZONE_POSITION, i);
											occupiedPlayerHandZonePos.Add(i);
											break;
										}
									}
								}
							}
							else
							{
								if(!occupiedOpponentHandZonePos.Contains(zonePos))
									occupiedOpponentHandZonePos.Add(zonePos);
								else
								{
									for(int i = 1; i <= 10; i++)
									{
										if(!occupiedOpponentHandZonePos.Contains(i))
										{
											entity.SetTag(GAME_TAG.ZONE_POSITION, i);
											occupiedOpponentHandZonePos.Add(i);
											break;
										}
									}
								}
							}

						}
						else if(entity.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.PLAY)
						{
							if(entity.GetTag(GAME_TAG.CONTROLLER) == playerController)
							{
								if(!occupiedPlayerBoardZonePos.Contains(zonePos))
									occupiedPlayerBoardZonePos.Add(zonePos);
								else
								{
									for(int i = 1; i <= 10; i++)
									{
										if(!occupiedPlayerBoardZonePos.Contains(i))
										{
											entity.SetTag(GAME_TAG.ZONE_POSITION, i);
											occupiedPlayerBoardZonePos.Add(i);
											break;
										}
									}
								}
							}
							else
							{
								if(!occupiedOpponentBoardZonePos.Contains(zonePos))
									occupiedOpponentBoardZonePos.Add(zonePos);
								else
								{
									for(int i = 1; i <= 10; i++)
									{
										if(!occupiedOpponentBoardZonePos.Contains(i))
										{
											entity.SetTag(GAME_TAG.ZONE_POSITION, i);
											occupiedOpponentBoardZonePos.Add(i);
											break;
										}
									}
								}
							}
						}
					}
				}
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
