#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class NetDeck
	{
		public static void CheckForChromeExtention()
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
									@"Google\Chrome\User Data\Default\Extensions\lpdbiakcpmcppnpchohihcbdnojlgeel");
			if(Config.Instance.NetDeckClipboardCheck.HasValue)
			{
				if(Config.Instance.NetDeckClipboardCheck.Value && !Directory.Exists(path))
				{
					Config.Instance.NetDeckClipboardCheck = false;
					Config.Save();
				}
				return;
			}

			if(Directory.Exists(path))
			{
				Config.Instance.NetDeckClipboardCheck = true;
				Config.Save();
			}
		}

		public static bool CheckForClipboardImport()
		{
			try
			{
				if(Clipboard.ContainsText())
				{
					var clipboardContent = Clipboard.GetText();
					if(clipboardContent.StartsWith("netdeckimport") || clipboardContent.StartsWith("trackerimport"))
					{
						var clipboardLines = clipboardContent.Split('\n').ToList();
						var deckName = clipboardLines.FirstOrDefault(line => line.StartsWith("name:"));
						if(!string.IsNullOrEmpty(deckName))
						{
							clipboardLines.Remove(deckName);
							deckName = deckName.Replace("name:", "").Trim();
						}
						var url = clipboardLines.FirstOrDefault(line => line.StartsWith("url:"));
						if(!string.IsNullOrEmpty(url))
						{
							clipboardLines.Remove(url);
							url = url.Replace("url:", "").Trim();
						}
						bool? isArenaDeck = null;
						var arena = clipboardLines.FirstOrDefault(line => line.StartsWith("arena:"));
						if(!string.IsNullOrEmpty(arena))
						{
							clipboardLines.Remove(arena);
							bool isArena;
							if(bool.TryParse(arena.Replace("arena:", "").Trim(), out isArena))
								isArenaDeck = isArena;
						}
						var localized = false;
						var nonEnglish = clipboardLines.FirstOrDefault(line => line.StartsWith("nonenglish:"));
						if(!string.IsNullOrEmpty(nonEnglish))
						{
							clipboardLines.Remove(nonEnglish);
							bool.TryParse(nonEnglish.Replace("nonenglish:", "").Trim(), out localized);
						}
						var tagsRaw = clipboardLines.FirstOrDefault(line => line.StartsWith("tags:"));
						var tags = new List<string>();
						if(!string.IsNullOrEmpty(tagsRaw))
						{
							clipboardLines.Remove(tagsRaw);
							tags = tagsRaw.Replace("tags:", "").Trim().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
						}
						clipboardLines.RemoveAt(0); //"netdeckimport" / "trackerimport"

						var deck = Helper.ParseCardString(clipboardLines.Aggregate((c, n) => c + "\n" + n), localized);
						if(deck != null)
						{
							if(tags.Any())
							{
								var reloadTags = false;
								foreach(var tag in tags)
								{
									if(!DeckList.Instance.AllTags.Contains(tag))
									{
										DeckList.Instance.AllTags.Add(tag);
										reloadTags = true;
									}
									deck.Tags.Add(tag);
								}
								if(reloadTags)
								{
									DeckList.Save();
									Core.MainWindow.ReloadTags();
								}
							}

							if(isArenaDeck.HasValue)
								deck.IsArenaDeck = isArenaDeck.Value;
							deck.Url = url;
							deck.Name = deckName;
							Core.MainWindow.SetNewDeck(deck);
							if(Config.Instance.AutoSaveOnImport)
								Core.MainWindow.SaveDeckWithOverwriteCheck();
							Core.MainWindow.ActivateWindow();
						}
						Clipboard.Clear();
						return true;
					}
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
			return false;
		}
	}
}