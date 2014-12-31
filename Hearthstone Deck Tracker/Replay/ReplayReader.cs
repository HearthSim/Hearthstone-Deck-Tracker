using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Replay
{
	public static class ReplayReader
	{
		public static void Read()
		{
			const string jsonFile = "replay.json";
			const string file = "Replays\\replay.hdtreplay";
			string json;

			using(var fs = new FileStream(file,FileMode.Open))
			using(var archive = new ZipArchive(fs,ZipArchiveMode.Read))
			using(var sr = new StreamReader(archive.GetEntry(jsonFile).Open()))
					json = sr.ReadToEnd();

			var replay = (List<ReplayKeyPoint>)JsonConvert.DeserializeObject(json, typeof(List<ReplayKeyPoint>));

			
			var rv = new ReplayViewer();
			rv.Show();
			rv.Load(replay);

		}
	}
}
