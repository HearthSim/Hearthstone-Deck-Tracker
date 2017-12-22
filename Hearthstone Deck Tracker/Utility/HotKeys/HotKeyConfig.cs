#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	[XmlRoot("HotKeyConfig")]
	public sealed class HotKeyConfig
	{
		private static readonly Lazy<HotKeyConfig> LazyInstance = new Lazy<HotKeyConfig>(Load);

		public static HotKeyConfig Instance => LazyInstance.Value;

		[XmlElement("HotKey")]
		public List<HotKeyConfigItem> HotKeys { get; set; } = new List<HotKeyConfigItem>();

		private static string ConfigPath => Path.Combine(Config.Instance.ConfigDir, "HotKeys.xml");

		private static HotKeyConfig Load()
		{
			if(!File.Exists(ConfigPath))
				return new HotKeyConfig();
			try
			{
				return XmlManager<HotKeyConfig>.Load(ConfigPath);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return new HotKeyConfig();
		}

		public static void Save()
		{
			try
			{
				XmlManager<HotKeyConfig>.Save(ConfigPath, Instance);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public void AddHotKey(HotKey hotKey, string actionName, bool save = true)
		{
			RemoveHotKey(hotKey, false);
			HotKeys.Add(new HotKeyConfigItem(hotKey, actionName));
			if(save)
				Save();
		}

		public void RemoveHotKey(HotKey hotKey, bool save = true)
		{
			var existing = HotKeys.FirstOrDefault(x => x.HotKey.Equals(hotKey));
			if(existing != null)
				HotKeys.Remove(existing);
			if(save)
				Save();
		}

		public class HotKeyConfigItem
		{
			public HotKeyConfigItem(HotKey hotKey, string action)
			{
				Key = hotKey.Key;
				Mod = hotKey.Mod;
				Action = action;
			}

			public HotKeyConfigItem()
			{
			}

			[XmlAttribute("Key")]
			public Keys Key { get; set; }

			[XmlAttribute("Mod")]
			public ModifierKeys Mod { get; set; }

			[XmlAttribute("Action")]
			public string Action { get; set; }

			[XmlIgnore]
			public HotKey HotKey => new HotKey(Mod, Key);
		}
	}
}
