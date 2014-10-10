﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    internal class Program
    {
        private const string Url = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases/download/v{0}/Hearthstone.Deck.Tracker-v{0}.zip";

        private static void Main(string[] args)
        {
            Console.Title = "Hearthstone Deck Tracker Updater";
            Console.CursorVisible = false;
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments");
                return;
            }
            try
            {
                //wait for tracker to shut down
                Thread.Sleep(1000);

                int procId = int.Parse(args[0]);
                if(Process.GetProcesses().Any(p => p.Id == procId))
                {
                    Process.GetProcessById(procId).Kill();
                    Console.WriteLine("Killed Hearthston Deck Tracker process");
                }
            }
            catch
            {
                return;
            }


            var update = Update(args[1]);
            update.Wait();

        }

        private static async Task Update(string version)
        {
            try
            {
                var filePath = string.Format("temp/v{0}.zip", version);

                Console.WriteLine("Creating temp file directory");
                if(Directory.Exists("temp"))
                    Directory.Delete("temp", true);
                Directory.CreateDirectory("temp");
                
                using(var wc = new WebClient())
                {
                    var lockThis = new object();
                    Console.WriteLine("Downloading latest version... 0%");
                    wc.DownloadProgressChanged += (sender, e) =>
                        {
                            lock(lockThis)
                            {
                                Console.CursorLeft = 0;
                                Console.CursorTop = 1;
                                Console.WriteLine("Downloading latest version... {0}/{1}KB ({2}%)", e.BytesReceived / (1024), e.TotalBytesToReceive / (1024), e.ProgressPercentage);
                            }
                        };
                    await wc.DownloadFileTaskAsync(string.Format(Url, version), filePath);
                }
                File.Move(filePath, filePath.Replace("rar", "zip"));
                Console.WriteLine("Extracting files...");
                ZipFile.ExtractToDirectory(filePath, "temp");
                const string newPath = "temp\\Hearthstone Deck Tracker\\";
                CopyFiles("temp", newPath);
                Console.WriteLine("Cleaning up...");
                Console.WriteLine("Done!");

                Process.Start("Hearthstone Deck Tracker.exe");
            }
            catch
            {
                Console.WriteLine("There was a problem updating to the latest version. Pressing any key will direct you to the manual download.");
                Console.ReadKey();
                Process.Start(@"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases");
            }
            finally
            {
                if(Directory.Exists("temp"))
                    Directory.Delete("temp", true);
            }
        }

        private static void CopyFiles(string dir, string newPath)
        {
            foreach(var subDir in Directory.GetDirectories(dir))
            {
                foreach(var file in Directory.GetFiles(subDir))
                {
                    var newDir = subDir.Replace(newPath, string.Empty);
                    if(!Directory.Exists(newDir))
                        Directory.CreateDirectory(newDir);

                    var newFilePath = file.Replace(newPath, string.Empty);
                    Console.WriteLine("Writing {0}", newFilePath);
                    if(file.Contains("Updater"))
                        File.Copy(file, file.Replace("Updater", "Updater_new"));
                    else
                        File.Copy(file, newFilePath, true);
                }
                CopyFiles(subDir, newPath);
            }
        }
    }
}