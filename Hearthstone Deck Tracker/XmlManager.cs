using System.IO;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
	public static class XmlManager<T>
	{
		public static T Load(string path)
		{
			T instance;
			using (TextReader reader = new StreamReader(path))
			{
				XmlSerializer xml = new XmlSerializer(typeof(T));
				instance = (T)xml.Deserialize(reader);
			}
			return instance;
		}

		public static T LoadFromString(string xmlString)
		{
			T instance;
			using (TextReader reader = new StringReader(xmlString))
			{
				XmlSerializer xml = new XmlSerializer(typeof(T));
				instance = (T)xml.Deserialize(reader);
			}
			return instance;
		}

		public static void Save(string path, object obj)
		{
			using (TextWriter writer = new StreamWriter(path))
			{
				XmlSerializer xml = new XmlSerializer(typeof(T));
				xml.Serialize(writer, obj);
			}
		}
	}
}
