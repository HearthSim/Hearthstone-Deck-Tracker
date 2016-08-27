#region

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Exceptions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Exporting.ExportingActions;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public static class DeckExporter
	{
		public static async Task<bool> Export(Deck deck, Func<Task<bool>> onInterrupt)
		{
			if(deck == null)
				return false;
			var currentClipboard = "";
			try
			{
				Log.Info("Exporting " + deck.GetDeckInfo());
				if(Config.Instance.ExportPasteClipboard && Clipboard.ContainsText())
					currentClipboard = Clipboard.GetText();
				var info = new ExportingInfo();
				LogDebugInfo(info);
				info = await ExportingHelper.EnsureHearthstoneInForeground(info);
				if(info == null)
					return false;
				LogDebugInfo(info);
				Log.Info($"Waiting for {Config.Instance.ExportStartDelay} seconds before starting the export process");
				await Task.Delay(Config.Instance.ExportStartDelay*1000);
				var exporter = new ExportingActions(info, deck, onInterrupt);
				await exporter.ClearDeck();
				await exporter.SetDeckName();
				await exporter.ClearFilters();
				await exporter.CreateDeck();
				await exporter.ClearSearchBox();
				if(Config.Instance.ExportPasteClipboard)
					Clipboard.Clear();
				Log.Info("Success exporting deck.");
				return true;
			}
			catch(ExportingInterruptedException e)
			{
				Log.Warn(e.Message);
				return false;
			}
			catch(Exception e)
			{
				Log.Error("Error exporting deck: " + e);
				return false;
			}
			finally
			{
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