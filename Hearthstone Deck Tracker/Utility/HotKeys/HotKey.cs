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

		public ModifierKeys Mod { get; } = ModifierKeys.None;

		public Keys Key { get; }

		public override bool Equals(object obj)
		{
		    return obj is HotKey hotKey && Equals(hotKey);
		}

		public bool Equals(HotKey hotKey) => hotKey.Mod == Mod && hotKey.Key == Key;

		public override int GetHashCode() => Mod.GetHashCode() * 31 + Key.GetHashCode();

		public static HotKey FromKeyPressedEventArgs(KeyPressedEventArgs args) => new HotKey(args.Modifier, args.Key);

		public override string ToString() => $"mod={Mod}, key={Key}";
	}
}
