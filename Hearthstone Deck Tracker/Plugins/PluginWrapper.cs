#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MenuItem = System.Windows.Controls.MenuItem;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginWrapper : INotifyPropertyChanged
	{
		private int _exceptions;
		private int _unhandledExceptions;
		private bool _isEnabled;
		private bool _loaded;

		public PluginWrapper()
		{
			_loaded = true;
		}

		public PluginWrapper(string fileName, IPlugin plugin)
		{
			FileName = fileName;
			Plugin = plugin;
		}

		public string FileName { get; set; }
		public IPlugin Plugin { get; set; }
		private MenuItem MenuItem { get; set; }
		public Plugin TempPlugin { get; set; }

		private string _updateHyperlink;
		public string UpdateHyperlink
		{
			get { return _updateHyperlink; } 
			set { _updateHyperlink = value; NotifyPropertyChanged("UpdateHyperlink"); }
		}

		private string _updateTextColor;
		public string UpdateTextColor
		{
			get { return _updateTextColor; }
			set { _updateTextColor = value; NotifyPropertyChanged("UpdateTextColor"); }
		}

		private string _updateTextDecorations;
		public string UpdateTextDecorations
		{
			get { return _updateTextDecorations; }
			set { _updateTextDecorations = value; NotifyPropertyChanged("UpdateTextDecorations"); }
		}

		private string _updateTextEnabled;
		public string UpdateTextEnabled
		{
			get { return _updateTextEnabled; }
			set { _updateTextEnabled = value; NotifyPropertyChanged("UpdateTextEnabled"); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string Name => Plugin != null ? Plugin.Name : FileName;

		public string NameAndVersion => Name + " " + (Plugin?.Version.ToString() ?? "");

		public string RelativeFilePath => new Uri(AppDomain.CurrentDomain.BaseDirectory).MakeRelativeUri(new Uri(FileName)).ToString();

		public string Repourl { get; set; }
		//public string Repourl => Updatable != null ? Updatable.Repourl : "";

		public bool RepoAvailable => !string.IsNullOrEmpty(Repourl);

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if(value)
				{
					if(!_loaded)
					{
						var couldLoad = Load();
						Log.Info("Enabled " + Name);
						if(!couldLoad)
							return;
					}
				}
				else
				{
					if(_loaded)
					{
						Log.Info("Disabled " + Name);
						Unload();
					}
				}
				_isEnabled = value;
				NotifyPropertyChanged("IsEnabled");
			}
		}

		public bool Load()
		{
			if(Plugin == null)
				return false;
			try
			{
				Log.Info("Loading " + Name);
				Plugin.OnLoad();
				_loaded = true;
				_exceptions = 0;
				MenuItem = Plugin.MenuItem;
				if(MenuItem != null)
				{
					Core.MainWindow.MenuItemPlugins.Items.Add(MenuItem);
					Core.MainWindow.MenuItemPluginsEmpty.Visibility = Visibility.Collapsed;
				}
			}
			catch(Exception ex)
			{
				ErrorManager.AddError("Error loading Plugin \"" + Name + "\"",
									  "Make sure you are using the latest version of the Plugin and HDT.\n\n" + ex);
				Log.Error(Name + ":\n" + ex);
				return false;
			}
			return true;
		}

		public void Update()
		{
			if(Plugin == null || !IsEnabled)
				return;
			var sw = Stopwatch.StartNew();
			try
			{
				Plugin.OnUpdate();
			}
			catch(Exception ex)
			{
				Log.Error(Name + ":\n" + ex);
				_exceptions++;
				if(_exceptions > PluginManager.MaxExceptions)
				{
					ErrorManager.AddError(NameAndVersion + " threw too many exceptions, disabled Plugin.",
										  "Make sure you are using the latest version of the Plugin and HDT.\n\n" + ex);
					IsEnabled = false;
				}
			}
			if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
			{
				Log.Warn($"Updating {Name} took {sw.ElapsedMilliseconds} ms.");
#if(!DEBUG)
	//IsEnabled = false;
#endif
			}
		}

		public void OnButtonPress()
		{
			if(Plugin == null)
				return;
			try
			{
				Plugin.OnButtonPress();
			}
			catch(Exception ex)
			{
				Log.Error(Name + "\n" + ex);
			}
		}

		public void Unload()
		{
			if(Plugin == null)
				return;
			try
			{
				Plugin.OnUnload();
			}
			catch(Exception ex)
			{
				Log.Error(Name + ":\n" + ex);
			}
			_loaded = false;
			if(MenuItem != null)
			{
				Core.MainWindow.MenuItemPlugins.Items.Remove(MenuItem);
				if(Core.MainWindow.MenuItemPlugins.Items.Count == 1)
					Core.MainWindow.MenuItemPluginsEmpty.Visibility = Visibility.Visible;
			}
		}

		internal bool UnhandledException() => ++_unhandledExceptions > PluginManager.MaxExceptions / 10;
	}
}