#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using static HearthDb.CardIds.Collectible.Neutral;
using static Hearthstone_Deck_Tracker.Utility.MouseActions;

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

		public static async Task<ExportingInfo> EnsureHearthstoneInForeground(ExportingInfo info)
		{
			if(User32.IsHearthstoneInForeground())
				return info;
			User32.ShowWindow(info.HsHandle, User32.GetHearthstoneWindowState() == WindowState.Minimized ? User32.SwRestore : User32.SwShow);
			User32.SetForegroundWindow(info.HsHandle);
			await Task.Delay(500);
			if(User32.IsHearthstoneInForeground())
				return new ExportingInfo();
			Core.MainWindow.ShowMessage("Exporting error", "Can't find Hearthstone window.").Forget();
			Log.Error("Can't find Hearthstone window.");
			return null;
		}
	}
}