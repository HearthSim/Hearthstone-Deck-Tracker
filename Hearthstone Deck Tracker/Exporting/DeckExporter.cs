#region

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Exporting.ExportingActions;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public static class DeckExporter
	{
		public static async Task<bool> Export(Deck deck)
		{
			if(deck == null)
				return false;
			var currentClipboard = "";
			var altScreenCapture = Config.Instance.AlternativeScreenCapture;
			try
			{
				Log.Info("Exporting " + deck.GetDeckInfo());
				if(Config.Instance.ExportPasteClipboard && Clipboard.ContainsText())
					currentClipboard = Clipboard.GetText();

				var info = new ExportingInfo();
				LogDebugInfo(info);

				var inForeground = await ExportingHelper.EnsureHearthstoneInForeground(info.HsHandle);
				if(!inForeground)
					return false;
				Log.Info($"Waiting for {Config.Instance.ExportStartDelay} seconds before starting the export process");
				await Task.Delay(Config.Instance.ExportStartDelay * 1000);
				if(!altScreenCapture)
					Core.Overlay.ForceHide(true);

				await ClearDeck(info);
				await SetDeckName(deck, info);
				await ClearFilters(info);
				var lostFocus = await CreateDeck(deck, info);
				if(lostFocus)
					return false;
				await ClearSearchBox(info.HsHandle, info.SearchBoxPos);

				if(Config.Instance.ExportPasteClipboard)
					Clipboard.Clear();
				Log.Info("Success exporting deck.");
				return true;
			}
			catch(Exception e)
			{
				Log.Error("Error exporting deck: " + e);
				return false;
			}
			finally
			{
				if(!altScreenCapture)
					Core.Overlay.ForceHide(false);
				try
				{
					if(Config.Instance.ExportPasteClipboard && currentClipboard != "")
						Clipboard.SetText(currentClipboard);
				}
				catch(Exception ex)
				{
					Log.Error("Could not restore clipboard content after export: " + ex);
				}
			}
		}

		private static void LogDebugInfo(ExportingInfo info) => Log.Debug($"HsHandle={info.HsHandle} HsRect={info.HsRect} Ratio={info.Ratio} SearchBoxPosX={Config.Instance.ExportSearchBoxX} SearchBoxPosY={Config.Instance.ExportSearchBoxY} CardPosX={Config.Instance.ExportCard1X} Card2PosX={Config.Instance.ExportCard2X} CardPosY={Config.Instance.ExportCardsY} ExportPasteClipboard={Config.Instance.ExportPasteClipboard} ExportNameDeckX={Config.Instance.ExportNameDeckX} ExportNameDeckY={Config.Instance.ExportNameDeckY} PrioritizeGolden={Config.Instance.PrioritizeGolden} DeckExportDelay={Config.Instance.DeckExportDelay} EnableExportAutoFilter={Config.Instance.EnableExportAutoFilter} ExportZeroButtonX={Config.Instance.ExportZeroButtonX} ExportZeroButtonY={Config.Instance.ExportZeroButtonY} ForceClear={Config.Instance.ExportForceClear}");
	}
}