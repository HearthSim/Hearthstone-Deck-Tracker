#region

#region

// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Garlic;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility;

#endregion

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if(e.Exception is MissingMethodException || e.Exception is TypeLoadException)
			{
				var plugin = Plugins.PluginManager.Instance.Plugins.FirstOrDefault(p => new FileInfo(p.FileName).Name.Replace(".dll", "") == e.Exception.Source);
				if(plugin != null)
				{
					plugin.IsEnabled = false;
					var header = string.Format("{0} is not compatible with HDT {1}.", plugin.NameAndVersion,
					                           Helper.GetCurrentVersion().ToVersionString());
					ErrorManager.AddError(header, "Make sure you are using the latest version of the Plugin and HDT.\n\n" + e.Exception);
					e.Handled = true;
					return;
				}
			}

			Analytics.Analytics.TrackEvent("UnhandledException", e.Exception.GetType().ToString().Split('.').Last(), e.Exception.TargetSite.ToString());
#if (!DEBUG)
			var date = DateTime.Now;
			var fileName = "Crash Reports\\"
			               + string.Format("Crash report {0}{1}{2}-{3}{4}", date.Day, date.Month, date.Year, date.Hour, date.Minute);

			if(!Directory.Exists("Crash Reports"))
				Directory.CreateDirectory("Crash Reports");

			using(var sr = new StreamWriter(fileName + ".txt", true))
			{
				sr.WriteLine("########## " + DateTime.Now + " ##########");
				sr.WriteLine(e.Exception);
				sr.WriteLine(Core.MainWindow.Options.OptionsTrackerLogging.TextBoxLog.Text);
			}

			MessageBox.Show(
			                "A crash report file was created at:\n\"" + Environment.CurrentDirectory + "\\" + fileName
			                + ".txt\"\n\nPlease \na) create an issue on github (https://github.com/Epix37/Hearthstone-Deck-Tracker) \nor \nb) send an email to support@hsdecktracker.net.\n\nPlease include the generated crash report(s) and a short explanation of what you were doing before the crash.",
			                "Oops! Something went wrong.", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			Shutdown();
#endif
		}

	    private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Core.Initialize();
        }
	}
}