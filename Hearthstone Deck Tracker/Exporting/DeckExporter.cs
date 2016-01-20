#region

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using static Hearthstone_Deck_Tracker.Exporting.ExportingActions;

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
				Logger.WriteLine($"Waiting for {Config.Instance.ExportStartDelay} seconds before starting the export process", "DeckExporter");
				await Task.Delay(Config.Instance.ExportStartDelay * 1000);
				Core.Overlay.ForceHide(true);

				await ClearDeck(info);
				await SetDeckName(deck, info);
				await ClearFilters(info);
				var lostFocus = await CreateDeck(deck, info);
				if(lostFocus)
					return;
				await ClearSearchBox(info.HsHandle, info.SearchBoxPos);

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
							 $"HsHandle={info.HsHandle} HsRect={info.HsRect} Ratio={info.Ratio} SearchBoxPosX={Config.Instance.ExportSearchBoxX} SearchBoxPosY={Config.Instance.ExportSearchBoxY} CardPosX={Config.Instance.ExportCard1X} Card2PosX={Config.Instance.ExportCard2X} CardPosY={Config.Instance.ExportCardsY} ExportPasteClipboard={Config.Instance.ExportPasteClipboard} ExportNameDeckX={Config.Instance.ExportNameDeckX} ExportNameDeckY={Config.Instance.ExportNameDeckY} PrioritizeGolden={Config.Instance.PrioritizeGolden} DeckExportDelay={Config.Instance.DeckExportDelay} EnableExportAutoFilter={Config.Instance.EnableExportAutoFilter} ExportZeroButtonX={Config.Instance.ExportZeroButtonX} ExportZeroButtonY={Config.Instance.ExportZeroButtonY} ForceClear={Config.Instance.ExportForceClear}",
							 "DeckExporter");
		}
	}
}