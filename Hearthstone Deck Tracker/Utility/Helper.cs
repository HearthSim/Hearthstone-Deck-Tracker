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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.FlyoutControls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Color = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;
using Point = System.Drawing.Point;
using Region = Hearthstone_Deck_Tracker.Enums.Region;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class Helper
	{
		public static double DpiScalingX = 1.0, DpiScalingY = 1.0;

		public static readonly string[] EventKeys = {"None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"};

		public static readonly Dictionary<string, string> LanguageDict = new Dictionary<string, string>
		{
			{"English", "enUS"},
			{"Chinese (China)", "zhCN"},
			{"Chinese (Taiwan)", "zhTW"},
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

		private static Version _currentVersion;

		private static bool? _hearthstoneDirExists;

		private static readonly Regex CardLineRegexCountFirst = new Regex(@"(^(\s*)(?<count>\d)(\s*x)?\s+)(?<cardname>[\w\s'\.:!-]+)");
		private static readonly Regex CardLineRegexCountLast = new Regex(@"(?<cardname>[\w\s'\.:!-]+)(\s+(x\s*)(?<count>\d))(\s*)$");
		private static readonly Regex CardLineRegexCountLast2 = new Regex(@"(?<cardname>[\w\s'\.:!-]+)(\s+(?<count>\d))(\s*)$");

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


		[Obsolete("Use API.Core.MainWindow", true)]
		public static MainWindow MainWindow => Core.MainWindow;

		public static OptionsMain OptionsMain { get; set; }
		public static bool SettingUpConstructedImporting { get; set; }

		public static bool HearthstoneDirExists
		{
			get
			{
				if(!_hearthstoneDirExists.HasValue)
					_hearthstoneDirExists = FindHearthstoneDir();
				return _hearthstoneDirExists.Value;
			}
		}

		// A bug in the SerializableVersion.ToString() method causes this to load Version.xml incorrectly.
		// The build and revision numbers are swapped (i.e. a Revision of 21 in Version.xml loads to Version.Build == 21).
		public static Version GetCurrentVersion()
		{
			try
			{
				return _currentVersion ?? (_currentVersion = new Version(XmlManager<SerializableVersion>.Load("Version.xml").ToString()));
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static bool IsNumeric(char c)
		{
			int output;
			return int.TryParse(c.ToString(), out output);
		}

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

		public static PngBitmapEncoder ScreenshotDeck(ItemsControl dlv, double dpiX, double dpiY, string name)
		{
			try
			{
				var rtb = new RenderTargetBitmap((int)dlv.ActualWidth, (int)dlv.ActualHeight, dpiX, dpiY, PixelFormats.Pbgra32);
				rtb.Render(dlv);

				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(rtb));
				return encoder;
			}
			catch(Exception)
			{
				return null;
			}
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

			view1.SortDescriptions.Add(new SortDescription(nameof(Card.Cost), ListSortDirection.Ascending));
			view1.SortDescriptions.Add(new SortDescription(nameof(Card.Type), ListSortDirection.Descending));
			view1.SortDescriptions.Add(new SortDescription(nameof(Card.LocalizedName), ListSortDirection.Ascending));
		}


		public static string DeckToIdString(Deck deck)
			=> deck.GetSelectedDeckVersion().Cards.Aggregate("", (current, card) => current + (card.Id + ":" + card.Count + ";"));

		[Obsolete("Use Utility.ScreenCapture.CaptureHearthstone", true)]
		public static Bitmap CaptureHearthstone(Point point, int width, int height, IntPtr wndHandle = default(IntPtr),
												bool requireInForeground = true) => CaptureHearthstoneAsync(point, width, height, wndHandle, requireInForeground).Result;

		[Obsolete("Use Utility.ScreenCapture.CaptureHearthstoneAsync", true)]
		public static async Task<Bitmap> CaptureHearthstoneAsync(Point point, int width, int height, IntPtr wndHandle = default(IntPtr),
																 bool requireInForeground = true, bool? altScreenCapture = null)
			=> await ScreenCapture.CaptureHearthstoneAsync(point, width, height, wndHandle, requireInForeground, altScreenCapture);

		[Obsolete("Use Utility.ScreenCapture.CaptureWindow", true)]
		public static Bitmap CaptureWindow(IntPtr wndHandle, Point point, int width, int height) => ScreenCapture.CaptureWindow(wndHandle, point, width, height);

		[Obsolete("Use Utility.ScreenCapture.CaptureScreen", true)]
		public static Bitmap CaptureScreen(IntPtr wndHandle, Point point, int width, int height) => ScreenCapture.CaptureScreen(wndHandle, point, width, height);

		public static async Task<bool> FriendsListOpen()
		{
			//wait for friendslist to open/close
			await Task.Delay(300);

			var rect = User32.GetHearthstoneRect(false);
			var capture = await ScreenCapture.CaptureHearthstoneAsync(new Point(0, (int)(rect.Height * 0.85)), (int)(rect.Width * 0.1), (int)(rect.Height * 0.15));
			if(capture == null)
				return false;

			for(var y = 0; y < capture.Height; y++)
			{
				for(var x = 0; x < capture.Width; x++)
				{
					if(!IsYellowPixel(capture.GetPixel(x, y)))
						continue;
					var foundFriendsList = true;

					//check for a straight yellow line (left side of add button)
					for(var i = 0; i < 5; i++)
					{
						if(x + i >= capture.Width || !IsYellowPixel(capture.GetPixel(x + i, y)))
							foundFriendsList = false;
					}
					if(foundFriendsList)
						return true;
				}
			}
			return false;
		}

		private static bool IsYellowPixel(Color pixel)
		{
			const int red = 216;
			const int green = 174;
			const int blue = 10;
			const int deviation = 10;
			return Math.Abs(pixel.R - red) <= deviation && Math.Abs(pixel.G - green) <= deviation && Math.Abs(pixel.B - blue) <= deviation;
		}

		public static void UpdateEverything(GameV2 game)
		{
			if(Core.Overlay.IsVisible)
				Core.Overlay.Update(false);

			if(Core.Windows.PlayerWindow.IsVisible)
				Core.Windows.PlayerWindow.SetCardCount(game.Player.HandCount, game.Player.DeckCount);

			if(Core.Windows.OpponentWindow.IsVisible)
				Core.Windows.OpponentWindow.SetOpponentCardCount(game.Opponent.HandCount, game.Opponent.DeckCount, game.Opponent.HasCoin);
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

		public static string GetWinPercentString(int wins, int losses)
			=> wins + losses == 0 ? "-%" : Math.Round(wins * 100.0 / (wins + losses), 0) + "%";

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
		{
			long time;
			return long.TryParse(unixTime, out time) ? FromUnixTime(time) : DateTime.Now;
		}

		public static async Task SetupConstructedImporting(GameV2 game)
		{
			var settings = new MessageDialogs.Settings {AffirmativeButtonText = "continue"};
			if(!game.IsRunning)
				await Core.MainWindow.ShowMessageAsync("Step 0:", "Start Hearthstone", settings: settings);
			await Core.MainWindow.ShowMessageAsync("Step 1:", "Go to the main menu", settings: settings);
			SettingUpConstructedImporting = true;
			await
				Core.MainWindow.ShowMessageAsync("Step 2:",
												 "Open \"My Collection\" and click each class icon at the top once.\n\n- Do not click on neutral\n- Do not open any decks\n- Do not flip the pages.",
												 settings: new MessageDialogs.Settings {AffirmativeButtonText = "done"});
			Config.Instance.ConstructedImportingIgnoreCachedIds = game.PossibleConstructedCards.Select(c => c.Id).ToArray();
			Config.Save();
			SettingUpConstructedImporting = false;
		}

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

		public static void UpdatePlayerCards(bool reset = false)
		{
			Core.Overlay.UpdatePlayerCards(reset);
			Core.Windows.PlayerWindow.UpdatePlayerCards(reset);
		}

		public static void UpdateOpponentCards(bool reset = false)
		{
			Core.Overlay.UpdateOpponentCards(reset);
			Core.Windows.OpponentWindow.UpdateOpponentCards(reset);
		}

		public static async Task StartHearthstoneAsync()
		{
			if(User32.GetHearthstoneWindow() != IntPtr.Zero)
				return;
			Core.MainWindow.BtnStartHearthstone.IsEnabled = false;
			var useNoDeckMenuItem = Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey("startHearthstone");
			Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Enabled = false;
			try
			{
				var bnetProc = Process.GetProcessesByName("Battle.net").FirstOrDefault();
				if(bnetProc == null)
				{
					Process.Start("battlenet://");

					var foundBnetWindow = false;
					Core.MainWindow.TextBlockBtnStartHearthstone.Text = "STARTING LAUNCHER...";
					for(var i = 0; i < 20; i++)
					{
						bnetProc = Process.GetProcessesByName("Battle.net").FirstOrDefault();
						if(bnetProc != null && bnetProc.MainWindowHandle != IntPtr.Zero)
						{
							foundBnetWindow = true;
							break;
						}
						await Task.Delay(500);
					}
					Core.MainWindow.TextBlockBtnStartHearthstone.Text = "START LAUNCHER / HEARTHSTONE";
					if(!foundBnetWindow)
					{
						Core.MainWindow.ShowMessageAsync("Error starting battle.net launcher", "Could not find or start the battle.net launcher.").Forget();
						Core.MainWindow.BtnStartHearthstone.IsEnabled = true;
						return;
					}
				}
				await Task.Delay(2000); //just to make sure
				Process.Start("battlenet://WTCG");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}

			Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Enabled = true;
			Core.MainWindow.BtnStartHearthstone.IsEnabled = true;
		}

		public static Region GetCurrentRegion()
		{
			try
			{
				var bnetAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battle.net");
				var files = new DirectoryInfo(bnetAppData).GetFiles();
				var config = files.OrderByDescending(x => x.LastWriteTime).FirstOrDefault(x => Regex.IsMatch(x.Name, @"\w{8}\.config"));
				if(config == null)
					return Region.UNKNOWN;
				string content;
				using(var fs = new FileStream(config.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using(var reader = new StreamReader(fs))
					content = reader.ReadToEnd();
				dynamic json = JsonConvert.DeserializeObject(content);
				switch((string)json.User.Client.PlayScreen.GameFamily.WTCG.LastSelectedGameRegion)
				{
					case "EU":
						return Region.EU;
					case "US":
						return Region.US;
					case "KR":
						return Region.ASIA;
					case "CN":
						return Region.CHINA;
					default:
						return Region.UNKNOWN;
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return Region.UNKNOWN;
		}

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

		public static Deck ParseCardString(string cards, bool localizedNames = false)
		{
			try
			{
				var deck = new Deck();
				var lines = cards.Split('\n');
				foreach(var line in lines)
				{
					var count = 1;
					var cardName = line.Trim();
					Match match = null;
					if(CardLineRegexCountFirst.IsMatch(cardName))
						match = CardLineRegexCountFirst.Match(cardName);
					else if(CardLineRegexCountLast.IsMatch(cardName))
						match = CardLineRegexCountLast.Match(cardName);
					else if(CardLineRegexCountLast2.IsMatch(cardName))
						match = CardLineRegexCountLast2.Match(cardName);
					if(match != null)
					{
						var tmpCount = match.Groups["count"];
						if(tmpCount.Success)
							count = int.Parse(tmpCount.Value);
						cardName = match.Groups["cardname"].Value.Trim();
					}

					var card = Database.GetCardFromName(cardName, localizedNames);
					if(string.IsNullOrEmpty(card?.Name))
						continue;
					card.Count = count;

					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;

					if(deck.Cards.Contains(card))
					{
						var deckCard = deck.Cards.First(c => c.Equals(card));
						deck.Cards.Remove(deckCard);
						deckCard.Count += count;
						deck.Cards.Add(deckCard);
					}
					else
						deck.Cards.Add(card);
				}
				return deck;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}


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

		public static void UpdateAppTheme()
		{
			var theme = GetAppTheme();
			ThemeManager.ChangeAppStyle(Application.Current, GetAppAccent(), theme);
			Application.Current.Resources["GrayTextColorBrush"] = theme.Name == MetroTheme.BaseLight.ToString()
																	  ? new SolidColorBrush((MediaColor)Application.Current.Resources["GrayTextColor1"])
																	  : new SolidColorBrush((MediaColor)Application.Current.Resources["GrayTextColor2"]);
		}

		public static Accent GetAppAccent() => string.IsNullOrEmpty(Config.Instance.AccentName)
												  ? ThemeManager.DetectAppStyle().Item2 : ThemeManager.Accents.First(a => a.Name == Config.Instance.AccentName);

		public static AppTheme GetAppTheme() => ThemeManager.AppThemes.First(t => t.Name == Config.Instance.AppTheme.ToString());

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

		public static MetroWindow GetParentWindow(DependencyObject current)
		{
			var parent = VisualTreeHelper.GetParent(current);
			while(parent != null && !(parent is MetroWindow))
				parent = VisualTreeHelper.GetParent(parent);
			return (MetroWindow)parent;
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

		public static int? GetHearthstoneBuild()
		{
			var exe = Path.Combine(Config.Instance.HearthstoneDirectory, "Hearthstone.exe");
			return !File.Exists(exe) ? (int?)null : FileVersionInfo.GetVersionInfo(exe).FilePrivatePart;
		}

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

		public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if(depObj == null)
				yield break;
			foreach(var child in LogicalTreeHelper.GetChildren(depObj))
			{
				var obj = child as T;
				if(obj != null)
					yield return obj;
				var childDepObj = child as DependencyObject;
				if(childDepObj == null)
					continue;
				foreach(var childOfChild in FindLogicalChildren<T>(childDepObj))
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

		public static Region GetRegionByServerIp(string ip)
		{
			if(string.IsNullOrEmpty(ip))
				return Region.UNKNOWN;
			if(ip.StartsWith("12.130"))
				return Region.US;
			if(ip.StartsWith("80.239"))
				return Region.EU;
			if(ip.StartsWith("117.52"))
				return Region.ASIA;
			if(ip.StartsWith("114.113"))
				return Region.CHINA;
			Log.Warn("Unknown IP: " + ip);
			return Region.UNKNOWN;
		}
	}
}