#region

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public static class DeckExporter
	{
		public static async Task Export(Deck deck)
		{
			if(deck == null)
				return;
			var currentClipboard = "";
			try
			{
				Logger.WriteLine("Exporting " + deck.GetDeckInfo(), "DeckExporter");
				if(Config.Instance.ExportPasteClipboard && Clipboard.ContainsText())
					currentClipboard = Clipboard.GetText();

				var info = new ExportingInfo();
				LogDebugInfo(info);

				var inForeground = await ExportingHelper.EnsureHearthstoneInForeground(info.HsHandle);
				if(!inForeground)
					return;
				Logger.WriteLine("Waiting for " + Config.Instance.ExportStartDelay + " seconds before starting the export process", "DeckExporter");
				await Task.Delay(Config.Instance.ExportStartDelay * 1000);
				Core.Overlay.ForceHide(true);

				await ExportingActions.ClearDeck(info);
				await ExportingActions.SetDeckName(deck, info);
				await ExportingActions.ClearFilters(info);
				var lostFocus = await ExportingActions.CreateDeck(deck, info);
				if(lostFocus)
					return;
				await ExportingActions.ClearSearchBox(info.HsHandle, info.SearchBoxPos);

				if(Config.Instance.ExportPasteClipboard)
					Clipboard.Clear();
				Logger.WriteLine("Success exporting deck.", "DeckExporter");
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error exporting deck: " + e, "DeckExporter");
			}
			finally
			{
				Core.Overlay.ForceHide(false);
				if(Config.Instance.ExportPasteClipboard && currentClipboard != "")
					Clipboard.SetText(currentClipboard);
			}
		}

		private static void LogDebugInfo(ExportingInfo info)
		{
			Logger.WriteLine(
			                 string.Format(
			                               "HsHandle={0} HsRect={1} Ratio={2} SearchBoxPosX={3} SearchBoxPosY={4} CardPosX={5} Card2PosX={6} CardPosY={7} ExportPasteClipboard={8} ExportNameDeckX={9} ExportNameDeckY={10} PrioritizeGolden={11} DeckExportDelay={12} EnableExportAutoFilter={13} ExportZeroButtonX={14} ExportZeroButtonY={15}",
			                               info.HsHandle, info.HsRect, info.Ratio, Config.Instance.ExportSearchBoxX,
			                               Config.Instance.ExportSearchBoxY, Config.Instance.ExportCard1X, Config.Instance.ExportCard2X,
			                               Config.Instance.ExportCardsY, Config.Instance.ExportPasteClipboard, Config.Instance.ExportNameDeckX,
			                               Config.Instance.ExportNameDeckY, Config.Instance.PrioritizeGolden, Config.Instance.DeckExportDelay,
			                               Config.Instance.EnableExportAutoFilter, Config.Instance.ExportZeroButtonX,
			                               Config.Instance.ExportZeroButtonY), "DeckExporter");
		}
	}
}