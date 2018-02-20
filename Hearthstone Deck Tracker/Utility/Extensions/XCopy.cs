using System.Diagnostics;


namespace HearthStone.DeckTracker.Utility.Extensions
{
    public static class XCopy
    {
        /// <summary>
        /// Method to Perform Xcopy to copy files/folders from Source machine to Target Machine
        /// </summary>
        /// <param name="solutionDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void Run(string solutionDirectory, string targetDirectory)
        {
            // Use ProcessStartInfo class
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = "xcopy",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "\"" + solutionDirectory + "\"" + " " + "\"" + targetDirectory + "\"" + @" /e /y /q"
            };

            using (var exeProcess = Process.Start(startInfo))
            {
                exeProcess?.WaitForExit();
            }
        }
    }
}
