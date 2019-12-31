#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls;
using System.Windows.Media;
using System.Drawing;
using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts
{
	public static class ToastManager
	{
		private static readonly List<ToastHelper> Toasts = new List<ToastHelper>();
		private static readonly Dictionary<ToastHelper, ToastHelper> GameResultToasts = new Dictionary<ToastHelper, ToastHelper>();

		internal static void ShowBattlegroundsToast(int[] heroDbfIds)
		{
			var th = new ToastHelper(new BattlegroundsToast(heroDbfIds));
			ShowToast(th, 6);
		}

		internal static void ShowCollectionUpdatedToast()
		{
			ShowToast(new ToastHelper(new CollectionUploadedToast()));
		}

		internal static void ShowGameResultToast(string deckName, GameStats game)
		{
			if(game == null)
				return;
			var result = new ToastHelper(new GameResultToast(deckName, game), false);
			if(Config.Instance.ShowReplayShareToast)
			{
				var replay = new ToastHelper(new ReplayToast(game));
				GameResultToasts.Add(replay, result);
				ShowToast(result);
				ShowToast(replay);
			}
			else
				ShowToast(result);
		}

		internal static Action<ReplayProgress> ShowReplayProgressToast()
		{
			var progressControl = new ReplayProgressToast();
			var toast = new ToastHelper(progressControl, false);
			ShowToast(toast, 20000);
			return status => progressControl.Status = status;
		}

		public static void ShowCustomToast(UserControl content) => ShowToast(new ToastHelper(content));

		public static void ForceCloseToast(UserControl control)
		{
			var toast = Toasts.FirstOrDefault(x => x.IsToastWindow(control));
			if(toast == null)
				return;
			if(toast.ToastType == typeof(ReplayToast))
			{
				if(GameResultToasts.TryGetValue(toast, out ToastHelper resultToast))
					resultToast.ForceClose();
			}
			toast.ForceClose();
		}

		private static async void ShowToast(ToastHelper toastHelper, int fadeOutDelay = 0)
		{
			Toasts.Add(toastHelper);
			toastHelper.Show();
			UpdateToasts();
			await toastHelper.HandleToast(fadeOutDelay);
			Toasts.Remove(toastHelper);
		}

		internal static void UpdateToasts()
		{
			var hsWindow = User32.GetHearthstoneWindow();
			var hsRect = hsWindow == IntPtr.Zero ? (Rectangle?)null : User32.GetHearthstoneRect(true);
			var bottom = hsRect?.Bottom ?? SystemParameters.WorkArea.Bottom;
			var right = hsRect?.Right ?? SystemParameters.WorkArea.Right;
			var offset = 0;
			foreach(var toast in Toasts)
				offset = toast.SetPosition(bottom, right, offset);
		}
	}
}
