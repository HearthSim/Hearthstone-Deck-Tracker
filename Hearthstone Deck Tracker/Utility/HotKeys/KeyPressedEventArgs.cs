#region

using System;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	/// <summary>
	/// Event Args for the event that is fired after the hot key has been pressed.
	/// </summary>
	public class KeyPressedEventArgs : EventArgs
	{
		internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
		{
			Modifier = modifier;
			Key = key;
		}

		public ModifierKeys Modifier { get; private set; }
		public Keys Key { get; private set; }
	}
}
