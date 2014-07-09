using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Hearthstone_Deck_Tracker
{
    internal class User32
    {
        private const int WsExTransparent = 0x00000020;
        private const int GwlExstyle = (-20);
        public const int SwRestore = 9;

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

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
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            int extendedStyle = GetWindowLong(hwnd, GwlExstyle);
            SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExTransparent);
        }

        public static bool IsForegroundWindow(String lpWindowName)
        {
            return GetForegroundWindow() == FindWindow("UnityWndClass", lpWindowName);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [Flags]
        public enum MouseEventFlags : uint
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct MousePoint
        {
            public int X;
            public int Y;
        }
        public static Point GetMousePos()
        {
            var p = new MousePoint();
            GetCursorPos(out p);
            return new Point(p.X, p.Y);
        }
        public static Rectangle GetHearthstoneRect(bool dpiScaling)
        {
            var rect = new Rect();
            GetWindowRect(FindWindow("UnityWndClass", "Hearthstone"), ref rect);
            if (dpiScaling)
            {
                rect.top = (int) (rect.top/Helper.DpiScalingY);
                rect.bottom = (int) (rect.bottom/Helper.DpiScalingY);
                rect.left = (int) (rect.left/Helper.DpiScalingX);
                rect.right = (int) (rect.right/Helper.DpiScalingX);
            }
            return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

    }
}