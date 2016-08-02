#region

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginWrapper
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

		public string Name => Plugin != null ? Plugin.Name : FileName;

		public string NameAndVersion => Name + " " + (Plugin?.Version.ToString() ?? "");

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