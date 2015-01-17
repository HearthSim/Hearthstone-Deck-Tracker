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

		public static void Read(string fileName)
		{
			var path = Path.Combine(Config.Instance.ReplayDir, fileName);
			if(!File.Exists(path))
				return;
			const string jsonFile = "replay.json";
			string json;

			using(var fs = new FileStream(path, FileMode.Open))
			using(var archive = new ZipArchive(fs, ZipArchiveMode.Read))
			using(var sr = new StreamReader(archive.GetEntry(jsonFile).Open()))
				json = sr.ReadToEnd();

			var replay = (List<ReplayKeyPoint>)JsonConvert.DeserializeObject(json, typeof(List<ReplayKeyPoint>));


			var rv = new ReplayViewer();
			rv.Show();
			rv.Load(replay);
			Viewers.Add(rv);
		}

		public static void CloseViewers()
		{
			foreach(var viewer in Viewers.Where(viewer => viewer != null))
				viewer.Close();
		}
	}
}