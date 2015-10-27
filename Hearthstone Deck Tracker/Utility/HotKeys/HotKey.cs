#region

using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	public class HotKey
	{
		private ModifierKeys _mod = ModifierKeys.None;

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

		public ModifierKeys Mod
		{
			get { return _mod; }
			set { _mod = value; }
		}

		public Keys Key { get; set; }

		public override bool Equals(object obj)
		{
			var hotKey = obj as HotKey;
			if(hotKey != null)
				return Equals(hotKey);
			return false;
		}

		public bool Equals(HotKey hotKey)
		{
			return hotKey.Mod == Mod && hotKey.Key == Key;
		}

		public override int GetHashCode()
		{
			return Mod.GetHashCode() * 31 + Key.GetHashCode();
		}

		public static HotKey FromKeyPressedEventArgs(KeyPressedEventArgs args)
		{
			return new HotKey(args.Modifier, args.Key);
		}
	}
}