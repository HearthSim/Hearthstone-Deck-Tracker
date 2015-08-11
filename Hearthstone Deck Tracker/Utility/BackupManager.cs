#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class BackupManager
	{
		private const int MaxBackups = 7;
		private static readonly string[] Files = {"PlayerDecks.xml", "DeckStats.xml", "DefaultDeckStats.xml", "config.xml"};

		public static void Run()
		{
			Logger.WriteLine("Running BackupManager", "BackupManager");
			if(!Directory.Exists(Config.Instance.BackupDir))
				Directory.CreateDirectory(Config.Instance.BackupDir);
			var dirInfo = new DirectoryInfo(Config.Instance.BackupDir);
			var backupFileName = string.Format("Backup_{0}.zip", DateTime.Today.ToString("ddMMyyyy"));

			try
			{
				var backups = dirInfo.GetFiles("Backup_*");
				while(backups.Count() > MaxBackups)
				{
					var oldest = backups.OrderBy(x => x.CreationTime).First();
					Logger.WriteLine("Deleting old backup: " + oldest.Name, "BackupManager");
					oldest.Delete();
					backups = dirInfo.GetFiles("Backup_*");
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error deleting old backup: " + ex, "BackupManager");
			}

			if(dirInfo.GetFiles().Any(x => x.Name == backupFileName))
			{
				Logger.WriteLine("Backup for today already exists", "BackupManager");
				return;
			}

			Logger.WriteLine("Creating backup for today", "BackupManager");

			CreateBackup(backupFileName);
		}

		public static void CreateBackup(string fileName)
		{
			try
			{
				var count = 1;
				var fileInfo = new FileInfo(fileName);
				while(File.Exists(Path.Combine(Config.Instance.BackupDir, fileName)))
					fileName = string.Format("{0}_{1}.{2}", fileInfo.Name, count++, fileInfo.Extension);

				var backupFilePath = Path.Combine(Config.Instance.BackupDir, fileName);
				using(var zip = ZipFile.Open(backupFilePath, ZipArchiveMode.Create))
				{
					foreach(var file in Files)
					{
						var path = Path.Combine(Config.Instance.DataDir, file);
						zip.CreateEntryFromFile(path, file);
					}
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error creating backup: " + ex, "BackupManager");
			}
		}
	}
}