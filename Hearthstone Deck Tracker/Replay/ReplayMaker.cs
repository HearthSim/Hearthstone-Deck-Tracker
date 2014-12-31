
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Hearthstone_Deck_Tracker.Hearthstone;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Replay
{
	public static class ReplayMaker
	{
		public static T DeepClone<T>(T obj)
		{
			using (var ms = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;

				return (T)formatter.Deserialize(ms);
			}
		}

		private static readonly List<ReplayKeyPoint> Points = new List<ReplayKeyPoint>();

		public static void Reset()
		{
			Points.Clear();
		}

		public static void Generate(KeyPointType type, int id)
		{
			Points.Add(new ReplayKeyPoint(Game.Entities.Values.ToArray(), type, id));
		}

		public static void SaveToDisk()
		{
			if(Points.Any())
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
								e2.CardId = entity.CardId;
						}
					}
				}
			}


			var path = Helper.GetValidFilePath("Replays", "replay", ".hdtreplay");
			using (var ms = new MemoryStream())
			{
				using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
				{
					var json = archive.CreateEntry("replay.json");

					using (var stream = json.Open())
					using (var sw = new StreamWriter(stream))
						sw.Write(JsonConvert.SerializeObject(Points));
				}

				using (var fileStream = new FileStream(path, FileMode.Create))
				{
					ms.Seek(0, SeekOrigin.Begin);
					ms.CopyTo(fileStream);
				}
			}

			
		}


	}
}
