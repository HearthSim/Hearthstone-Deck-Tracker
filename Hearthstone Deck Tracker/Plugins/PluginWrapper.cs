#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	public class PluginWrapper : INotifyPropertyChanged
	{
		private int _exceptions;
		private int _unhandledExceptions;
		private bool _isEnabled;
		private bool _loaded;
		private MenuItem _menuItem;

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

		public MenuItem MenuItem
		{
			get => _menuItem;
			set
			{
				_menuItem = value; 
				OnPropertyChanged();
			}
		}

		public string Name => Plugin != null ? Plugin.Name : FileName;

		public string NameAndVersion => Name + " " + (Plugin?.Version.ToString() ?? "");

		public string RelativeFilePath => new Uri(AppDomain.CurrentDomain.BaseDirectory).MakeRelativeUri(new Uri(FileName)).ToString();

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
				OnPropertyChanged();
			}
		}

		public bool Load()
		{
			if(Plugin == null)
				return false;
			try
			{
				Log.Info("Loading " + Name);
				var start = DateTime.Now;
				Plugin.OnLoad();
				Influx.OnPluginLoaded(Plugin, DateTime.Now - start);
				_loaded = true;
				_exceptions = 0;
				MenuItem = Plugin.MenuItem;
			}
			catch(Exception ex)
			{
				ErrorManager.AddError("Error loading Plugin \"" + Name + "\"",
				                      "Make sure you are using the latest version of the Plugin and HDT.\n\n" + ex);
				Log.Error(Name + ":\n" + ex);
				Influx.OnPluginLoadingError(Plugin);
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
			MenuItem = null;
		}

		internal bool UnhandledException() => ++_unhandledExceptions > PluginManager.MaxExceptions / 10;
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
