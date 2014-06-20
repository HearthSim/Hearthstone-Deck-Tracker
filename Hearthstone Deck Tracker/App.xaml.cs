using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if (!DEBUG)
            var date = DateTime.Now;
            var fileName = "Crash Reports/" + string.Format("Crash report {0}{1}{2}-{3}{4}", date.Day, date.Month, date.Year, date.Hour, date.Minute);

            if (!Directory.Exists("Crash Reports"))
                Directory.CreateDirectory("Crash Reports");
            
            using (var sr = new StreamWriter(fileName + ".txt", true))
            {
                sr.WriteLine("########## " + DateTime.Now + " ##########");
                sr.WriteLine(e.Exception);
            }

            MessageBox.Show("Something went wrong.\nA crash report file was created at:\n\"" + Environment.CurrentDirectory + "\\" + fileName + "\"\nPlease create an issue in the github, message me on reddit (/u/tnx) or send this file to epikz37@gmail.com, with a short explanation of what you were doing before the crash.", "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Shutdown();
#endif
        }
    }
}
