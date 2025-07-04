﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds;

public static class HDTToolsManager
{
    private const string HDTToolsVersion = "1.0.4";
    private static readonly AssetDownloader<string, byte[]>? Downloader;
    private static readonly string ToolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HDTTools");
    private static readonly string ExtractedToolsDir = Path.Combine(ToolsDir, "bin");
    private const string ZipFileName = "HDTTools.zip";
    private const string ZipUrl = $"https://hdt-downloads-hongkong.s3.ap-east-1.amazonaws.com/HDTTools/HDTTools-v{HDTToolsVersion}.zip";
    internal static string VersionString => $"v{HDTToolsVersion}";

    private const int DownloadMaxRetries = 3;
    private const int DownloadDelaySeconds = 2;

    static HDTToolsManager()
    {
        try
        {
            Downloader = new AssetDownloader<string, byte[]>(
                ToolsDir,
                key => ZipUrl,
                key => ZipFileName,
                data => data,
                alwaysKeepCached: new HashSet<string> { "HDTTools" },
                maxCacheSize: 1
            );
        }
        catch (Exception e)
        {
            Log.Error($"Could not create asset downloader for HDTTools: {e.Message}");
        }
    }

    public static List<string> GetRecentLogs()
    {
	    var logsDir = Path.Combine(
		    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		    "HearthstoneDeckTracker", "HDTTools", "logs");

	    var dirInfo = new DirectoryInfo(logsDir);
	    var logFile = dirInfo.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

	    if (logFile == null)
			return new List<string>();

	    try
	    {
		    var allLines = File.ReadAllLines(logFile.FullName);
		    return allLines.Skip(Math.Max(0, allLines.Length - 100)).ToList();
	    }
	    catch (Exception ex)
	    {
		    Log.Error($"Error reading HDTTools log file: {ex.Message}");
		    return new List<string>();
	    }
    }

    private static bool _loading;
    public static async Task<bool> EnsureLatestHDTTools()
    {
	    if(_loading)
		    return false;
        _loading = true;

        try
        {
            if (Downloader == null)
            {
                Log.Warn("No HDTTools downloader available");
                return false;
            }

            Downloader.InvalidateCachedAssets();

	        LRUCache<byte[]>.Entry? asset = null;

	        for (var attempt = 1; attempt <= DownloadMaxRetries; attempt++)
	        {
	            try
	            {
	                Log.Info($"Downloading HDTTools.zip (attempt {attempt}/{DownloadMaxRetries})...");
	                asset = await Task.Run(async () => await Downloader.GetAssetEntry("HDTTools", true));

	                if (asset?.Data != null)
	                    break;

	                Log.Warn($"Download attempt {attempt} returned null data");
	            }
	            catch (Exception downloadEx)
	            {
	                Log.Warn($"Download attempt {attempt} failed: {downloadEx.Message}");

	                if (attempt >= DownloadMaxRetries)
	                    throw;

	                var delay = TimeSpan.FromSeconds(DownloadDelaySeconds);
	                Log.Info($"Waiting {delay.TotalSeconds}s before retry...");
	                await Task.Delay(delay);
	            }
	        }

	        if (asset?.Data == null)
	        {
	            Log.Warn("Could not download HDTTools.zip after all retries");
	            return false;
			}

            var exePath = Path.Combine(ExtractedToolsDir, "HDTTools.exe");
            var needExtract = !File.Exists(exePath) || !asset.NotModified;

            if (!needExtract)
            {
                Log.Info("HDTTools is up to date, no extraction needed");
                return true;
            }

            Log.Info("Extracting HDTTools.zip...");
	        await Task.Run(() => ExtractHDTToolsZip(asset.Data));
            Log.Info("HDTTools.zip updated and extracted successfully");
            return true;
        }
        catch (Exception ex)
        {
	        // Important to clear cached etag and zip.
	        // This way we make sure all files are present and correct.
	        Downloader?.ClearStorage();
            Log.Error($"Error in EnsureLatestHDTTools: {ex.Message}");
            return false;
        }
        finally
        {
            _loading = false;
        }
    }

    private static async Task ExtractHDTToolsZip(byte[] zipData)
    {
	    ClearExtractedDirectory();

	    using var memoryStream = new MemoryStream(zipData);
	    using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
	    foreach (var entry in archive.Entries)
	    {
		    if (string.IsNullOrEmpty(entry.Name))
			    continue;

		    var destinationPath = Path.Combine(ExtractedToolsDir, entry.FullName);
		    var directoryPath = Path.GetDirectoryName(destinationPath);
		    if (!string.IsNullOrEmpty(directoryPath))
			    Directory.CreateDirectory(directoryPath);

		    try
		    {
			    using var entryStream = entry.Open();
			    using var fileStream = File.Create(destinationPath);
			    await entryStream.CopyToAsync(fileStream);
		    }
		    catch (Exception ex)
		    {
			    Log.Warn($"Failed to extract {entry.FullName}: {ex.Message}");
			    throw;
		    }
	    }
    }

    private static void ClearExtractedDirectory()
    {
        try
        {
            if (Directory.Exists(ExtractedToolsDir))
            {
                Directory.Delete(ExtractedToolsDir, true);
            }
            Directory.CreateDirectory(ExtractedToolsDir);
            Log.Info("Cleared and recreated extracted tools directory");
        }
        catch (Exception ex)
        {
            Log.Error($"Error clearing extracted directory: {ex.Message}");
            throw;
        }
    }
}
