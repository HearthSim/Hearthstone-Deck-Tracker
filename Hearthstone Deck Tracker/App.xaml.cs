#region

#region

// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Windows;

#endregion

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static bool _createdReport;

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if(e.Exception is MissingMethodException || e.Exception is MissingFieldException || e.Exception is MissingMemberException || e.Exception is TypeLoadException)
			{
				var plugin =
					PluginManager.Instance.Plugins.FirstOrDefault(p => new FileInfo(p.FileName).Name.Replace(".dll", "") == e.Exception.Source);
				if(plugin != null)
				{
					plugin.IsEnabled = false;
					var header = $"{plugin.NameAndVersion} is not compatible with HDT {Helper.GetCurrentVersion().ToVersionString()}.";
					ErrorManager.AddError(header, "Make sure you are using the latest version of the Plugin and HDT.\n\n" + e.Exception);
					e.Handled = true;
					return;
				}
			}
			if(!_createdReport)
			{
				_createdReport = true;
				new CrashDialog(e.Exception).ShowDialog();
#if(!DEBUG)
				var date = DateTime.Now;
				var fileName = "Crash Reports\\" + $"Crash report {date.Day}{date.Month}{date.Year}-{date.Hour}{date.Minute}";

				if(!Directory.Exists("Crash Reports"))
					Directory.CreateDirectory("Crash Reports");

				using(var sr = new StreamWriter(fileName + ".txt", true))
				{
					sr.WriteLine("########## " + DateTime.Now + " ##########");
					sr.WriteLine(e.Exception);
					sr.WriteLine(Core.MainWindow.Options.OptionsTrackerLogging.TextBoxLog.Text);
				}
#endif
				e.Handled = true;
				Shutdown();
			}
			e.Handled = true;
		}

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			Core.Initialize();
		}
	}
}