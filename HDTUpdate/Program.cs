using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HDTUpdate
{
	internal class Program
	{
		private static UpdatingState _state;
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
					Console.WriteLine("Killed Hearthstone Deck Tracker process");
				}
			}
			catch
			{
				return;
			}

			try
			{
				var update = Update(args[1]);
				update.Wait();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				switch(_state)
				{
					case UpdatingState.Preparation:
						Console.WriteLine("Please delete the 'temp' directory and try to update again. Press any key to exit.");
						Console.ReadKey();
						break;
					case UpdatingState.Downloading:
						Console.WriteLine("There was an error downloading the latest update. Press any key to open the website for manual download.");
						Console.ReadKey();
						Process.Start(@"https://github.com/HearthSim/Hearthstone-Deck-Tracker/releases");
						break;
					case UpdatingState.Extracting:
						Console.WriteLine("There was an error installing the latest update. Press any key to open the website for manual download.");
						Console.ReadKey();
						Process.Start(@"https://github.com/HearthSim/Hearthstone-Deck-Tracker/releases");
						break;
					case UpdatingState.Starting:
						Console.WriteLine("There was an error re-starting HDT. You should be able to start it manually. Press any key to exit.");
						Console.ReadKey();
						break;
				}
			}
			finally
			{
				try
				{
					Console.WriteLine("Cleaning up...");
					if(Directory.Exists("temp"))
						Directory.Delete("temp", true);
					Console.WriteLine("Done!");
				}
				catch
				{
					Console.WriteLine("Failed to delete temp file directory");
				}
			}
		}

		private static async Task Update(string url)
		{
			var fileName = url.Split('/').LastOrDefault() ?? "tmp.zip";
			var filePath = Path.Combine("temp", fileName);
			try
			{
				Console.WriteLine("Creating temp file directory");
				if(Directory.Exists("temp"))
					Directory.Delete("temp", true);
				Directory.CreateDirectory("temp");
			}
			catch(Exception e)
			{
				throw new Exception("Error creating/clearing the download directory.", e);
			}
			_state = UpdatingState.Downloading;
			try
			{
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
					await wc.DownloadFileTaskAsync(url, filePath);
				}
			}
			catch(Exception e)
			{
				throw new Exception("Error download the file.", e);
			}
			_state = UpdatingState.Extracting;
			try
			{
				File.Move(filePath, filePath.Replace("rar", "zip"));
				Console.WriteLine("Extracting files...");
				ZipFile.ExtractToDirectory(filePath, "temp");
				const string newPath = "temp\\Hearthstone Deck Tracker\\";
				CopyFiles("temp", newPath);
			}
			catch(Exception e)
			{
				throw new Exception("Error extracting the downloaded file.", e);
			}
			_state = UpdatingState.Starting;
			try
			{
				Process.Start("Hearthstone Deck Tracker.exe");
			}
			catch(Exception e)
			{
				throw new Exception("Error restarting HDT.", e);
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
					if(file.Contains("HDTUpdate.exe"))
						File.Copy(file, newFilePath.Replace("HDTUpdate.exe", "HDTUpdate_new.exe"));
					else
						File.Copy(file, newFilePath, true);
				}
				CopyFiles(subDir, newPath);
			}
		}
	}

	public enum UpdatingState
	{
		Preparation,
		Downloading,
		Extracting,
		Starting
	}
}