#region

#region

// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

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
				sr.WriteLine(Helper.MainWindow.Options.OptionsTrackerLogging.TextBoxLog.Text);
			}

			MessageBox.Show(
			                "A crash report file was created at:\n\"" + Environment.CurrentDirectory + "\\" + fileName
			                + ".txt\"\n\nPlease \na) create an issue on github (https://github.com/Epix37/Hearthstone-Deck-Tracker) \nor \nb) send me an email (epikz37@gmail.com).\n\nPlease include the generated crash report(s) and a short explanation of what you were doing before the crash.",
			                "Oops! Something went wrong.", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			Shutdown();
#endif
		}
	}
}