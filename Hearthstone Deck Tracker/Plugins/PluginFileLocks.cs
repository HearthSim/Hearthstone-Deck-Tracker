using System;
using System.Collections.Generic;
using System.IO;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Plugins;

internal static class PluginFileLocks
{
	private static readonly Dictionary<string, FileStream> Handles = new(StringComparer.OrdinalIgnoreCase);

	public static void Lock()
	{
		FileInfo[] files;
		try
		{
			var dir = PluginManager.PluginDirectory;
			if(!dir.Exists)
				return;
			files = dir.GetFiles("*.dll", SearchOption.AllDirectories);
		}
		catch(Exception ex)
		{
			Log.Error(ex);
			return;
		}
		foreach(var file in files)
		{
			if(Handles.ContainsKey(file.FullName))
				continue;
			try
			{
				Handles[file.FullName] = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}
	}

	public static IDisposable Unlock()
	{
		foreach(var handle in Handles.Values)
			handle.Dispose();
		Handles.Clear();
		return new Deferred(Lock);
	}
}
