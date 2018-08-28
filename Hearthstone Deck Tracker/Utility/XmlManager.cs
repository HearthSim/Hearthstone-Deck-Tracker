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
			Log.Debug("Saving file: " + path);
			var tempFile = path + ".tmp";
			try
			{
				Serialize(tempFile, obj);
				if (File.Exists(path))
				{
					File.Replace(tempFile, path, path + ".bak");
				}
				else
				{
					File.Move(tempFile, path);
				}
				Log.Debug("File saved: " + path);
			}
			catch(Exception e)
			{
				Log.Error(e);
				TryDelete(tempFile);
			}
		}

		private static void Serialize(string path, object obj)
		{
			using (TextWriter writer = new StreamWriter(path))
			{
				var xml = new XmlSerializer(typeof(T));
				xml.Serialize(writer, obj);
			}
		}

		private static bool TryDelete(string path)
		{
			if (!File.Exists(path))
				return true;

			try
			{
				File.Delete(path);
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
