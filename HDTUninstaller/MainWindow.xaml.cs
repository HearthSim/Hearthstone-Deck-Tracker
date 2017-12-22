#region

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

#endregion

namespace HDTUninstaller
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private string LogConfigPath
		{
			get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Blizzard\Hearthstone\log.config"; }
		}

		private string HDTDirectory
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HearthstoneDeckTracker"); }
		}

		private bool LoggingEnabled
		{
			get { return File.Exists(LogConfigPath); }
		}

		private bool DataExists
		{
			get { return Directory.Exists(HDTDirectory); }
		}

		private bool AutostartEnabled
		{
			get
			{
				var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				if(regKey != null)
					return regKey.GetValue("Hearthstone Deck Tracker") != null;
				return false;
			}
		}

		public string TextLogging
		{
			get { return LoggingEnabled ? "REMOVE" : "REMOVED"; }
		}

		public string TextData
		{
			get { return DataExists ? "REMOVE" : "REMOVED"; }
		}

		public string TextAutostart
		{
			get { return AutostartEnabled ? "REMOVE" : "REMOVED"; }
		}

		public SolidColorBrush BackgroundLogging
		{
			get { return new SolidColorBrush(LoggingEnabled ? Colors.Red : Colors.Green); }
		}

		public SolidColorBrush BackgroundData
		{
			get { return new SolidColorBrush(DataExists ? Colors.Red : Colors.Green); }
		}

		public SolidColorBrush BackgroundAutostart
		{
			get { return new SolidColorBrush(AutostartEnabled ? Colors.Red : Colors.Green); }
		}
		
		private void ButtonAutostart_Click(object sender, RoutedEventArgs e)
		{
			if(!AutostartEnabled)
				return;
			try
			{
				var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				if(regKey != null)
					regKey.DeleteValue("Hearthstone Deck Tracker", false);
				OnPropertyChanged("TextAutostart");
				OnPropertyChanged("BackgroundAutostart");
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void ButtonLogging_Click(object sender, RoutedEventArgs e)
		{
			if(!LoggingEnabled)
				return;
			try
			{
				File.Delete(LogConfigPath);
				OnPropertyChanged("TextLogging");
				OnPropertyChanged("BackgroundLogging");
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void ButtonData_Click(object sender, RoutedEventArgs e)
		{
			if(!DataExists)
				return;
			try
			{
				var result = MessageBox.Show("Are you sure? This can not be undone!", "Delete HDT Data", MessageBoxButton.OKCancel);
				if(result == MessageBoxResult.OK)
				{
					Directory.Delete(HDTDirectory, true);
					OnPropertyChanged("TextData");
					OnPropertyChanged("BackgroundData");
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
