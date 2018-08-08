using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class Kernel32
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint flags, StringBuilder text,
			ref uint size);

		public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags) => OpenProcess(flags, false, proc.Id);

		public static string GetProcessExePath(Process process)
		{
			var handle = OpenProcess(process, ProcessAccessFlags.QueryLimitedInformation);
			if(handle == IntPtr.Zero)
				ThrowLastWin32Error();
			try
			{
				return GetProcessExePath(handle);
			}
			finally
			{
				CloseHandle(handle);
			}
		}

		public static string GetProcessExePath(IntPtr hProcess)
		{
			var capacity = 1024u;
			var sb = new StringBuilder((int)capacity);
			if(!QueryFullProcessImageName(hProcess, 0, sb, ref capacity))
				ThrowLastWin32Error();
			return sb.ToString(0, (int)capacity);
		}

		public static void ThrowLastWin32Error() => throw new Win32Exception(Marshal.GetLastWin32Error());
	}

	[Flags]
	public enum ProcessAccessFlags
	{
		All = 0x001F0FFF,
		Terminate = 0x00000001,
		CreateThread = 0x00000002,
		VirtualMemoryOperation = 0x00000008,
		VirtualMemoryRead = 0x00000010,
		VirtualMemoryWrite = 0x00000020,
		DuplicateHandle = 0x00000040,
		CreateProcess = 0x000000080,
		SetQuota = 0x00000100,
		SetInformation = 0x00000200,
		QueryInformation = 0x00000400,
		QueryLimitedInformation = 0x00001000,
		Synchronize = 0x00100000
	}
}
