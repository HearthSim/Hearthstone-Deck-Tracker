#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Exporting;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using Clipboard = System.Windows.Clipboard;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	public class PredefinedHotKeyActionInfo
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string MethodName { get; set; }
	}

	public class PredefinedHotKeyActions
	{
		public static IEnumerable<PredefinedHotKeyActionInfo> PredefinedActionNames
		{
			get
			{
				return
					typeof(PredefinedHotKeyActions).GetMethods()
					                               .Where(x => x.GetCustomAttributes(typeof(PredefinedHotKeyActionAttribute), false).Any())
					                               .Select(x =>
					                               {
						                               var attr =
							                               ((PredefinedHotKeyActionAttribute)
							                                x.GetCustomAttributes(typeof(PredefinedHotKeyActionAttribute), false)[0]);
						                               return new PredefinedHotKeyActionInfo
						                               {
							                               MethodName = x.Name,
							                               Title = attr.Title,
							                               Description = attr.Description
						                               };
					                               });
			}
		}

		[PredefinedHotKeyAction("Toggle overlay", "Turns the overlay on or off (if the game is running).")]
		public static void ToggleOverlay()
		{
			if(!Core.Game.IsRunning)
				return;
			Config.Instance.HideOverlay = !Config.Instance.HideOverlay;
			Config.Save();
			Core.Overlay.UpdatePosition();
		}

		[PredefinedHotKeyAction("Toggle overlay: card marks",
			"Turns the card marks and age on the overlay on or off (if the game is running).")]
		public static void ToggleOverlayCardMarks()
		{
			if(!Core.Game.IsRunning)
				return;
			Config.Instance.HideOpponentCardMarks = !Config.Instance.HideOpponentCardMarks;
			Config.Instance.HideOpponentCardAge = Config.Instance.HideOpponentCardMarks;
			Config.Save();
			Core.Overlay.UpdatePosition();
		}

		[PredefinedHotKeyAction("Toggle overlay: secrets", "Turns the secrets panel on the overlay on or off (if the game is running).")]
		public static void ToggleOverlaySecrets()
		{
			if(!Core.Game.IsRunning)
				return;
			Config.Instance.HideSecrets = !Config.Instance.HideSecrets;
			Config.Save();
			Core.Overlay.UpdatePosition();
		}

		[PredefinedHotKeyAction("Toggle overlay: timers", "Turns the timers on the overlay on or off (if the game is running).")]
		public static void ToggleOverlayTimer()
		{
			if(!Core.Game.IsRunning)
				return;
			Config.Instance.HideTimers = !Config.Instance.HideTimers;
			Config.Save();
			Core.Overlay.UpdatePosition();
		}

		[PredefinedHotKeyAction("Toggle overlay: attack icons", "Turns both attack icons on the overlay on or off (if the game is running).")
		]
		public static void ToggleOverlayAttack()
		{
			if(!Core.Game.IsRunning)
				return;
			Config.Instance.HidePlayerAttackIcon = !Config.Instance.HidePlayerAttackIcon;
			Config.Instance.HideOpponentAttackIcon = Config.Instance.HidePlayerAttackIcon;
			Config.Save();
			Core.Overlay.UpdatePosition();
		}

		[PredefinedHotKeyAction("Toggle no deck mode", "Activates \"no deck mode\" (use no deck) or selects the last used deck.")]
		public static void ToggleNoDeckMode()
		{
			if(DeckList.Instance.ActiveDeck == null)
				Core.MainWindow.SelectLastUsedDeck();
			else
				Core.MainWindow.SelectDeck(null, true);
		}

		[PredefinedHotKeyAction("Export deck",
			"Activates \"no deck mode\" (use no deck) or selects the last used deck. This will not show any dialogs in the main window.")]
		public static void ExportDeck()
		{
			if(DeckList.Instance.ActiveDeck != null && Core.Game.IsInMenu)
				DeckExporter.Export(DeckList.Instance.ActiveDeckVersion).Forget();
		}

		[PredefinedHotKeyAction("Edit active deck", "Opens the edit dialog for the active deck (if any) and brings HDT to foreground.")]
		public static void EditDeck()
		{
			if(DeckList.Instance.ActiveDeck == null)
				return;
			Core.MainWindow.SetNewDeck(DeckList.Instance.ActiveDeck, true);
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Import from game: arena", "Starts the webimport process with all dialogs.")]
		public static void ImportFromArena()
		{
			Core.MainWindow.StartArenaImporting().Forget();
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Import from game: constructed", "Starts the webimport process with all dialogs.")]
		public static void ImportFromConstructed()
		{
			Core.MainWindow.ShowImportDialog(false);
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Import from web", "Starts the webimport process with all dialogs.")]
		public static void ImportFromWeb()
		{
			Core.MainWindow.ImportDeck();
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Import from web: clipboard", "Starts the webimport process without the import dialog.")]
		public static void ImportFromWebClipboard()
		{
			var clipboard = Clipboard.ContainsText() ? Clipboard.GetText() : "could not get text from clipboard";
			Core.MainWindow.ImportDeck(clipboard);
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Import from web: highlight",
			"Starts the webimport process without any dialogs. This sends a \"ctrl-c\" command before starting the import: just highlight the url and press the hotkey."
			)]
		public static async void ImportFromWebHighlight()
		{
			SendKeys.SendWait("^c");
			await Task.Delay(200);
			var clipboard = Clipboard.ContainsText() ? Clipboard.GetText() : "could not get text from clipboard";
			Core.MainWindow.ImportDeck(clipboard);
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Screenshot",
			"Creates a screenshot of the game and overlay (and everthing else in front of it). Comes with an option to automatically upload to imgur."
			)]
		public static async void Screenshot()
		{
			var handle = User32.GetHearthstoneWindow();
			if(handle == IntPtr.Zero)
				return;
			var rect = User32.GetHearthstoneRect(false);
			var bmp = await ScreenCapture.CaptureHearthstoneAsync(new Point(0, 0), rect.Width, rect.Height, handle, false, false);
			if(bmp == null)
			{
				Log.Error("There was an error capturing hearthstone.");
				return;
			}
			using(var mem = new MemoryStream())
			{
				var encoder = new PngBitmapEncoder();
				bmp.Save(mem, ImageFormat.Png);
				encoder.Frames.Add(BitmapFrame.Create(mem));
				await Core.MainWindow.SaveOrUploadScreenshot(encoder, "Hearthstone " + DateTime.Now.ToString("MM-dd-yy hh-mm-ss"));
			}
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Game Screenshot",
			"Creates a screenshot of the game only. Comes with an option to automatically upload to imgur."
			)]
		public static async void GameScreenshot()
		{
			var handle = User32.GetHearthstoneWindow();
			if(handle == IntPtr.Zero)
				return;
			var rect = User32.GetHearthstoneRect(false);
			var bmp = await ScreenCapture.CaptureHearthstoneAsync(new Point(0, 0), rect.Width, rect.Height, handle, false, true);
			if(bmp == null)
			{
				Log.Error("There was an error capturing hearthstone.");
				return;
			}
			using(var mem = new MemoryStream())
			{
				var encoder = new PngBitmapEncoder();
				bmp.Save(mem, ImageFormat.Png);
				encoder.Frames.Add(BitmapFrame.Create(mem));
				await Core.MainWindow.SaveOrUploadScreenshot(encoder, "Hearthstone " + DateTime.Now.ToString("MM-dd-yy hh-mm-ss"));
			}
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Note Dialog", "Brings up the note dialog for the current (running) game.")]
		public static void NoteDialog()
		{
			if(Core.Game.IsRunning && !Core.Game.IsInMenu)
				new NoteDialog(Core.Game.CurrentGameStats).Show();
		}

		[PredefinedHotKeyAction("Start Hearthstone", "Starts the Battle.net launcher and/or Hearthstone.")]
		public static void StartHearthstone()
		{
			if(Core.MainWindow.BtnStartHearthstone.IsEnabled)
				Helper.StartHearthstoneAsync().Forget();
		}

		[PredefinedHotKeyAction("Show main window", "Brings up the main window.")]
		public static void ShowMainWindow()
		{
			Core.MainWindow.ActivateWindow();
		}

		[PredefinedHotKeyAction("Show stats", "Brings up the stats window or flyout.")]
		public static void ShowStats()
		{
			Core.MainWindow.ShowStats(false, false);
		}

		[PredefinedHotKeyAction("Reload deck", "Resets HDT to last game start.")]
		public static void ReloadDeck()
		{
			if(DeckList.Instance.ActiveDeck == null)
				Core.MainWindow.SelectDeck(null, true);
			else
				Core.MainWindow.SelectLastUsedDeck();
		}

		[PredefinedHotKeyAction("Close HDT", "Closes HDT.")]
		public static void CloseHdt()
		{
			Core.MainWindow.Close();
		}
	}
}