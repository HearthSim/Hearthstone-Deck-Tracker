using System;
using System.IO;
using System.IO.Compression;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
  public static class ZipArchiveExtensions
	{
		//http://stackoverflow.com/questions/14795197/forcefully-replacing-existing-files-during-extracting-file-using-system-io-compr
		public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
		{
			if(!overwrite)
			{
				archive.ExtractToDirectory(destinationDirectoryName, false);
				return;
			}
			foreach(var file in archive.Entries)
			{
				var completeFileName = Path.Combine(destinationDirectoryName, file. FullName);
				if(file.Name == "")
				{
					// Assuming Empty for Directory
					Directory.CreateDirectory(Path.GetDirectoryName(completeFileName) ?? throw new InvalidOperationException());
					continue;
				}
				file.ExtractToFile(completeFileName, true);
			}
		}
	}
}
