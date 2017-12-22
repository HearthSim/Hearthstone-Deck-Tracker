#region

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	//http://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp
	public sealed class KeyboardHook : IDisposable
	{
		private readonly Window _window = new Window();
		private int _currentId;

		public KeyboardHook()
		{
			// register the event of the inner native window.
			_window.KeyPressed += delegate(object sender, KeyPressedEventArgs args)
			{
				KeyPressed?.Invoke(this, args);
			};
		}

		// Registers a hot key with Windows.
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		// Unregisters the hot key with Windows.
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		/// <summary>
		/// Registers a hot key in the system.
		/// </summary>
		/// <param name="modifier">The modifiers that are associated with the hot key.</param>
		/// <param name="key">The key itself that is associated with the hot key.</param>
		public int RegisterHotKey(ModifierKeys modifier, Keys key)
		{
			// increment the counter.
			_currentId = _currentId + 1;

			// register the hot key.
			if(!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
				throw new InvalidOperationException("Couldn’t register the hot key.");

			return _currentId;
		}

		public void UnRegisterHotKey(int id) => UnregisterHotKey(_window.Handle, id);

		/// <summary>
		/// A hot key has been pressed.
		/// </summary>
		public event EventHandler<KeyPressedEventArgs> KeyPressed;

		/// <summary>
		/// Represents the window that is used internally to get the messages.
		/// </summary>
		private class Window : NativeWindow, IDisposable
		{
			private static readonly int WM_HOTKEY = 0x0312;

			public Window()
			{
				// create the handle for the window.
				CreateHandle(new CreateParams());
			}

			#region IDisposable Members

			public void Dispose() => DestroyHandle();

			#endregion

			/// <summary>
			/// Overridden to get the notifications.
			/// </summary>
			/// <param name="m"></param>
			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);

				// check if we got a hot key pressed.
				if(m.Msg == WM_HOTKEY)
				{
					// get the keys.
					var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
					var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

					// invoke the event to notify the parent.
					KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
				}
			}

			public event EventHandler<KeyPressedEventArgs> KeyPressed;
		}

		#region IDisposable Members

		private bool _disposed;

		public void Dispose()
		{
			if(_disposed)
				return;
			// unregister all the registered hot keys.
			for(var i = _currentId; i > 0; i--)
				UnregisterHotKey(_window.Handle, i);

			// dispose the inner native window.
			_window.Dispose();
			_disposed = true;
		}

		~KeyboardHook()
		{
			Dispose();
		}

		#endregion
	}
}
