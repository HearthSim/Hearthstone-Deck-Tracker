#region

using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	public class HotKey
	{
		public HotKey()
		{
		}

		public HotKey(Keys key)
		{
			Key = key;
		}

		public HotKey(ModifierKeys mod, Keys key)
		{
			Mod = mod;
			Key = key;
		}

		public ModifierKeys Mod { get; set; } = ModifierKeys.None;

		public Keys Key { get; set; }

		public override bool Equals(object obj)
		{
			var hotKey = obj as HotKey;
			return hotKey != null && Equals(hotKey);
		}

		public bool Equals(HotKey hotKey) => hotKey.Mod == Mod && hotKey.Key == Key;

		public override int GetHashCode() => Mod.GetHashCode() * 31 + Key.GetHashCode();

		public static HotKey FromKeyPressedEventArgs(KeyPressedEventArgs args) => new HotKey(args.Modifier, args.Key);

		public override string ToString() => $"mod={Mod}, key={Key}";
	}
}
