#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.FlyoutControls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using MediaColor = System.Windows.Media.Color;
using Region = Hearthstone_Deck_Tracker.Enums.Region;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class Helper
	{
		public static double DpiScalingX = 1.0, DpiScalingY = 1.0;

		public static readonly Dictionary<string, string> LanguageDict = new Dictionary<string, string>
		{
			{"English", "enUS"},
			{"简体中文", "zhCN"},
			{"繁體中文", "zhTW"},
			{"English (Great Britain)", "enGB"},
			{"French", "frFR"},
			{"German", "deDE"},
			{"Italian", "itIT"},
			{"Japanese", "jaJP"},
			{"Korean", "koKR"},
			{"Polish", "plPL"},
			{"Portuguese (Brazil)", "ptBR"},
			{"Russian", "ruRU"},
			{"Spanish (Mexico)", "esMX"},
			{"Spanish (Spain)", "esES"},
			{"Thai", "thTH"}
		};

		public static readonly List<string> LatinLanguages = new List<string>
		{
			"enUS",
			"enGB",
			"frFR",
			"deDE",
			"itIT",
			"ptBR",
			"esMX",
			"esES"
		};

		public static string[] WildOnlySets = new[]
		{
			CardSet.BRM, CardSet.LOE, CardSet.TGT, CardSet.HOF,
			CardSet.FP1, CardSet.PE1, CardSet.PROMO,
			CardSet.KARA, CardSet.OG, CardSet.GANGS,
			CardSet.UNGORO, CardSet.ICECROWN, CardSet.LOOTAPALOOZA
		}.Select(HearthDbConverter.SetConverter).ToArray();

		private static bool? _hearthstoneDirExists;

		public static Dictionary<string, MediaColor> ClassicClassColors = new Dictionary<string, MediaColor>
		{
			{"Druid", MediaColor.FromArgb(0xFF, 0xFF, 0x7D, 0x0A)}, //#FF7D0A, 
			{"Death Knight", MediaColor.FromArgb(0xFF, 0xC4, 0x1F, 0x3B)}, //#C41F3B,
			{"Hunter", MediaColor.FromArgb(0xFF, 0xAB, 0xD4, 0x73)}, //#ABD473,
			{"Mage", MediaColor.FromArgb(0xFF, 0x69, 0xCC, 0xF0)}, //#69CCF0,
			{"Monk", MediaColor.FromArgb(0xFF, 0x00, 0xFF, 0x96)}, //#00FF96,
			{"Paladin", MediaColor.FromArgb(0xFF, 0xF5, 0x8C, 0xBA)}, //#F58CBA,
			{"Priest", MediaColor.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)}, //#FFFFFF,
			{"Rogue", MediaColor.FromArgb(0xFF, 0xFF, 0xF5, 0x69)}, //#FFF569,
			{"Shaman", MediaColor.FromArgb(0xFF, 0x00, 0x70, 0xDE)}, //#0070DE,
			{"Warlock", MediaColor.FromArgb(0xFF, 0x94, 0x82, 0xC9)}, //#9482C9,
			{"Warrior", MediaColor.FromArgb(0xFF, 0xC7, 0x9C, 0x6E)} //#C79C6E
		};

		public static Dictionary<string, MediaColor> HearthStatsClassColors = new Dictionary<string, MediaColor>
		{
			{"Druid", MediaColor.FromArgb(0xFF, 0x62, 0x31, 0x13)}, //#623113,
			{"Death Knight", MediaColor.FromArgb(0xFF, 0xC4, 0x1F, 0x3B)}, //#C41F3B,
			{"Hunter", MediaColor.FromArgb(0xFF, 0x20, 0x8D, 0x43)}, //#208D43,
			{"Mage", MediaColor.FromArgb(0xFF, 0x25, 0x81, 0xBC)}, //#2581BC,
			{"Monk", MediaColor.FromArgb(0xFF, 0x00, 0xFF, 0x96)}, //#00FF96,
			{"Paladin", MediaColor.FromArgb(0xFF, 0xFB, 0xD7, 0x07)}, //#FBD707,
			{"Priest", MediaColor.FromArgb(0xFF, 0xA3, 0xB2, 0xB2)}, //#A3B2B2,
			{"Rogue", MediaColor.FromArgb(0xFF, 0x2F, 0x2C, 0x27)}, //#2F2C27,
			{"Shaman", MediaColor.FromArgb(0xFF, 0x28, 0x32, 0x73)}, //#283273,
			{"Warlock", MediaColor.FromArgb(0xFF, 0x4F, 0x26, 0x69)}, //#4F2669,
			{"Warrior", MediaColor.FromArgb(0xFF, 0xB3, 0x20, 0x25)} //#B32025
		};

		public static OptionsMain OptionsMain { get; set; }

		public static bool UseLatinFont() => LatinLanguages.Contains(Config.Instance.SelectedLanguage) || Config.Instance.NonLatinUseDefaultFont == false;

		public static bool HearthstoneDirExists
		{
			get
			{
				if(!_hearthstoneDirExists.HasValue)
					_hearthstoneDirExists = FindHearthstoneDir();
				return _hearthstoneDirExists.Value;
			}
		}

		public static int CurrentSeason => (DateTime.Now.Year - 2014) * 12 - 3 + DateTime.Now.Month;

		public static WindowState GameWindowState { get; internal set; } = User32.GetHearthstoneWindowState();

		public static Version GetCurrentVersion() => Assembly.GetExecutingAssembly().GetName().Version;

		public static bool IsHex(IEnumerable<char> chars)
			=> chars.All(c => ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')));

		public static double DrawProbability(int copies, int deck, int draw)
			=> 1 - (BinomialCoefficient(deck - copies, draw) / BinomialCoefficient(deck, draw));

		public static double BinomialCoefficient(int n, int k)
		{
			double result = 1;
			for(var i = 1; i <= k; i++)
			{
				result *= n - (k - i);
				result /= i;
			}
			return result;
		}

		public static string ShowSaveFileDialog(string filename, string ext)
		{
			var defaultExt = $"*.{ext}";
			var saveFileDialog = new SaveFileDialog
			{
				FileName = filename,
				DefaultExt = defaultExt,
				Filter = $"{ext.ToUpper()} ({defaultExt})|{defaultExt}"
			};
			return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
		}

		public static string GetValidFilePath(string dir, string name, string extension)
		{
			var validDir = RemoveInvalidPathChars(dir);
			if(!Directory.Exists(validDir))
				Directory.CreateDirectory(validDir);

			if(!extension.StartsWith("."))
				extension = "." + extension;

			var path = validDir + "\\" + RemoveInvalidFileNameChars(name);
			if(File.Exists(path + extension))
			{
				var num = 1;
				while(File.Exists(path + "_" + num + extension))
					num++;
				path += "_" + num;
			}

			return path + extension;
		}

		public static string RemoveInvalidPathChars(string s) => RemoveChars(s, Path.GetInvalidPathChars());
		public static string RemoveInvalidFileNameChars(string s) => RemoveChars(s, Path.GetInvalidFileNameChars());
		public static string RemoveChars(string s, char[] c) => new Regex($"[{Regex.Escape(new string(c))}]").Replace(s, "");

		public static void SortCardCollection(IEnumerable collection, bool classFirst)
		{
			if(collection == null)
				return;
			var view1 = (CollectionView)CollectionViewSource.GetDefaultView(collection);
			view1.SortDescriptions.Clear();

			if(classFirst)
				view1.SortDescriptions.Add(new SortDescription(nameof(Card.IsClassCard), ListSortDirection.Descending));

			view1.SortDescriptions.Add(new SortDescription(nameof(Card.HideStats), ListSortDirection.Descending));
			view1.SortDescriptions.Add(new SortDescription(nameof(Card.Cost), ListSortDirection.Ascending));
			view1.SortDescriptions.Add(new SortDescription(nameof(Card.LocalizedName), ListSortDirection.Ascending));
		}


		public static string DeckToIdString(Deck deck)
			=> deck.GetSelectedDeckVersion().Cards.Aggregate("", (current, card) => current + (card.Id + ":" + card.Count + ";"));

		public static void UpdateEverything(GameV2 game)
		{
			if(Core.Overlay.IsVisible || Core.Windows.CapturableOverlay != null)
				Core.Overlay.Update(false);

			var gameStarted = !game.IsInMenu && game.SetupDone && game.Player.PlayerEntities.Any();
			if(Core.Windows.PlayerWindow.IsVisible)
				Core.Windows.PlayerWindow.SetCardCount(game.Player.HandCount, !gameStarted ? 30 : game.Player.DeckCount);

			if(Core.Windows.OpponentWindow.IsVisible)
				Core.Windows.OpponentWindow.SetOpponentCardCount(game.Opponent.HandCount, !gameStarted ? 30 : game.Opponent.DeckCount, game.Opponent.HasCoin);
		}

		//http://stackoverflow.com/questions/23927702/move-a-folder-from-one-drive-to-another-in-c-sharp
		public static void CopyFolder(string sourceFolder, string destFolder)
		{
			if(!Directory.Exists(destFolder))
				Directory.CreateDirectory(destFolder);
			var files = Directory.GetFiles(sourceFolder);
			foreach(var file in files)
			{
				var name = Path.GetFileName(file);
				var dest = Path.Combine(destFolder, name);
				File.Copy(file, dest);
			}
			var folders = Directory.GetDirectories(sourceFolder);
			foreach(var folder in folders)
			{
				var name = Path.GetFileName(folder);
				var dest = Path.Combine(destFolder, name);
				CopyFolder(folder, dest);
			}
		}

		//http://stackoverflow.com/questions/3769457/how-can-i-remove-accents-on-a-string
		public static string RemoveDiacritics(string src, bool compatNorm)
		{
			var sb = new StringBuilder();
			foreach(var c in src.Normalize(compatNorm ? NormalizationForm.FormKD : NormalizationForm.FormD))
			{
				switch(CharUnicodeInfo.GetUnicodeCategory(c))
				{
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.SpacingCombiningMark:
					case UnicodeCategory.EnclosingMark:
						break;
					default:
						sb.Append(c);
						break;
				}
			}

			return sb.ToString();
		}

		public static T DeepClone<T>(T obj)
		{
			using(var ms = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;
				return (T)formatter.Deserialize(ms);
			}
		}

		public static DateTime FromUnixTime(long unixTime)
			=> new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(unixTime)).ToLocalTime();

		public static DateTime FromUnixTime(string unixTime)
			=> long.TryParse(unixTime, out var time) ? FromUnixTime(time) : DateTime.Now;

		public static Rectangle GetHearthstoneRect(bool dpiScaling) => User32.GetHearthstoneRect(dpiScaling);

		public static string ParseDeckNameTemplate(string template) => ParseDeckNameTemplate(template, null);

		public static string ParseDeckNameTemplate(string template, Deck deck)
		{
			try
			{
				var result = template;
				const string dateRegex = "{Date (?<date>(.*?))}";
				var match = Regex.Match(template, dateRegex);
				if(match.Success)
				{
					var date = DateTime.Now.ToString(match.Groups["date"].Value);
					result = Regex.Replace(result, dateRegex, date);
				}
				const string classRegex = "{Class}";
				match = Regex.Match(template, classRegex);
				if(match.Success)
					result = Regex.Replace(result, classRegex, deck?.Class ?? "");
				return result;
			}
			catch
			{
				return template;
			}
		}

		public static async Task<Region> GetCurrentRegion()
		{
			for(var i = 0; i < 10; i++)
			{
				var accId = HearthMirror.Reflection.GetAccountId();
				if(accId != null)
				{
					var region = GetRegion(accId.Hi);
					Log.Info("Region: " + region);
					return region;
				}
				await Task.Delay(2000);
			}
			return Region.UNKNOWN;
		}

		public static Region GetRegion(ulong accountHi) => (Region)((accountHi >> 32) & 0xFF);

		private static bool FindHearthstoneDir()
		{
			if(string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory)
			   || !File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using(var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone"))
				{
					if(hsDirKey == null)
						return false;
					var hsDir = (string)hsDirKey.GetValue("InstallLocation");

					//verify the install location actually is correct (possibly moved?)
					if(!File.Exists(hsDir + @"\Hearthstone.exe"))
						return false;
					Config.Instance.HearthstoneDirectory = hsDir;
					Config.Save();
				}
			}
			return true;
		}



