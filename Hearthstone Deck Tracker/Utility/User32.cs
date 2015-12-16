﻿#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class User32
	{
		[Flags]
		public enum MouseEventFlags : uint
		{
			LeftDown = 0x00000002,
			LeftUp = 0x00000004,
			RightDown = 0x00000008,
			RightUp = 0x00000010,
			Wheel = 0x00000800
		}

		public const int WsExTransparent = 0x00000020;
		public const int WsExToolWindow = 0x00000080;
		private const int GwlExstyle = (-20);
		public const int SwRestore = 9;
		private const int Alt = 0xA4;
		private const int ExtendedKey = 0x1;
		private const int KeyUp = 0x2;
		private static DateTime _lastCheck;
		private static IntPtr _hsWindow;

		[DllImport("user32.dll")]
		public static extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);

		[DllImport("user32.dll")]
		public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport("user32.dll")]
		private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

		[DllImport("user32.dll")]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		public static void SetWindowExStyle(IntPtr hwnd, int style)
		{
			var extendedStyle = GetWindowLong(hwnd, GwlExstyle);
			SetWindowLong(hwnd, GwlExstyle, extendedStyle | style);
		}

		public static bool IsHearthstoneInForeground()
		{
			return GetForegroundWindow() == GetHearthstoneWindow();
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out MousePoint lpPoint);

		public static Point GetMousePos()
		{
			MousePoint p;
			GetCursorPos(out p);
			return new Point(p.X, p.Y);
		}

		private static readonly string[] WindowNames = { "Hearthstone", "하스스톤", "《爐石戰記》", "炉石传说" };

		public static IntPtr GetHearthstoneWindow()
		{
			if(DateTime.Now - _lastCheck < new TimeSpan(0, 0, 5) && _hsWindow == IntPtr.Zero)
				return _hsWindow;
			if(_hsWindow != IntPtr.Zero && IsWindow(_hsWindow))
				return _hsWindow;
			if(Config.Instance.UseAnyUnityWindow)
			{
				foreach(var process in Process.GetProcesses())
				{
					var sb = new StringBuilder(200);
					GetClassName(process.MainWindowHandle, sb, 200);
					if(sb.ToString().Equals("UnityWndClass", StringComparison.InvariantCultureIgnoreCase))
					{
						_hsWindow = process.MainWindowHandle;
						break;
					}
                }
            }
			else
			{
				_hsWindow = FindWindow("UnityWndClass", Config.Instance.HearthstoneWindowName);
				if(_hsWindow != IntPtr.Zero)
					return _hsWindow;
				foreach(var windowName in WindowNames)
				{
					_hsWindow = FindWindow("UnityWndClass", windowName);
					if(_hsWindow != IntPtr.Zero)
					{
						if(Config.Instance.HearthstoneWindowName != windowName)
						{
							Config.Instance.HearthstoneWindowName = windowName;
							Config.Save();
						}
						break;
					}
				}
			}
			_lastCheck = DateTime.Now;
			return _hsWindow;
		}

		public static Rectangle GetHearthstoneRect(bool dpiScaling)
		{
			// Returns the co-ordinates of Hearthstone's client area in screen co-ordinates
			var hsHandle = GetHearthstoneWindow();
			var rect = new Rect();
			var ptUL = new Point();
			var ptLR = new Point();

			GetClientRect(hsHandle, ref rect);

			ptUL.X = rect.left;
			ptUL.Y = rect.top;

			ptLR.X = rect.right;
			ptLR.Y = rect.bottom;

			ClientToScreen(hsHandle, ref ptUL);
			ClientToScreen(hsHandle, ref ptLR);

			if(dpiScaling)
			{
				ptUL.X = (int)(ptUL.X / Helper.DpiScalingX);
				ptUL.Y = (int)(ptUL.Y / Helper.DpiScalingY);
				ptLR.X = (int)(ptLR.X / Helper.DpiScalingX);
				ptLR.Y = (int)(ptLR.Y / Helper.DpiScalingY);
			}

			return new Rectangle(ptUL.X, ptUL.Y, ptLR.X - ptUL.X, ptLR.Y - ptUL.Y);
		}

		public static void BringHsToForeground()
		{
			var hsHandle = GetHearthstoneWindow();
			ActivateWindow(hsHandle);
			SetForegroundWindow(hsHandle);
		}

		public static void FlashHs()
		{
			var hsHandle = GetHearthstoneWindow();
			FlashWindow(hsHandle, false);
		}

		//http://www.roelvanlisdonk.nl/?p=4032
		public static void ActivateWindow(IntPtr mainWindowHandle)
		{
			// Guard: check if window already has focus.
			if(mainWindowHandle == GetForegroundWindow()) return;

			// Show window maximized.
			ShowWindow(mainWindowHandle, SwRestore);

			// Simulate an "ALT" key press.
			keybd_event(Alt, 0x45, ExtendedKey | 0, 0);

			// Simulate an "ALT" key release.
			keybd_event(Alt, 0x45, ExtendedKey | KeyUp, 0);

			// Show window in forground.
			SetForegroundWindow(mainWindowHandle);
		}



		//http://joelabrahamsson.com/detecting-mouse-and-keyboard-input-with-net/
		public class MouseInput : IDisposable
		{
			private const Int32 WH_MOUSE_LL = 14;
			private const Int32 WM_LBUTTONDOWN = 0x201;
			private const Int32 WM_LBUTTONUP = 0x0202;
			private readonly WindowsHookHelper.HookDelegate _mouseDelegate;
			private readonly IntPtr _mouseHandle;
			private bool _disposed;

			public MouseInput()
			{
				_mouseDelegate = MouseHookDelegate;
				_mouseHandle = WindowsHookHelper.SetWindowsHookEx(WH_MOUSE_LL, _mouseDelegate, IntPtr.Zero, 0);
			}

			public void Dispose()
			{
				Dispose(true);
			}

			public event EventHandler<EventArgs> LmbDown;
			public event EventHandler<EventArgs> LmbUp;
			public event EventHandler<EventArgs> MouseMoved;

			private IntPtr MouseHookDelegate(Int32 code, IntPtr wParam, IntPtr lParam)
			{
				if(code < 0)
					return WindowsHookHelper.CallNextHookEx(_mouseHandle, code, wParam, lParam);


				switch(wParam.ToInt32())
				{
					case WM_LBUTTONDOWN:
						if(LmbDown != null)
							LmbDown(this, new EventArgs());
						break;
					case WM_LBUTTONUP:
						if(LmbUp != null)
							LmbUp(this, new EventArgs());
						break;
					default:
						if(MouseMoved != null)
							MouseMoved(this, new EventArgs());
						break;
				}

				return WindowsHookHelper.CallNextHookEx(_mouseHandle, code, wParam, lParam);
			}

			protected virtual void Dispose(bool disposing)
			{
				if(!_disposed)
				{
					if(_mouseHandle != IntPtr.Zero)
						WindowsHookHelper.UnhookWindowsHookEx(_mouseHandle);

					_disposed = true;
				}
			}

			~MouseInput()
			{
				Dispose(false);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MousePoint
		{
			public readonly int X;
			public readonly int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Rect
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		public class WindowsHookHelper
		{
			public delegate IntPtr HookDelegate(Int32 code, IntPtr wParam, IntPtr lParam);

			[DllImport("User32.dll")]
			public static extern IntPtr CallNextHookEx(IntPtr hHook, Int32 nCode, IntPtr wParam, IntPtr lParam);

			[DllImport("User32.dll")]
			public static extern IntPtr UnhookWindowsHookEx(IntPtr hHook);

			[DllImport("User32.dll")]
			public static extern IntPtr SetWindowsHookEx(Int32 idHook, HookDelegate lpfn, IntPtr hmod, Int32 dwThreadId);
		}
	}
}