#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private const string NoticeFileName = "READ THIS.txt";
		private const string TriggerTypeName = "MergedTrigger";
		private static PluginManager _instance;
		private bool _update;
		public static DirectoryInfo LocalPluginDirectory => new DirectoryInfo(DefaultPath);
		public static DirectoryInfo PluginDirectory => new DirectoryInfo(Path.Combine(Config.AppDataPath, DefaultPath));

		private PluginManager()
		{
			Plugins = new ObservableCollection<PluginWrapper>();
			try
			{
				if(!LocalPluginDirectory.Exists)
					LocalPluginDirectory.Create();
				if(!PluginDirectory.Exists)
					PluginDirectory.Create();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			SyncPlugins();
			CreateNoticeFile();
		}


		private void CreateNoticeFile()
		{
			var file = Path.Combine(PluginDirectory.FullName, NoticeFileName);
			if(File.Exists(file))
			{
				try
				{
					File.Delete(file);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
			file = Path.Combine(LocalPluginDirectory.FullName, NoticeFileName);
			if(File.Exists(file))
				return;
			try
			{
				using(var sw = new StreamWriter(file))
				{
					sw.WriteLine("PLUGINS INSTALLED TO THIS DIRECTORY WILL BE REMOVED!");
					sw.WriteLine("");
					sw.WriteLine("Please install your new plugins to '%AppData%/HearthstoneDeckTracker'.");
					sw.WriteLine("'options > tracker > plugins > plugins folder' will open that directory for you.");
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public IEnumerable<FileInfo> SyncPlugins() => SyncPlugins(PluginDirectory, LocalPluginDirectory, LocalPluginDirectory).ToList();

		private IEnumerable<FileInfo> SyncPlugins(DirectoryInfo sourceDir, DirectoryInfo destDir, DirectoryInfo baseDir)
		{
			if(!sourceDir.Exists)
				yield break;
			if(!destDir.Exists)
				destDir.Create();
			var sourceFiles = sourceDir.GetFiles();
			var destFiles = destDir.GetFiles();
			foreach(var file in sourceFiles)
			{
				var destFile = destFiles.FirstOrDefault(x => x.Name == file.Name);
				if(destFile != null && destFile.LastWriteTimeUtc >= file.LastWriteTimeUtc)
					continue;
				var destPath = Path.Combine(destDir.FullName, file.Name);
				Log.Info($"{(destFile == null ? "Adding" : "Updating")} {((destFile?.FullName) ?? Path.Combine(destDir.FullName, file.Name)).Substring(baseDir.FullName.Length + 1)}");
				var success = false;
				try
				{
					File.Copy(file.FullName, destPath, true);
					success = true;
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
				if(success)
					yield return new FileInfo(destPath);
			}
			foreach(var file in destFiles.Where(df => sourceFiles.All(sf => sf.Name != df.Name)))
			{
				try
				{
					if(file.Name == NoticeFileName)
						continue;
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
				var plugins = new List<FileInfo>();
				try
				{
					var synced = SyncPlugins(dir, destDirs.FirstOrDefault(x => x.Name == dir.Name)
						?? new DirectoryInfo(Path.Combine(destDir.FullName, dir.Name)), baseDir);
					plugins.AddRange(synced);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
				foreach(var plugin in plugins)
					yield return plugin;
			}
		}

		public static int MaxPluginExecutionTime => 2000;

		public ObservableCollection<PluginWrapper> Plugins { get; }

		public static PluginManager Instance => _instance ?? (_instance = new PluginManager());

		private static string PluginSettingsFile => Path.Combine(Config.Instance.ConfigDir, "plugins.xml");

		public static int MaxExceptions => 100;

		public void LoadPluginsFromDefaultPath() => LoadPluginsFromPath(DefaultPath, true);

		public void LoadPluginsFromPath(string pluginPath, bool checkSubDirs)
		{
			if(!Directory.Exists(pluginPath))
				return;
			if(Plugins.Any())
				UnloadPlugins();
			var files = Helper.GetFileInfos(pluginPath, checkSubDirs);
			Log.Info("Loading Plugins...");
			LoadPlugins(files);
			LoadPluginSettings();
		}

		public void LoadPlugins(IEnumerable<FileInfo> files)
		{
			foreach(var file in files.Where(f => f.Extension.Equals(".dll")))
			{
				var plugins = GetModule(file.FullName, typeof(IPlugin));
				foreach(var p in plugins)
					Plugins.Add(p);
			}
		}

		private IEnumerable<PluginWrapper> GetModule(string pFileName, Type pTypeInterface)
		{
			var plugins = new List<PluginWrapper>();
			try
			{
				var assembly = Assembly.LoadFrom(pFileName);
				// trigger the loading of embedded dependencies
				TriggerAssembly(assembly);
				foreach(var type in assembly.GetTypes())
				{
					try
					{
						if(!type.IsPublic || type.IsAbstract)
							continue;
						var typeInterface = type.GetInterface(pTypeInterface.ToString(), true);
						if(typeInterface == null)
							continue;
						if(Activator.CreateInstance(type) is IPlugin instance)
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
					var path = setting.FileName;
					if(Uri.IsWellFormedUriString(path, UriKind.Absolute))
					{
						var uri = new Uri(path, UriKind.Absolute);
						path = string.Join("", uri.Segments.SkipWhile(x => x != "Plugins/"));
					}
					var plugin = Plugins.FirstOrDefault(x => x.RelativeFilePath == path && x.Name == setting.Name);
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

		// Triggers an assembly with embedded dependencies to load its internal assembly
		// resolver by instantiating a known type (TriggerType) in the default namespace, 
		// this avoids errors when using reflection. Costura.Fody and other tools can 
		// create these kinds of merged assemblies.
		private void TriggerAssembly(Assembly assembly)
		{
			try
			{
				if(assembly.CreateInstance(TriggerTypeName) != null)
					Log.Debug($"Created {TriggerTypeName} in {assembly.GetName().Name}", "Trigger");
			}
			catch(Exception ex)
			{
				Log.Debug($"Creating {TriggerTypeName} in {assembly.GetName().Name} errored ({ex.Message})", "Trigger");
			}
		}
	}
}
