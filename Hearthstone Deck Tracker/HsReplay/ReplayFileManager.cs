#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay.Converter;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class ReplayFileManager
	{
		private const string HdtReplayFile = "replay.json";
		private const string RawLogFile = "output_log.txt";
		private const string HsReplayFile = "hsreplay.xml";
		private readonly GameStats _game;

		public ReplayFileManager(GameStats game)
		{
			_game = game;
			if(!ReplayExists)
				return;
			if(_game.StartTime == DateTime.MinValue && File.Exists(game.ReplayFile))
			{
				var fi = new FileInfo(game.ReplayFile);
				_game.StartTime = fi.LastWriteTime;
			}
			using(var fs = new FileStream(FilePath, FileMode.Open))
			using(var archive = new ZipArchive(fs, ZipArchiveMode.Read))
			{
				HasHdtReplayFile = archive.Entries.Any(x => x.Name == HdtReplayFile);
				if(archive.Entries.Any(x => x.Name == HsReplayFile))
				{
					using(var sr = new StreamReader(archive.GetEntry(HsReplayFile).Open()))
						HsReplay = sr.ReadToEnd();
				}
				if(archive.Entries.Any(x => x.Name == RawLogFile))
				{
					using(var sr = new StreamReader(archive.GetEntry(RawLogFile).Open()))
						RawLog = sr.ReadToEnd();
				}
			}
		}

		public ReplayFileManager(string filePath) : this(new GameStats() { ReplayFile = filePath})
		{
		}

		public string FilePath => _game.ReplayFile != null ? Path.Combine(Config.Instance.ReplayDir, _game.ReplayFile) : null;
		public bool ReplayExists => _game.ReplayFile != null && File.Exists(FilePath);
		public bool HasHdtReplayFile { get; }
		public bool HasRawLogFile => !string.IsNullOrEmpty(RawLog);
		public bool HasHsReplayFile => !string.IsNullOrEmpty(HsReplay);
		public string RawLog { get; }
		public string HsReplay { get; private set; }

		public void StoreHsReplay(string xml)
		{
			if(HasHsReplayFile)
				return;
			Log.Info($"Adding {HsReplayFile} to hdtrelay file.");
			try
			{
				using(var fs = new FileStream(FilePath, FileMode.Open))
				using(var archive = new ZipArchive(fs, ZipArchiveMode.Update))
				using(var sw = new StreamWriter(archive.CreateEntry(HsReplayFile).Open(), Encoding.UTF8))
					sw.Write(xml);
				HsReplay = xml;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public async Task<bool> ConvertAndStoreHsReplay()
		{
			if(!HasRawLogFile)
				return false;
			var log = RawLog.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
			var xml = await HsReplayConverter.Convert(log, _game.StartTime == DateTime.MinValue ? null :_game, null);
			if(string.IsNullOrEmpty(xml))
				return false;
			StoreHsReplay(xml);
			return true;
		}
	}
}