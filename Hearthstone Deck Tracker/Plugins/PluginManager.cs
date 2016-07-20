#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	internal class PluginManager
	{
		private const string DefaultPath = "Plugins";
		private static PluginManager _instance;
		private bool _update;
		public static DirectoryInfo LocalPluginDirectory => new DirectoryInfo(DefaultPath);
		public static DirectoryInfo PluginDirectory => new DirectoryInfo(Path.Combine(Config.AppDataPath, DefaultPath));

		private PluginManager()
		{
			Plugins = new List<PluginWrapper>();
			SyncPlugins(PluginDirectory, LocalPluginDirectory, LocalPluginDirectory);
		}

		private void SyncPlugins(DirectoryInfo sourceDir, DirectoryInfo destDir, DirectoryInfo baseDir)
		{
			if(!sourceDir.Exists)
				return;
			if(!destDir.Exists)
				destDir.Create();
			var sourceFiles = sourceDir.GetFiles();
			var destFiles = destDir.GetFiles();
			foreach(var file in sourceFiles)
			{
				try
				{
					var destFile = destFiles.FirstOrDefault(x => x.Name == file.Name);
					if(destFile != null && destFile.LastWriteTimeUtc >= file.LastWriteTimeUtc)
						continue;
					var destPath = Path.Combine(destDir.FullName, file.Name);
					Log.Info($"{(destFile == null ? "Adding" : "Updating")} {((destFile?.FullName) ?? Path.Combine(destDir.FullName, file.Name)).Substring(baseDir.FullName.Length + 1)}");
					File.Copy(file.FullName, destPath);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
			foreach(var file in destFiles.Where(df => sourceFiles.All(sf => sf.Name != df.Name)))
			{
				try
				{
					Log.Info($"Deleting {file.FullName.Substring(baseDir.FullName.Length + 1)}");
					file.Delete();
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
			var sourceDirs = sourceDir.GetDirectories();
			var destDirs = destDir.GetDirectories();
			foreach(var dir in destDirs.Where(df => sourceDirs.All(sf => sf.Name != df.Name)))
			{
				try
				{
					Log.Info($"Deleting {dir.FullName.Substring(baseDir.FullName.Length + 1)}");
					dir.Delete(true);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
			foreach(var dir in sourceDir.GetDirectories())
			{
				try
				{
					SyncPlugins(dir, destDirs.FirstOrDefault(x => x.Name == dir.Name)
						?? new DirectoryInfo(Path.Combine(destDir.FullName, dir.Name)), baseDir);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
		}

		public static int MaxPluginExecutionTime => 2000;

		public List<PluginWrapper> Plugins { get; }

		public static PluginManager Instance => _instance ?? (_instance = new PluginManager());

		private static string PluginSettingsFile => Path.Combine(Config.Instance.ConfigDir, "plugins.xml");

		public static int MaxExceptions => 100;

		public void LoadPlugins() => LoadPlugins(DefaultPath, true);

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
			Log.Info("Loading Plugins...");
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
						Log.Error("Error loading " + pFileName + ":\n" + ex);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Error("Error loading " + pFileName + ":\n" + ex);
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

		public void StopUpdate() => _update = false;

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
				Log.Error("Error loading plugin settings:\n" + ex);
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
				Log.Error("Error saving plugin settings:\n" + ex);
			}
		}
	}
}