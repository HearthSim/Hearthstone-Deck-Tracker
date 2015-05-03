#region

using System;
using System.Diagnostics;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginWrapper
	{
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
						Load();
						Logger.WriteLine("Enabled " + Name, "PluginWrapper");
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

		public void Load()
		{
			if(Plugin == null)
				return;
			try
			{
				Logger.WriteLine("Loading " + Name, "PluginWrapper");
				Plugin.OnLoad();
				_loaded = true;
				MenuItem = Plugin.MenuItem;
				if(MenuItem != null)
					Helper.MainWindow.MenuItemPlugins.Items.Add(MenuItem);
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error loading " + Name + ":\n" + ex, "PluginWrapper");
			}
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
			}
			if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
			{
				Logger.WriteLine(string.Format("Updating {0} took {1} ms. Plugin disabled", Name, sw.ElapsedMilliseconds), "PluginWrapper");
#if(!DEBUG)
				IsEnabled = false;
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
				_loaded = false;
				if(MenuItem != null)
					Helper.MainWindow.MenuItemPlugins.Items.Remove(MenuItem);
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error unloading " + Name + ":\n" + ex, "PluginWrapper");
			}
		}
	}
}