#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginManager
	{
		private const string DefaultPath = "Plugins";
		private static PluginManager _instance;
		private bool _update;

		private PluginManager()
		{
			Plugins = new List<PluginWrapper>();
		}

		public static int MaxPluginExecutionTime
		{
			get { return 2000; }
		}

		public List<PluginWrapper> Plugins { get; private set; }

		public static PluginManager Instance
		{
			get { return _instance ?? (_instance = new PluginManager()); }
		}

		private static string PluginSettingsFile
		{
			get { return Path.Combine(Config.Instance.ConfigDir, "plugins.xml"); }
		}

		public void LoadPlugins()
		{
			LoadPlugins(DefaultPath, true);
		}

		public void LoadPlugins(string pluginPath, bool checkSubDirs)
		{
			if(!Directory.Exists(pluginPath))
				return;
			if(Plugins.Any())
				UnloadPlugins();
			var dirInfo = new DirectoryInfo(pluginPath);

			var files = dirInfo.GetFiles().Select(f => f.FullName).ToList();
			if(checkSubDirs)
			{
				foreach(var dir in dirInfo.GetDirectories())
					files.AddRange(dir.GetFiles().Select(f => f.FullName));
			}

			foreach(var file in files)
			{
				var fileInfo = new FileInfo(file);

				if(fileInfo.Extension.Equals(".dll"))
				{
					var plugins = GetModule(file, typeof(IPlugin));
					foreach(var p in plugins)
						Plugins.Add(p);
				}
			}
			Logger.WriteLine("Loading Plugins...", "PluginManager");
			LoadPluginSettings();
		}

		private IEnumerable<PluginWrapper> GetModule(string pFileName, Type pTypeInterface)
		{
			var plugins = new List<PluginWrapper>();
			try
			{
				var assembly = Assembly.LoadFrom(pFileName);
				foreach(var type in assembly.GetTypes())
				{
					try
					{
						if(!type.IsPublic || type.IsAbstract)
							continue;
						var typeInterface = type.GetInterface(pTypeInterface.ToString(), true);
						if(typeInterface == null)
							continue;
						var instance = Activator.CreateInstance(type) as IPlugin;
						if(instance != null)
							plugins.Add(new PluginWrapper(pFileName, instance));
					}
					catch(Exception ex)
					{
						Logger.WriteLine("Error Loading " + pFileName + ":\n" + ex, "PluginManager");
					}
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error Loading " + pFileName + ":\n" + ex, "PluginManager");
			}
			return plugins;
		}

		internal void UnloadPlugins()
		{
			//not really unloading anything but it's the best I can do without multiple assemblies
			foreach(var plugin in Plugins)
				plugin.Unload();
			Plugins.Clear();
		}

		public void Update()
		{
			foreach(var plugin in Plugins)
				plugin.Update();
		}

		public async void StartUpdateAsync()
		{
			if(_update)
				return;
			_update = true;
			while(_update)
			{
				Update();
				await Task.Delay(100);
			}
		}

		public void StopUpdate()
		{
			_update = false;
		}

		private void LoadPluginSettings()
		{
			if(!File.Exists(PluginSettingsFile))
				return;
			try
			{
				var settings = XmlManager<List<PluginSettings>>.Load(PluginSettingsFile);
				foreach(var setting in settings)
				{
					var plugin = Plugins.FirstOrDefault(x => x.FileName == setting.FileName && x.Name == setting.Name);
					if(plugin != null)
						plugin.IsEnabled = setting.IsEnabled;
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error loading plugin settings:\n" + ex, "PluginManager");
			}
		}

		internal static void SavePluginsSettings()
		{
			try
			{
				var settings = Instance.Plugins.Select(x => new PluginSettings(x)).ToList();
				XmlManager<List<PluginSettings>>.Save(PluginSettingsFile, settings);
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error saving plugin settings:\n" + ex, "PluginManager");
			}
		}
	}
}