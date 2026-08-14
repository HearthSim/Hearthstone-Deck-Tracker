using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;
using CreateParams = System.Windows.Forms.CreateParams;
using Message = System.Windows.Forms.Message;
using NativeWindow = System.Windows.Forms.NativeWindow;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal static class SingleInstance
	{
		private const uint SendTimeout = 20000;
		private const int StartupMutexTimeout = 5000;
		private const int ErrorTimeout = 1460;

		[DllImport("user32.dll")]
		private static extern bool AllowSetForegroundWindow(int dwProcessId);

		private static readonly uint ActivateMessage = User32.RegisterWindowMessage("HearthstoneDeckTracker_Activate");

		private static MessageWindow? _messageWindow;

		/// <summary>
		/// Returns false if another instance using the same data directory is already running and this one should
		/// shut down. That instance is asked to come to the foreground. Any failure to determine this lets us start,
		/// because refusing to launch is worse than briefly running twice.
		/// </summary>
		public static bool TryClaim(Action onSecondInstance)
		{
			if(ActivateMessage == 0)
			{
				Log.Error("Could not register the activation message");
				return true;
			}

			IntPtr running;
			try
			{
				if(TryCreateMessageWindow(onSecondInstance, out running))
					return true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return true;
			}

			NotifyRunningInstance(running);
			return false;
		}

		private static bool TryCreateMessageWindow(Action onSecondInstance, out IntPtr running)
		{
			var title = GetWindowTitle();

			// the window only exists once we create it, so without this two instances launching at the same time
			// could both look, find nothing and claim
			using var startupMutex = new Mutex(false, GetStartupMutexName());
			var acquired = false;
			try
			{
				try
				{
					acquired = startupMutex.WaitOne(StartupMutexTimeout);
				}
				catch(AbandonedMutexException)
				{
					// the previous owner died before it could create its window
					acquired = true;
				}

				running = User32.FindWindowEx(User32.HwndMessage, IntPtr.Zero, null, title);
				if(running != IntPtr.Zero)
					return false;

				_messageWindow = new MessageWindow(title, onSecondInstance);
				return true;
			}
			finally
			{
				if(acquired)
					startupMutex.ReleaseMutex();
			}
		}

		private static void NotifyRunningInstance(IntPtr hwnd)
		{
			try
			{
				User32.GetWindowThreadProcessId(hwnd, out var processId);

				// let the running instance take the foreground away from us
				AllowSetForegroundWindow((int)processId);

				var sent = User32.SendMessageTimeout(hwnd, ActivateMessage, IntPtr.Zero, IntPtr.Zero,
					User32.SmtoAbortIfHung, SendTimeout, out var result);
				if(sent != IntPtr.Zero && result != IntPtr.Zero)
					return;

				var error = Marshal.GetLastWin32Error();
				OnRunningInstanceUnreachable(error == ErrorTimeout ? "timeout" : $"error {error}");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				OnRunningInstanceUnreachable(ex.GetType().Name);
			}
		}

		private static void OnRunningInstanceUnreachable(string reason)
		{
			// our own log file belongs to the instance we could not reach, so this only goes to sentry
			SentryReporter.CaptureSingleInstanceProblem(reason);
			MessageBox.Show("Hearthstone Deck Tracker is already running, but it is not responding. Please close it "
				+ "using the task manager and try again.", "Error starting Hearthstone Deck Tracker",
				MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private static string GetWindowTitle() => "HearthstoneDeckTracker_" + GetDataDirHash();

		private static string GetStartupMutexName() => @"Local\HearthstoneDeckTracker_" + GetDataDirHash() + "_Startup";

		private static string GetDataDirHash()
		{
			using var sha = SHA1.Create();
			var dataDir = Path.GetFullPath(Config.Instance.DataDir).ToLowerInvariant();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(dataDir));
			return BitConverter.ToString(hash).Replace("-", "");
		}

		private class MessageWindow : NativeWindow
		{
			private readonly Action _onSecondInstance;

			public MessageWindow(string title, Action onSecondInstance)
			{
				_onSecondInstance = onSecondInstance;
				CreateHandle(new CreateParams { Caption = title, Parent = User32.HwndMessage });
			}

			protected override void WndProc(ref Message m)
			{
				if(m.Msg == ActivateMessage)
				{
					try
					{
						_onSecondInstance();
						m.Result = (IntPtr)1;
					}
					catch(Exception ex)
					{
						Log.Error(ex);
						m.Result = IntPtr.Zero;
					}
					return;
				}
				base.WndProc(ref m);
			}
		}
	}
}
