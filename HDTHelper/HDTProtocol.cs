#region

using System;
using System.Linq;
using Microsoft.Win32;

#endregion

namespace HDTHelper
{
	public class HDTProtocol
	{
		private static string Scheme
		{
			get { return "hdt"; }
		}

		private static string Path
		{
			get { return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HDTHelper.exe"); }
		}

		internal static bool IsRegistered
		{
			get { return Registry.ClassesRoot.GetSubKeyNames().Any(x => x == Scheme); }
		}

		internal static void Register()
		{
			try
			{
				var key = Registry.ClassesRoot.CreateSubKey(Scheme);
				key.CreateSubKey("DefaultIcon").SetValue("", System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HDTHelper.exe"));
				key.SetValue("", "URL: hdt Protocol Handler");
				key.SetValue("URL Protocol", "");
				key.CreateSubKey(@"shell\open\command").SetValue("", Path + " %1");
			}
			catch(Exception ex)
			{
			}
		}

		internal static void Delete()
		{
			if(IsRegistered)
			{
				try
				{
					Registry.ClassesRoot.DeleteSubKeyTree(Scheme);
				}
				catch(Exception)
				{
				}
			}
		}

		public static bool Verify()
		{
			var key = Registry.ClassesRoot.OpenSubKey(Scheme);
			if(key == null)
				return false;
			var path = key.OpenSubKey(@"shell\open\command").GetValue("").ToString();
			return path.StartsWith(Path);
		}
	}
}