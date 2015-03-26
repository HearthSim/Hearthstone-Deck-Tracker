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
		public List<PluginWrapper> Plugins { get; private set; }
		private static PluginManager _instance;
		private bool _update;

		private PluginManager()
		{
			Plugins = new List<PluginWrapper>();
		}

		public static PluginManager Instance
		{
			get { return _instance ?? (_instance = new PluginManager()); }
		}

		public void LoadPlugins()
		{
			LoadPlugins(DefaultPath);
		}

		public void LoadPlugins(string pPluginPath)
		{
			if(Plugins.Any())
				UnloadPlugins();
			var files = Directory.GetFiles(pPluginPath);

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
			foreach(var p in Plugins)
				p.Load();
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
							plugins.Add(new PluginWrapper(type.Name, instance));
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

		private void UnloadPlugins()
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
	}
}