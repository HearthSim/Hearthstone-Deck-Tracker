#region

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Replay
{
	public static class ReplayReader
	{
		private static readonly List<ReplayViewer> Viewers = new List<ReplayViewer>();

		public static void LaunchReplayViewer(string fileName)
		{
			var replay = LoadReplay(fileName);
			var rv = new ReplayViewer();
			rv.Show();
			rv.Load(replay);
			Viewers.Add(rv);
		}

		public static List<ReplayKeyPoint> LoadReplay(string fileName)
		{
			var path = Path.Combine(Config.Instance.ReplayDir, fileName);
			if(!File.Exists(path))
				return new List<ReplayKeyPoint>();
			const string jsonFile = "replay.json";
			string json;

			using(var fs = new FileStream(path, FileMode.Open))
			using(var archive = new ZipArchive(fs, ZipArchiveMode.Read))
			using(var sr = new StreamReader(archive.GetEntry(jsonFile).Open()))
				json = sr.ReadToEnd();
			json = json.Replace("EQUIPPED_WEAPON", "WEAPON"); //legacy enum name

			return (List<ReplayKeyPoint>)JsonConvert.DeserializeObject(json, typeof(List<ReplayKeyPoint>));
		}

		public static void CloseViewers()
		{
			foreach(var viewer in Viewers.Where(viewer => viewer != null))
				viewer.Close();
		}
	}
}