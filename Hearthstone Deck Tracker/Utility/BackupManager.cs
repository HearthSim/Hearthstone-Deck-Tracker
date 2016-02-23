#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
	}
}