using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	public partial class SquirrelInfo : INotifyPropertyChanged
	{
		private int _progress;
		private bool _inProgress;
		public readonly string InstallerFile = Path.Combine(Config.AppDataPath, "HDT-Installer.exe");

		public SquirrelInfo()
		{
			InitializeComponent();
		}

		public int Progress
		{
			get { return _progress; }
			set
			{
				if(value == _progress)
					return;
				_progress = value;
				OnPropertyChanged();
			}
		}

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e) => Core.MainWindow.FlyoutUpdateNotes.IsOpen = false;

		private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				Process.Start(InstallerFile);
				try
				{
					Core.MainWindow.Close();
				}
				catch
				{
					Application.Current.Shutdown();
				}
			}
			catch
			{
				ErrorManager.AddError("Error running the installer.", $"Please manually run '{InstallerFile}'.");
			}
		}

		private void ButtonBack_OnClick(object sender, RoutedEventArgs e) => TabControl.SelectedIndex = 0;

		private async void ButtonUpgrade_OnClick(object sender, RoutedEventArgs e)
		{
#if(!SQUIRREL)
			if(_inProgress)
			{
				TabControl.SelectedIndex = 1;
				return;
			}
			_inProgress = true;
			if(File.Exists(InstallerFile))
			{
				try
				{
					File.Delete(InstallerFile);
				}
				catch(Exception)
				{
					_inProgress = false;
					ErrorManager.AddError("Installer file already exists.", $"Please delete '{InstallerFile}' and try again.");
					return;
				}
			}
			TabControl.SelectedIndex = 1;
			LabelHeader.Content = "Downloading...";
			try
			{
				using(var wc = new WebClient())
				{
					var release = await GitHub.CheckForUpdate("HearthSim", "HDT-Releases", new Version(0, 0));
					var installer = release?.Assets?.FirstOrDefault(x => x.Name == "HDT-Installer.exe");
					if(installer != null)
					{
						wc.DownloadProgressChanged += (o, args) => Progress = args.ProgressPercentage;
						await wc.DownloadFileTaskAsync(installer.Url, InstallerFile);
					}
					ButtonRestart.IsEnabled = true;
					ButtonBack.Visibility = Visibility.Collapsed;
					LabelHeader.Content = "Download complete";
				}
			}
			catch(Exception ex)
			{
				_inProgress = false;
				Log.Error(ex);
				ErrorManager.AddError("Could not download new installer.", "Please manually download it from 'https://hsdecktracker.net/download'.");
				if(File.Exists(InstallerFile))
				{
					try
					{
						File.Delete(InstallerFile);
					}
					catch(Exception ex1)
					{
						Log.Error(ex1);
					}
				}
			}
#endif
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