#if(!SQUIRREL)
		public static void CopyReplayFiles()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataReplayDirPath = Config.AppDataPath + @"\Replays";
			var dataReplayDirPath = Config.Instance.DataDirPath + @"\Replays";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(Directory.Exists(dataReplayDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					if(Directory.Exists(appDataReplayDirPath))
					{
						CopyFolder(appDataReplayDirPath, appDataReplayDirPath + time);
						Directory.Delete(appDataReplayDirPath, true);
						Log.Info("Created backups of replays in appdata");
					}


					CopyFolder(dataReplayDirPath, appDataReplayDirPath);
					Directory.Delete(dataReplayDirPath, true);

					Log.Info("Moved replays to appdata");
				}
			}
			else if(Directory.Exists(appDataReplayDirPath)) //Save in DataDir and AppData Replay dir still exists
			{
				//backup in case the file already exists
				var time = DateTime.Now.ToFileTime();
				if(Directory.Exists(dataReplayDirPath))
				{
					CopyFolder(dataReplayDirPath, dataReplayDirPath + time);
					Directory.Delete(dataReplayDirPath, true);
				}
				Log.Info("Created backups of replays locally");


				CopyFolder(appDataReplayDirPath, dataReplayDirPath);
				Directory.Delete(appDataReplayDirPath, true);
				Log.Info("Moved replays to appdata");
			}
		}
