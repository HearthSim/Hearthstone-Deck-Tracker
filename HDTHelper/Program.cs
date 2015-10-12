#region

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Web;

#endregion

namespace HDTHelper
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.Title = "HDT Helper";
			if(args.Length > 0)
			{
				switch(args[0])
				{
					case "registerProtocol":
						HDTProtocol.Register();
						break;
					case "deleteProtocol":
						HDTProtocol.Delete();
						break;
					case "hdt:start":
						StartHdt();
						break;
					case "hdt:sync":
						HdtGeneral("sync");
						break;
				}
				if(args[0].StartsWith("hdt:import="))
				{
					var value = args[0].Substring(11);
					string json = null;
					if(value.StartsWith("%7B")) // encoded '{'
						json = HttpUtility.UrlDecode(value);
					else if(value.StartsWith("http://"))
						json = GetJsonFromUrl(value);
					else if(File.Exists(value))
						json = GetJsonFromFile(value);
					if(json != null)
						HdtImport(json);
				}
			}
		}

		private static void HdtImport(string json)
		{
			StartHdt();
			using(var pipe = new NamedPipeClientStream(".", "hdtimport", PipeDirection.Out, PipeOptions.Asynchronous))
			{
				pipe.Connect();
				using(var sw = new StreamWriter(pipe))
					sw.WriteLine(json);
			}
		}

		private static void HdtGeneral(string message)
		{
			using(var pipe = new NamedPipeClientStream(".", "hdtgeneral", PipeDirection.Out, PipeOptions.Asynchronous))
			{
				pipe.Connect();
				using(var sw = new StreamWriter(pipe))
					sw.Write(message);
			}
		}

		private static void StartHdt()
		{
			if(!Process.GetProcessesByName("Hearthstone Deck Tracker").Any())
			{
				Console.Write("Starting HDT...");
				var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hearthstone Deck Tracker.exe");
				Process.Start(path);
			}
		}

		private static string GetJsonFromFile(string path)
		{
			try
			{
				using(var sr = new StreamReader(path))
					return sr.ReadToEnd();
			}
			catch(Exception)
			{
				return "";
			}
		}

		private static string GetJsonFromUrl(string url)
		{
			try
			{
				using(var wc = new WebClient())
					return wc.DownloadString(url);
			}
			catch(Exception)
			{
				return "";
			}
		}
	}
}