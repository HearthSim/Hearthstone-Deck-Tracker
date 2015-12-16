#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	[XmlRoot("HotKeyConfig")]
	public sealed class HotKeyConfig
	{
		private List<HotKeyConfigItem> _hotKeys = new List<HotKeyConfigItem>();

		public static HotKeyConfig Instance
		{
			get { return LazyInstance.Value; }
		}

		[XmlElement("HotKey")]
		public List<HotKeyConfigItem> HotKeys
		{
			get { return _hotKeys; }
			set { _hotKeys = value; }
		}

		private static string ConfigPath
		{
			get { return Path.Combine(Config.Instance.ConfigDir, "HotKeys.xml"); }
		}

		private static HotKeyConfig Load()
		{
			if(File.Exists(ConfigPath))
			{
				try
				{
					return XmlManager<HotKeyConfig>.Load(ConfigPath);
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error loading HotKeyConfig: " + ex, "HotKeyConfig");
				}
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
				Logger.WriteLine("Error saving HotKeyConfig: " + ex, "HotKeyConfig");
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
			public HotKey HotKey
			{
				get { return new HotKey(Mod, Key); }
			}
		}

		private static readonly Lazy<HotKeyConfig> LazyInstance = new Lazy<HotKeyConfig>(Load);
	}
}