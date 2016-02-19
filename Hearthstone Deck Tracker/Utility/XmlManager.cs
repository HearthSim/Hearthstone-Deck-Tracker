#region

using System;
using System.IO;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class XmlManager<T>
	{
		public static T Load(string path)
		{
			Log.Debug("Loading file: " + path);
			T instance;
			using(TextReader reader = new StreamReader(path))
			{
				var xml = new XmlSerializer(typeof(T));
				instance = (T)xml.Deserialize(reader);
			}

			Log.Debug("File loaded: " + path);
			return instance;
		}

		public static T LoadFromString(string xmlString)
		{
			T instance;
			using(TextReader reader = new StringReader(xmlString))
			{
				var xml = new XmlSerializer(typeof(T));
				instance = (T)xml.Deserialize(reader);
			}
			return instance;
		}

		public static void Save(string path, object obj)
		{
			var i = 0;
			var deleteBackup = true;
			var backupPath = path.Replace(".xml", "_backup.xml");

			//make sure not to overwrite backups that could not be restored (were not deleted)
			while(File.Exists(backupPath))
				backupPath = path.Replace(".xml", "_backup" + i++ + ".xml");

			Log.Debug("Saving file: " + path);

			//create backup
			if(File.Exists(path))
			{
				try
				{
					File.Copy(path, backupPath);
				}
				catch(IOException ex)
				{
					Log.Error($"Error copying file: {backupPath}\n{ex}");
				}
			}
			try
			{
				//standard serialization
				using(TextWriter writer = new StreamWriter(path))
				{
					var xml = new XmlSerializer(typeof(T));
					xml.Serialize(writer, obj);
				}
				Log.Debug("File saved: " + path);
			}
			catch(Exception e)
			{
				Log.Error("Error saving file: " + path + "\n" + e);
				try
				{
					//restore backup
					File.Delete(path);
					if(File.Exists(backupPath))
						File.Move(backupPath, path);
				}
				catch(Exception e2)
				{
					//restoring failed 
					deleteBackup = false;
					Log.Error("Error restoring backup for: " + path + "\n" + e2);
				}
			}
			finally
			{
				if(deleteBackup && File.Exists(backupPath))
				{
					try
					{
						File.Delete(backupPath);
					}
					catch(Exception)
					{
						//note sure, todo?
					}
				}
			}
		}
	}
}