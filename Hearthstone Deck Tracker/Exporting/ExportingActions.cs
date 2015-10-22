#region

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class ExportingActions
	{
		public static async Task SetDeckName(Deck deck, ExportingInfo info)
		{
			if(Config.Instance.ExportSetDeckName && !deck.TagList.ToLower().Contains("brawl"))
			{
				var name = deck.Name;
				if(Config.Instance.ExportAddDeckVersionToName)
					name += " " + deck.SelectedVersion.ShortVersionString;

				Logger.WriteLine("Setting deck name...", "DeckExporter");
				var nameDeckPos = new Point((int)Helper.GetScaledXPos(Config.Instance.ExportNameDeckX, info.HsRect.Width, info.Ratio),
				                            (int)(Config.Instance.ExportNameDeckY * info.HsRect.Height));
				await MouseActions.ClickOnPoint(info.HsHandle, nameDeckPos);
				//send enter and second click to make sure the current name gets selected
				SendKeys.SendWait("{ENTER}");
				await MouseActions.ClickOnPoint(info.HsHandle, nameDeckPos);
				if(Config.Instance.ExportPasteClipboard)
				{
					Clipboard.SetText(name);
					SendKeys.SendWait("^v");
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
			Logger.WriteLine("Clearing deck...", "DeckExporter");
			while(!ExportingHelper.IsDeckEmpty(info.HsHandle, info.HsRect.Width, info.HsRect.Height, info.Ratio))
			{
				await
					MouseActions.ClickOnPoint(info.HsHandle,
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
				Core.MainWindow.ShowMessage("Exporting aborted", "Hearthstone window lost focus.");
				Logger.WriteLine("Exporting aborted, window lost focus", "DeckExporter");
				return -1;
			}

			await MouseActions.ClickOnPoint(info.HsHandle, info.SearchBoxPos);

			if(Config.Instance.ExportPasteClipboard)
			{
				Clipboard.SetText(ExportingHelper.GetSearchString(card));
				SendKeys.SendWait("^v");
			}
			else
				SendKeys.SendWait(ExportingHelper.GetSearchString(card));
			SendKeys.SendWait("{ENTER}");

			Logger.WriteLine("try to export card: " + card.Name, "DeckExporter", 1);
			await Task.Delay(Config.Instance.DeckExportDelay * 2);

			if(await ExportingHelper.CheckForSpecialCases(card, info.CardPosX + 50, info.Card2PosX + 50, info.CardPosY + 50, info.HsHandle))
				return 0;

			//Check if Card exist in collection
			if(ExportingHelper.CardExists(info.HsHandle, (int)info.CardPosX, (int)info.CardPosY, info.HsRect.Width, info.HsRect.Height))
			{
				//Check if a golden exist
				if(Config.Instance.PrioritizeGolden
				   && ExportingHelper.CardExists(info.HsHandle, (int)info.Card2PosX, (int)info.CardPosY, info.HsRect.Width, info.HsRect.Height))
				{
					await MouseActions.ClickOnPoint(info.HsHandle, new Point((int)info.Card2PosX + 50, (int)info.CardPosY + 50));

					if(card.Count == 2)
					{
						await MouseActions.ClickOnPoint(info.HsHandle, new Point((int)info.Card2PosX + 50, (int)info.CardPosY + 50));
						await MouseActions.ClickOnPoint(info.HsHandle, new Point((int)info.CardPosX + 50, (int)info.CardPosY + 50));
					}
				}
				else
				{
					await MouseActions.ClickOnPoint(info.HsHandle, new Point((int)info.CardPosX + 50, (int)info.CardPosY + 50));

					if(card.Count == 2)
					{
						//Check if two card are not available 
						await Task.Delay(200 - Config.Instance.DeckExportDelay);
						if(ExportingHelper.CardHasLock(info.HsHandle, (int)(info.CardPosX + info.HsRect.Width * 0.048),
						                               (int)(info.CardPosY + info.HsRect.Height * 0.287), info.HsRect.Width, info.HsRect.Height))
						{
							if(ExportingHelper.CardExists(info.HsHandle, (int)info.Card2PosX, (int)info.CardPosY, info.HsRect.Width, info.HsRect.Height))
							{
								await MouseActions.ClickOnPoint(info.HsHandle, new Point((int)info.Card2PosX + 50, (int)info.CardPosY + 50));
								return 0;
							}
							Logger.WriteLine("Only one copy found: " + card.Name, "DeckExporter", 1);
							return 1;
						}

						await MouseActions.ClickOnPoint(info.HsHandle, new Point((int)info.CardPosX + 50, (int)info.CardPosY + 50));
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
			Logger.WriteLine("Creating deck...", "DeckExporter");
			deck.MissingCards.Clear();
			foreach(var card in deck.Cards)
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
			Logger.WriteLine(deck.MissingCards.Count + " missing cards", "DeckExporter");
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
			Logger.WriteLine("Clearing \"Zero\" crystal...", "DeckExporter");

			// First, ensure mana filters are cleared
			var crystalPoint = new Point((int)Helper.GetScaledXPos(Config.Instance.ExportZeroButtonX, info.HsRect.Width, info.Ratio),
			                             (int)(Config.Instance.ExportZeroButtonY * info.HsRect.Height));

			if(ExportingHelper.IsZeroCrystalSelected(info.HsHandle, info.Ratio, info.HsRect.Width, info.HsRect.Height))
			{
				// deselect it
				await MouseActions.ClickOnPoint(info.HsHandle, crystalPoint);
			}
			else
			{
				// select it and then unselect it (in case other crystals are on)
				await MouseActions.ClickOnPoint(info.HsHandle, crystalPoint);
				await MouseActions.ClickOnPoint(info.HsHandle, crystalPoint);
			}
		}

		public static async Task ClearSetsFilter(ExportingInfo info)
		{
			Logger.WriteLine("Clearing set filter...", "DeckExporter");
			// Then ensure "All Sets" is selected
			var setsPoint = new Point((int)Helper.GetScaledXPos(Config.Instance.ExportSetsButtonX, info.HsRect.Width, info.Ratio),
			                          (int)(Config.Instance.ExportSetsButtonY * info.HsRect.Height));

			// open sets menu
			await MouseActions.ClickOnPoint(info.HsHandle, setsPoint);
			// select "All Sets"
			await
				MouseActions.ClickOnPoint(info.HsHandle,
				                         new Point((int)Helper.GetScaledXPos(Config.Instance.ExportAllSetsButtonX, info.HsRect.Width, info.Ratio),
				                                   (int)(Config.Instance.ExportAllSetsButtonY * info.HsRect.Height)));
			// close sets menu
			await MouseActions.ClickOnPoint(info.HsHandle, setsPoint);
		}

		public static async Task ClearSearchBox(IntPtr hsHandle, Point searchBoxPos)
		{
			await MouseActions.ClickOnPoint(hsHandle, searchBoxPos);
			SendKeys.SendWait("{DELETE}");
			SendKeys.SendWait("{ENTER}");
		}
	}
}