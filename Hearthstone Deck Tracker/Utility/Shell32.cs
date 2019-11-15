using System.Runtime.InteropServices;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class Shell32
	{
		[DllImport("shell32.dll", SetLastError = true)]
		public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId);		
	}
}
