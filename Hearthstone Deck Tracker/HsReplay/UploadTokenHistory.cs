using System;
using System.IO;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class UploadTokenHistory
	{
		private const string FileName = "upload_token_history.txt";
		public static void Write(string data)
		{
			data = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + data;
			try
			{
				var file = Path.Combine(Config.Instance.DataDir, FileName);
				File.AppendAllLines(file, new [] {data});
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
