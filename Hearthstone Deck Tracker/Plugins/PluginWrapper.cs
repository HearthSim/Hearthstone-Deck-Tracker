#region

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls.Error;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginWrapper
	{
		private bool _isEnabled;
		private bool _loaded;
		private int _exceptions;

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

		public string Name
		{
			get { return Plugin != null ? Plugin.Name : FileName; }
		}

		public string NameAndVersion
		{
			get { return Name + " " + (Plugin != null ? Plugin.Version.ToString() : ""); }
		}

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
						Logger.WriteLine("Enabled " + Name, "PluginWrapper");
						if(!couldLoad)
							return;
					}
				}
				else
				{
					if(_loaded)
					{
						Logger.WriteLine("Disabled " + Name, "PluginWrapper");
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
				Logger.WriteLine("Loading " + Name, "PluginWrapper");
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
				ErrorManager.AddError("Error loading Plugin \"" + Name + "\"", "Make sure you are using the latest version of the Plugin and HDT.\n\n" + ex);
				Logger.WriteLine("Error loading " + Name + ":\n" + ex, "PluginWrapper");
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
				Logger.WriteLine("Error updating " + Name + ":\n" + ex, "PluginWrapper");
				_exceptions++;
				if(_exceptions > PluginManager.MaxExceptions)
				{
					ErrorManager.AddError(NameAndVersion + " threw too many exceptions, disabled Plugin.", "Make sure you are using the latest version of the Plugin and HDT.\n\n" + ex);
					IsEnabled = false;
				}
			}
			if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
			{
				Logger.WriteLine(string.Format("Warning: Updating {0} took {1} ms.", Name, sw.ElapsedMilliseconds), "PluginWrapper");
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
				Logger.WriteLine("Error performing OnButtonPress for " + Name + ":\n" + ex, "PluginWrapper");
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
				Logger.WriteLine("Error unloading " + Name + ":\n" + ex, "PluginWrapper");
			}
			_loaded = false;
			if(MenuItem != null)
			{
				Core.MainWindow.MenuItemPlugins.Items.Remove(MenuItem);
				if(Core.MainWindow.MenuItemPlugins.Items.Count == 1)
					Core.MainWindow.MenuItemPluginsEmpty.Visibility = Visibility.Visible;
			}
		}
	}
}