#endif

		public static double GetScaledXPos(double left, int width, double ratio) => (width * ratio * left) + (width * (1 - ratio) / 2);

		public static MediaColor GetClassColor(string className, bool priestAsGray)
		{
			if(string.IsNullOrEmpty(className))
				return Colors.DimGray;
			MediaColor color;
			if(Config.Instance.ClassColorScheme == ClassColorScheme.HearthStats)
			{
				if(!HearthStatsClassColors.TryGetValue(className, out color))
					color = Colors.DimGray;
			}
			else
			{
				if(className == "Priest" && priestAsGray)
					color = MediaColor.FromArgb(0xFF, 0xD2, 0xD2, 0xD2); //#D2D2D2
				else if(!ClassicClassColors.TryGetValue(className, out color))
					color = MediaColor.FromArgb(0xFF, 0x80, 0x80, 0x80); //#808080
			}
			return color;
		}

		public static MetroWindow GetParentWindow(DependencyObject current) => GetVisualParent<MetroWindow>(current);

		public static T GetVisualParent<T>(DependencyObject current)
		{
			var parent = VisualTreeHelper.GetParent(current);
			while(parent != null && !(parent is T))
				parent = VisualTreeHelper.GetParent(parent);
			return (T)(object)parent;
		}

		public static bool IsWindows10()
		{
			try
			{
				var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				return reg != null && ((string)reg.GetValue("ProductName")).Contains("Windows 10");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}
		public static bool IsWindows8()
		{
			try
			{
				var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				return reg != null && ((string)reg.GetValue("ProductName")).Contains("Windows 8");
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		public static bool TryOpenUrl(string url, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
		{
			try
			{
				Log.Info("[Helper.TryOpenUrl] " + url, memberName, sourceFilePath);
				Process.Start(url);
				return true;
			}
			catch(Exception e)
			{
				Log.Error("[Helper.TryOpenUrl] " + e, memberName, sourceFilePath);
				return false;
			}
		}

		public static string BuildHsReplayNetUrl(string path, string campaign, IEnumerable<string> queryParams = null, IEnumerable<string> fragmentParams = null)
		{
			var url = "https://hsreplay.net";
			if(!path.StartsWith("/"))
				url += "/";
			url += path;
			if(!url.EndsWith("/"))
				url += "/";
			return url + GetHsReplayNetUrlParams(campaign, queryParams, fragmentParams);
		}

		public static string GetHsReplayNetUrlParams(string campaign, IEnumerable<string> queryParams = null, IEnumerable<string> fragmentParams = null)
		{
			var query = new List<string>
			{
				"utm_source=hdt",
				"utm_medium=client",
			};
			if(!string.IsNullOrEmpty(campaign))
				query.Add("utm_campaign=" + campaign);
			if(queryParams != null)
				query.AddRange(queryParams);
			var urlParams = "?" + string.Join("&", query);
			var fragments = fragmentParams?.ToArray();
			if(fragments?.Any() ?? false)
				urlParams += "#" + string.Join("&", fragments);
			return urlParams;
		}

		private static int? _hearthstoneBuild;
		public static int? GetHearthstoneBuild()
		{
			if(_hearthstoneBuild.HasValue)
				return _hearthstoneBuild;
			var exe = Path.Combine(Config.Instance.HearthstoneDirectory, "Hearthstone.exe");
			_hearthstoneBuild = !File.Exists(exe) ? (int?)null : FileVersionInfo.GetVersionInfo(exe).FilePrivatePart;
			return _hearthstoneBuild;
		}

		internal static void ClearCachedHearthstoneBuild() => _hearthstoneBuild = null;

		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if(depObj == null)
				yield break;
			for(var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i) as T;
				if(child != null)
					yield return child;
				foreach(var childOfChild in FindVisualChildren<T>(child))
					yield return childOfChild;
			}
		}

		// Finds all children of depObj that are type T, recursing into matching children.
		public static IEnumerable<T> FindLogicalChildrenDeep<T>(DependencyObject depObj) where T : DependencyObject
		{
			if(depObj == null)
				yield break;
			foreach(var child in LogicalTreeHelper.GetChildren(depObj).OfType<T>())
			{
				yield return child;
				foreach(var childOfChild in FindLogicalChildrenDeep<T>(child))
					yield return childOfChild;
			}
		}

		// Finds all descendants of depObj that match filter, but does not recurse into matching objects (ie will not return both a parent and its child).
		public static IEnumerable<T> FindLogicalDescendants<T>(Func<T, bool> filter, DependencyObject depObj) where T : DependencyObject
		{
			if(depObj == null)
				yield break;
			foreach(var child in LogicalTreeHelper.GetChildren(depObj).OfType<DependencyObject>())
			{
				if(child is T childT && filter(childT))
				{
					yield return childT;
					continue;
				}
				foreach(var childOfChild in FindLogicalDescendants<T>(filter, child))
					yield return childOfChild;
			}
		}

		public static async Task WaitForFileAccess(string path, int delay)
		{
			while(true)
			{
				try
				{
					using(var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
					{
						if(stream.Name != null)
							break;
					}
				}
				catch
				{
					await Task.Delay(delay);
				}
			}
		}

		public static SolidColorBrush BrushFromHex(string hex)
		{
			if(hex.StartsWith("#"))
				hex = hex.Remove(0, 1);
			if(string.IsNullOrEmpty(hex) || hex.Length != 6 || !Helper.IsHex(hex))
				return null;
			var color = ColorTranslator.FromHtml("#" + hex);
			return new SolidColorBrush(MediaColor.FromRgb(color.R, color.G, color.B));
		}

		//See https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx for value conversion
		public static int GetInstalledDotNetVersion()
		{
			try
			{
				const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
				using(var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
					return (int)(ndpKey?.GetValue("Release") ?? -1);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return -1;
			}
		}

		public static string GetWindowsVersion()
		{
			try
			{
				var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				return reg == null ? "Unknown" : $"{reg.GetValue("ProductName")} {reg.GetValue("CurrentBuild")}";
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return "Unknown";
			}
		}

		public static bool IsValidUrl(string url)
			=> Uri.TryCreate(url, UriKind.Absolute, out Uri result)
				&& (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);

		public static readonly Dictionary<MultiClassGroup, CardClass[]> MultiClassGroups = new Dictionary<MultiClassGroup, CardClass[]>
		{
			{MultiClassGroup.GRIMY_GOONS, new[] {CardClass.HUNTER, CardClass.PALADIN, CardClass.WARRIOR}},
			{MultiClassGroup.JADE_LOTUS, new[] {CardClass.DRUID, CardClass.ROGUE, CardClass.SHAMAN}},
			{MultiClassGroup.KABAL, new[] {CardClass.MAGE, CardClass.PRIEST, CardClass.WARLOCK}}
		};

		public static IEnumerable<FileInfo> GetFileInfos(string path, bool subDir)
		{
			var dirInfo = new DirectoryInfo(path);
			foreach(var fileInfo in dirInfo.GetFiles())
				yield return fileInfo;
			if(!subDir)
				yield break;
			foreach(var dir in dirInfo.GetDirectories())
			foreach(var fileInfo in dir.GetFiles())
				yield return fileInfo;
		}

		internal static void VerifyHearthstonePath()
		{
			var proc = User32.GetHearthstoneProc();
			if(proc == null)
			{
				Log.Warn("Could not find Hearthstone process");
				return;
			}
			try
			{
				var currentPath = Config.Instance.HearthstoneDirectory;
				var procPath = Path.GetDirectoryName(Kernel32.GetProcessExePath(proc));
				if(procPath != null && procPath != currentPath)
				{
					Log.Warn($"Current path (\"{currentPath}\") does not match the running Hearthstone process: \"{procPath}\". Updating path");
					Config.Instance.HearthstoneDirectory = procPath;
					Config.Save();
					Core.Reset().Forget();
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public static bool TryGetAttribute<T>(object obj, out T attribute) where T : Attribute
		{
			var members = obj?.GetType().GetMember(obj.ToString());
			if(members?.Length > 0)
			{
				var attributes = members[0].GetCustomAttributes(typeof(T), false);
				if(attributes.Length > 0)
				{
					attribute = (T)attributes[0];
					return true;
				}
			}
			attribute = default(T);
			return false;
		}

		internal static string GetUserAgent()
		{
#if(SQUIRREL)
			const string name = "HDT";
#else
			const string name = "HDTPortable";
#endif
			return name + "/" + GetCurrentVersion();
		}
	}
}
