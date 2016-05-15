#region

using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using static Hearthstone_Deck_Tracker.Exporting.ExportingHelper;
using static Hearthstone_Deck_Tracker.Exporting.MouseActions;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class ExportingActions
	{
		private const int MaxLengthDeckName = 24;

		public static async Task SetDeckName(Deck deck, ExportingInfo info)
		{
			if(Config.Instance.ExportSetDeckName && !deck.TagList.ToLower().Contains("brawl"))
			{
				var name = Regex.Replace(deck.Name, @"[\(\)\{\}]", "");
				if(name != deck.Name)
					Log.Info("Removed parenthesis/braces from deck name. New name: " + name);
				if(Config.Instance.ExportAddDeckVersionToName)
				{
					var version = " " + deck.SelectedVersion.ShortVersionString;
					if(name.Length + version.Length > MaxLengthDeckName)
						name = name.Substring(0, MaxLengthDeckName - version.Length);
					name += version;
				}

				Log.Info("Setting deck name...");
				var nameDeckPos = new Point((int)Helper.GetScaledXPos(Config.Instance.ExportNameDeckX, info.HsRect.Width, info.Ratio),
				                            (int)(Config.Instance.ExportNameDeckY * info.HsRect.Height));
				await ClickOnPoint(info.HsHandle, nameDeckPos);
				//send enter and second click to make sure the current name gets selected
				SendKeys.SendWait("{ENTER}");
				await ClickOnPoint(info.HsHandle, nameDeckPos);
				if(Config.Instance.ExportPasteClipboard)
				{
					Clipboard.SetText(name);
					SendKeys.SendWait("^{v}");
				}
				else
					SendKeys.SendWait(name);
				SendKeys.SendWait("{ENTER}");
			}
		}

		public static async Task ClearDeck(ExportingInfo info)
		{
			if(!Config.Instance.AutoClearDeck)
				return;
			var count = 0;
			Log.Info("Clearing deck...");
			while(!await IsDeckEmpty(info.HsHandle, info.HsRect.Width, info.HsRect.Height, info.Ratio))
			{
				await
					ClickOnPoint(info.HsHandle,
					                          new Point((int)Helper.GetScaledXPos(Config.Instance.ExportClearX, info.HsRect.Width, info.Ratio),
					                                    (int)(Config.Instance.ExportClearY * info.HsRect.Height)));
				if(count++ > 35)
					break;
			}
		}

		///<summary>
		/// Returns -1 if Hearthstone loses focus
		/// </summary>
		public static async Task<int> AddCardToDeck(Card card, ExportingInfo info)
		{
			if(!User32.IsHearthstoneInForeground())
			{
				Core.MainWindow.ShowMessage("Exporting aborted", "Hearthstone window lost focus.").Forget();
				Log.Info("Exporting aborted, window lost focus");
				return -1;
			}

			if(Config.Instance.ExportForceClear)
				await ClearSearchBox(info.HsHandle, info.SearchBoxPos);

			await ClickOnPoint(info.HsHandle, info.SearchBoxPos);

			if(Config.Instance.ExportPasteClipboard || !Helper.LatinLanguages.Contains(Config.Instance.SelectedLanguage))
			{
				Clipboard.SetText(GetSearchString(card));
				SendKeys.SendWait("^{v}");
			}
			else
				SendKeys.SendWait(GetSearchString(card));
			SendKeys.SendWait("{ENTER}");

			Log.Info("try to export card: " + card);
			await Task.Delay(Config.Instance.DeckExportDelay * 2);

			//Check if Card exist in collection
			var cardExists = await CardExists(info.HsHandle, (int)info.CardPosX, (int)info.CardPosY, info.HsRect.Width, info.HsRect.Height);
			if(cardExists)
			{
				//Check if a golden exist
				if(Config.Instance.PrioritizeGolden
				   && await CardExists(info.HsHandle, (int)info.Card2PosX, (int)info.CardPosY, info.HsRect.Width, info.HsRect.Height))
				{
					await ClickOnPoint(info.HsHandle, new Point((int)info.Card2PosX + 50, (int)info.CardPosY + 50));

					if(card.Count == 2)
					{
						await ClickOnPoint(info.HsHandle, new Point((int)info.Card2PosX + 50, (int)info.CardPosY + 50));
						await ClickOnPoint(info.HsHandle, new Point((int)info.CardPosX + 50, (int)info.CardPosY + 50));
					}
				}
				else
				{
					await ClickOnPoint(info.HsHandle, new Point((int)info.CardPosX + 50, (int)info.CardPosY + 50));

					if(card.Count == 2)
					{
						//Check if two card are not available 
						await Task.Delay(200 - Config.Instance.DeckExportDelay);
						if(await CardHasLock(info.HsHandle, (int)(info.CardPosX + info.HsRect.Width * 0.048),
						                               (int)(info.CardPosY + info.HsRect.Height * 0.287), info.HsRect.Width, info.HsRect.Height))
						{
							var card2Exists = await CardExists(info.HsHandle, (int)info.Card2PosX, (int)info.CardPosY, info.HsRect.Width, info.HsRect.Height);
							if(card2Exists)
							{
								await ClickOnPoint(info.HsHandle, new Point((int)info.Card2PosX + 50, (int)info.CardPosY + 50));
								return 0;
							}
							Log.Info("Only one copy found: " + card.Name);
							return 1;
						}

						await ClickOnPoint(info.HsHandle, new Point((int)info.CardPosX + 50, (int)info.CardPosY + 50));
					}
				}
			}
			else
				return card.Count;
			return 0;
		}

		/// <summary>
		/// Returns true if Hearthstone lost focus in the process
		/// </summary>
		public static async Task<bool> CreateDeck(Deck deck, ExportingInfo info)
		{
			Log.Info("Creating deck...");
			deck.MissingCards.Clear();
			foreach(var card in deck.GetSelectedDeckVersion().Cards.ToSortedCardList())
			{
				var missingCardsCount = await AddCardToDeck(card, info);
				if(missingCardsCount < 0)
					return true;
				if(missingCardsCount > 0)
				{
					var missingCard = (Card)card.Clone();
					missingCard.Count = missingCardsCount;
					deck.MissingCards.Add(missingCard);
				}
			}
			Log.Info(deck.MissingCards.Count + " missing cards");
			if(deck.MissingCards.Any())
				DeckList.Save();
			return false;
		}

		public static async Task ClearFilters(ExportingInfo info)
		{
			if(!Config.Instance.EnableExportAutoFilter)
				return;
			await ClearManaFilter(info);
			await ClearSetsFilter(info);
		}

		public static async Task ClearManaFilter(ExportingInfo info)
		{
			Log.Info("Clearing \"Zero\" crystal...");

			// First, ensure mana filters are cleared
			var crystalPoint = new Point((int)Helper.GetScaledXPos(Config.Instance.ExportZeroButtonX, info.HsRect.Width, info.Ratio),
			                             (int)(Config.Instance.ExportZeroButtonY * info.HsRect.Height));

			if(await IsZeroCrystalSelected(info.HsHandle, info.Ratio, info.HsRect.Width, info.HsRect.Height))
			{
				// deselect it
				await ClickOnPoint(info.HsHandle, crystalPoint);
			}
			else
			{
				// select it and then unselect it (in case other crystals are on)
				await ClickOnPoint(info.HsHandle, crystalPoint);
				await ClickOnPoint(info.HsHandle, crystalPoint);
			}
		}

		public static async Task ClearSetsFilter(ExportingInfo info)
		{
			Log.Info("Clearing set filter...");
			// Then ensure "All Sets" is selected
			var setsPoint = new Point((int)Helper.GetScaledXPos(Config.Instance.ExportSetsButtonX, info.HsRect.Width, info.Ratio),
			                          (int)(Config.Instance.ExportSetsButtonY * info.HsRect.Height));

			// open sets menu
			await ClickOnPoint(info.HsHandle, setsPoint);
			await Task.Delay(100);
			// select "All Sets"
			await
				ClickOnPoint(info.HsHandle,
				                          new Point(
					                          (int)Helper.GetScaledXPos(Config.Instance.ExportAllSetsButtonX, info.HsRect.Width, info.Ratio),
					                          (int)(Config.Instance.ExportStandardSetButtonY * info.HsRect.Height)));
			await Task.Delay(100);
			// close sets menu
			await ClickOnPoint(info.HsHandle, setsPoint);
		}

		public static async Task ClearSearchBox(IntPtr hsHandle, Point searchBoxPos)
		{
			await ClickOnPoint(hsHandle, searchBoxPos);
			SendKeys.SendWait("{DELETE}");
			SendKeys.SendWait("{ENTER}");
		}
	}
}