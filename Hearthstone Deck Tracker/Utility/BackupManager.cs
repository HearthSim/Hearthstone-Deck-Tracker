#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class BackupManager
	{
		private const int MaxBackups = 7;
		private static readonly string[] Files = {"PlayerDecks.xml", "DeckStats.xml", "DefaultDeckStats.xml", "config.xml", "HotKeys.xml"};

		public static void Run()
		{
			Log.Info("Running BackupManager");
			if(!Directory.Exists(Config.Instance.BackupDir))
				Directory.CreateDirectory(Config.Instance.BackupDir);
			var dirInfo = new DirectoryInfo(Config.Instance.BackupDir);
			var backupFileName = $"Backup_{DateTime.Today.ToString("ddMMyyyy")}.zip";

			if (dirInfo.GetFiles().Any(x => x.Name == backupFileName))
			{
				Log.Info("Backup for today already exists");
				return;
			}

			try
			{
				var backups = dirInfo.GetFiles("Backup_*");
				while(backups.Count() > MaxBackups)
				{
					var oldest = backups.OrderBy(x => x.CreationTime).First();
					Log.Info("Deleting old backup: " + oldest.Name);
					oldest.Delete();
					backups = dirInfo.GetFiles("Backup_*");
				}
			}
			catch(Exception ex)
			{
				Log.Error("Error deleting old backup: " + ex);
			}

			Log.Info("Creating backup for today");

			CreateBackup(backupFileName);
		}

		public static void CreateBackup(string fileName)
		{
			try
			{
				var count = 1;
				var fileInfo = new FileInfo(fileName);
				while(File.Exists(Path.Combine(Config.Instance.BackupDir, fileName)))
					fileName = $"{fileInfo.Name}_{count++}.{fileInfo.Extension}";

				var backupFilePath = Path.Combine(Config.Instance.BackupDir, fileName);
				using(var zip = ZipFile.Open(backupFilePath, ZipArchiveMode.Create))
				{
					foreach(var file in Files)
					{
						var path = Path.Combine(Config.Instance.DataDir, file);
						if(File.Exists(path))
							zip.CreateEntryFromFile(path, file);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		internal static bool Restore(FileInfo backup, bool reload, params string[] files)
		{
			try
			{
				var archive = new ZipArchive(backup.OpenRead(), ZipArchiveMode.Read);
				if(files.Length == 0)
					archive.ExtractToDirectory(Config.Instance.DataDir, true);
				else
				{
					foreach(var file in files.Where(x => Files.Contains(x)))
						archive.GetEntry(file).ExtractToFile(Path.Combine(Config.Instance.DataDir, file), true);
				}
				if(!reload)
					return true;
				if(files.Length == 0 || files.Contains("config.xml"))
				{
					Config.Load();
					Config.Save();
				}
				if(files.Length == 0 || files.Contains("PlayerDecks.xml"))
				{
					DeckList.Reload();
					DeckList.Save();
				}
				if(files.Length == 0 || files.Contains("DeckStats.xml"))
				{
					DeckStatsList.Reload();
					DeckStatsList.Save();
				}
				if(files.Length == 0 || files.Contains("DefaultDeckStats.xml"))
				{
					DefaultDeckStats.Reload();
					DefaultDeckStats.Save();
				}
				return true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		internal static bool RestoreFromLatest(bool reload, int skip = 0, params string[] files)
		{
			if(!Directory.Exists(Config.Instance.BackupDir))
				return false;
			var dirInfo = new DirectoryInfo(Config.Instance.BackupDir);
			var latest = dirInfo.GetFiles("Backup*").OrderByDescending(x => x.CreationTimeUtc).Skip(skip).FirstOrDefault();
			return latest != null && Restore(latest, reload, files);
		}

		internal static T TryRestore<T>(string file)
		{
			var restored = false;
			try
			{
				Log.Info($"Restoring latest backup for {file}...");
				var filePath = Path.Combine(Config.Instance.DataDir, file);
				if(!(restored = RestoreFromLatest(false, 0, file)))
					return default(T);
				try
				{
					return XmlManager<T>.Load(filePath);
				}
				catch(Exception ex2)
				{
					Log.Error(ex2);
					Log.Info($"Restoring second to latest backup for {file}...");
					if(!(restored = RestoreFromLatest(false, 1, file)))
						return default(T);
					try
					{
						return XmlManager<T>.Load(filePath);
					}
					catch(Exception ex3)
					{
						Log.Error(ex3);
						return default(T);
					}
				}
			}
			finally
			{
				if(restored)
				{
					ErrorManager.AddError(file + " was corrupted but restored from the latest backup.",
						"This is likely due to an unexpected shutdown." + Environment.NewLine
						+ "Backups are generated on the first start of each day, so there may be lost data." + Environment.NewLine
						+ "We are very sorry for the inconvenience. :(", true);
				}
				else
				{
					ErrorManager.AddError(file + " was corrupted and could not be restored from a backup.",
						"This is likely due to an unexpected shutdown." + Environment.NewLine
						+ "We are very sorry for any data that was lost. :(", true);
				}
			}
		}
	}
}
