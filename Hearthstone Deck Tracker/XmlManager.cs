using System.IO;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
	public static class XmlManager<T>
	{
		public static T Load(string path)
		{
			Logger.WriteLine("Loading file: " + path, "XmlManager", 1);
			T instance;
			using(TextReader reader = new StreamReader(path))
			{
				var xml = new XmlSerializer(typeof(T));
				instance = (T)xml.Deserialize(reader);
			}
			Logger.WriteLine("File loaded: " + path, "XmlManager", 1);
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
			Logger.WriteLine("Saving file: " + path, "XmlManager", 1);
			using(TextWriter writer = new StreamWriter(path))
			{
				var xml = new XmlSerializer(typeof(T));
				xml.Serialize(writer, obj);
			}
			Logger.WriteLine("File saved: " + path, "XmlManager", 1);
		}
	}
}