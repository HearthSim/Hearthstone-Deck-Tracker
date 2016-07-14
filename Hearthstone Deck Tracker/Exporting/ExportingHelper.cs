#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using static HearthDb.CardIds.Collectible.Neutral;
using static Hearthstone_Deck_Tracker.Exporting.MouseActions;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class ExportingHelper
	{
		private static readonly Dictionary<string, string> ArtistDict = new Dictionary<string, string>
		{
			{"enUS", "artist"},
			{"zhCN", "画家"},
			{"zhTW", "畫家"},
			{"enGB", "artist"},
			{"frFR", "artiste"},
			{"deDE", "künstler"},
			{"itIT", "artista"},
			{"jaJP", "アーティスト"},
			{"koKR", "아티스트"},
			{"plPL", "grafik"},
			{"ptBR", "artista"},
			{"ruRU", "художник"},
			{"esMX", "artista"},
			{"esES", "artista"},
		};
		private static readonly Dictionary<string, string> ManaDict = new Dictionary<string, string>
		{
			{"enUS", "mana"},
			{"zhCN", "法力值"},
			{"zhTW", "法力"},
			{"enGB", "mana"},
			{"frFR", "mana"},
			{"deDE", "mana"},
			{"itIT", "mana"},
			{"jaJP", "マナ"},
			{"koKR", "마나"},
			{"plPL", "mana"},
			{"ptBR", "mana"},
			{"ruRU", "мана"},
			{"esMX", "maná"},
			{"esES", "maná"},
		};
		private static readonly Dictionary<string, string> AttackDict = new Dictionary<string, string>
		{
			{"enUS", "attack"},
			{"zhCN", "攻击力"},
			{"zhTW", "攻擊力"},
			{"enGB", "attack"},
			{"frFR", "attaque"},
			{"deDE", "angriff"},
			{"itIT", "attacco"},
			{"jaJP", "攻撃"},
			{"koKR", "공격력"},
			{"plPL", "atak"},
			{"ptBR", "ataque"},
			{"ruRU", "атака"},
			{"esMX", "ataque"},
			{"esES", "ataque"},
		};

		public static async Task<bool> CardExists(IntPtr wndHandle, int posX, int posY, int width, int height)
		{
			const double scale = 0.037; // 40px @ height = 1080
			const double minHue = 90;

			var size = (int)Math.Round(height * scale);

			var capture = await ScreenCapture.CaptureHearthstoneAsync(new Point(posX, posY), size, size, wndHandle);
			if(capture == null)
				return false;

			return HueAndBrightness.GetAverage(capture).Hue > minHue;
		}

		public static async Task<bool> CardHasLock(IntPtr wndHandle, int posX, int posY, int width, int height)
		{
			// setting this as a "width" value relative to height, maybe not best solution?
			const double xScale = 0.051; // 55px @ height = 1080
			const double yScale = 0.0278; // 30px @ height = 1080
			const double maxBrightness = 5.0 / 11.0;

			// ReSharper disable once SuggestVarOrType_BuiltInTypes
			var lockWidth = (int)Math.Round(height * xScale);
			var lockHeight = (int)Math.Round(height * yScale);

			var capture = await ScreenCapture.CaptureHearthstoneAsync(new Point(posX, posY), lockWidth, lockHeight, wndHandle);
			if(capture == null)
				return false;

			return HueAndBrightness.GetAverage(capture).Brightness < maxBrightness;
		}

		public static async Task<bool> IsDeckEmpty(IntPtr wndHandle, int width, int height, double ratio)
		{
			var capture =
				await ScreenCapture.CaptureHearthstoneAsync(
				                          new Point((int)Helper.GetScaledXPos(Config.Instance.ExportClearX, width, ratio),
				                                    (int)(Config.Instance.ExportClearCheckYFixed * height)), 1, 1, wndHandle);
			return capture != null && ColorDistance(capture.GetPixel(0, 0), Color.FromArgb(255, 56, 45, 69), 5);
		}

		public static async Task<bool> IsZeroCrystalSelected(IntPtr wndHandle, double ratio, int width, int height)
		{
			const double scale = 0.020; // 22px @ height = 1080
			const double minBrightness = 0.55;

			var size = (int)Math.Round(height * scale);

			var posX = (int)Helper.GetScaledXPos(Config.Instance.ExportZeroSquareX, width, ratio);
			var posY = (int)(Config.Instance.ExportZeroSquareY * height);

			var capture = await ScreenCapture.CaptureHearthstoneAsync(new Point(posX, posY), size, size, wndHandle);

			if(capture == null)
				return false;

			return HueAndBrightness.GetAverage(capture).Brightness > minBrightness;
		}


		public static string GetArtistSearchString(string artist)
		{
			string artistStr;
			if(ArtistDict.TryGetValue(Config.Instance.SelectedLanguage, out artistStr))
				return $" {artistStr}:{artist.Split(' ').LastOrDefault()}";
			return "";
		}

		public static string GetManaSearchString(int cost)
		{
			string manaStr;
			if(ManaDict.TryGetValue(Config.Instance.SelectedLanguage, out manaStr))
				return $" {manaStr}:{cost}";
			return "";
		}

		public static string GetAttackSearchString(int atk)
		{
			string atkStr;
			if(AttackDict.TryGetValue(Config.Instance.SelectedLanguage, out atkStr))
				return $" {atkStr}:{atk}";
			return "";
		}

		public static string GetSearchString(Card card)
		{
			var searchString = $"{card.LocalizedName}{GetArtistSearchString(card.Artist)} {GetManaSearchString(card.Cost)}".ToLowerInvariant();
			if(card.Id == Feugen || card.Id == Stalagg)
				searchString += GetAttackSearchString(card.Attack);
			return searchString;
		}

		public static async Task<bool> EnsureHearthstoneInForeground(IntPtr hsHandle)
		{
			if(!User32.IsHearthstoneInForeground())
			{
				//restore window and bring to foreground
				User32.ShowWindow(hsHandle, User32.SwRestore);
				User32.SetForegroundWindow(hsHandle);
				//wait it to actually be in foreground, else the rect might be wrong
				await Task.Delay(500);
			}
			if(!User32.IsHearthstoneInForeground())
			{
				Core.MainWindow.ShowMessage("Exporting error", "Can't find Hearthstone window.").Forget();
				Log.Error("Can't find Hearthstone window.");
				return false;
			}
			return true;
		}

		public static bool ColorDistance(Color color, Color target, double distance)
			=> Math.Abs(color.R - target.R) < distance && Math.Abs(color.G - target.G) < distance && Math.Abs(color.B - target.B) < distance;
	}
}