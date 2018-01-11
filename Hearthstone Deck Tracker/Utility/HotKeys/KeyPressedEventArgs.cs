#region

using System;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	/// <inheritdoc />
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

		public ModifierKeys Modifier { get; }
		public Keys Key { get; }
	}
}
