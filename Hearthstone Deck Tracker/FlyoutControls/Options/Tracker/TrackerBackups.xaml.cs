#region

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherTracker.xaml
	/// </summary>
	public partial class TrackerBackups
	{
		public TrackerBackups()
		{
			InitializeComponent();
		}

		public void Load()
		{
			var dirInfo = new DirectoryInfo(Config.Instance.BackupDir);
			if(dirInfo.Exists)
			{
				foreach(var file in dirInfo.GetFiles("Backup*.zip").OrderBy(x => x.CreationTime))
					ListBoxBackups.Items.Add(new BackupFile { FileInfo = file });
			}
		}

		private async void ButtonRestore_Click(object sender, RoutedEventArgs e)
		{
			var selected = ListBoxBackups.SelectedItem as BackupFile;
			if(selected != null)
			{
				var result =
					await
					Helper.MainWindow.ShowMessageAsync("Restore backup " + selected.DisplayName,
					                                   "This can not be undone! Make sure you have a current backup (if necessary). To create one, CANCEL and click \"CREATE NEW\".",
					                                   MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Affirmative)
				{
					var archive = new ZipArchive(selected.FileInfo.OpenRead(), ZipArchiveMode.Read);
					archive.ExtractToDirectory(Config.Instance.DataDir, true);
					Config.Load();
					Config.Save();
					DeckList.Load();
					DeckList.Save();
					DeckStatsList.Load();
					DeckStatsList.Save();
					DefaultDeckStats.Load();
					DefaultDeckStats.Save();
					Helper.MainWindow.ShowMessage("Success", "Please restart HDT for this to take effect.");
				}
			}
		}

		private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
		{
			BackupManager.CreateBackup(string.Format("BackupManual_{0}.zip", DateTime.Today.ToString("ddMMyyyy")));
			ListBoxBackups.Items.Clear();
			Load();
		}

		private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
		{
			if(ListBoxBackups.SelectedItems.Count == 0)
				return;
			var msg = ListBoxBackups.SelectedItems.Count == 1
				          ? "Delete backup " + ((BackupFile)ListBoxBackups.SelectedItem).DisplayName
				          : "Delete " + ListBoxBackups.SelectedItems.Count + " backups";
			var result =
				await Helper.MainWindow.ShowMessageAsync(msg, "Are you sure? This can not be undone!", MessageDialogStyle.AffirmativeAndNegative);
			if(result == MessageDialogResult.Affirmative)
			{
				foreach(var backupFile in ListBoxBackups.SelectedItems.OfType<BackupFile>())
				{
					try
					{
						File.Delete(backupFile.FileInfo.FullName);
					}
					catch(Exception ex)
					{
						Logger.WriteLine("Error deleting backup: " + backupFile.FileInfo.FullName);
					}
				}
				ListBoxBackups.Items.Clear();
				Load();
			}
		}

		private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Process.Start(Config.Instance.BackupDir);
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error opening backup dir:\n" + ex);
			}
		}

		public class BackupFile
		{
			public FileInfo FileInfo { get; set; }

			public string DisplayName
			{
				get { return FileInfo.CreationTime + " " + (FileInfo.Name.StartsWith("Backup_") ? "(auto)" : "(manual)"); }
			}
		}

		private void TrackerBackups_OnLoaded(object sender, RoutedEventArgs e)
		{
			ListBoxBackups.Items.Clear();
			Load();
		}
	}
}