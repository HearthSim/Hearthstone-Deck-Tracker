using System;
using System.Diagnostics;

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginWrapper
	{
		public PluginWrapper()
		{
			_loaded = true;
		}
		public string FileName { get; set; }
		public IPlugin Plugin { get; set; }
		private bool _loaded;

		public string Name
		{
			get { return Plugin != null ? Plugin.Name : FileName; }
		}

		public string NameAndVersion
		{
			get { return Name + " " + (Plugin != null ? Plugin.Version.ToString() : ""); }
		}

		private bool _isEnabled;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if(value)
				{
					Logger.WriteLine("Enabled " + Name, "PluginWrapper");
					if(!_loaded)
						Load();
				}
				else
				{
					Logger.WriteLine("Disabled " + Name, "PluginWrapper");
					Unload();
				}
				_isEnabled = value;
			} 
		}

		public PluginWrapper(string fileName, IPlugin plugin)
		{
			FileName = fileName;
			Plugin = plugin;
			IsEnabled = true;
			//todo: load enabled from config
		}

		public void Load()
		{
			if(Plugin == null || (!IsEnabled && _loaded))
				return;
			try
			{
				Logger.WriteLine("Loading " + Name, "PluginWrapper");
				Plugin.OnLoad();
				_loaded = true;
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
			catch (Exception ex)
			{
				Logger.WriteLine("Error updating " + Name + ":\n" + ex, "PluginWrapper");
			}
			if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
			{
				Logger.WriteLine(string.Format("Updating {0} took {1} ms. Plugin disabled", Name, sw.ElapsedMilliseconds), "PluginWrapper");
				//TODO: ACTUALLY DISABLE
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
			if(Plugin == null || !IsEnabled || !_loaded)
				return;
			try
			{
				Plugin.OnUnload();
				_loaded = false;
			}
			catch (Exception ex)
			{
				Logger.WriteLine("Error unloading " + Name + ":\n" + ex, "PluginWrapper");
			}
		}
	}
}