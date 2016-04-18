#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts
{
	public static class ToastManager
	{
		private static readonly List<ToastHelper> Toasts = new List<ToastHelper>();

		internal static void ShowGameResultToast(string deckName, GameStats game)
		{
			if(game == null)
				return;
			ShowToast(new ToastHelper(new GameResultToast(deckName, game)));
			ShowToast(new ToastHelper(new ReplayToast(game)));
		}

		internal static Action<ReplayProgress> ShowReplayProgressToast()
		{
			var progressControl = new ReplayProgressToast();
			var toast = new ToastHelper(progressControl);
			ShowToast(toast, 20000);
			return status => progressControl.Status = status;
		}

		public static void ShowCustomToast(UserControl content) => ShowToast(new ToastHelper(content));

		public static void ForceCloseToast(UserControl control) => Toasts.FirstOrDefault(x => x.IsToastWindow(control))?.ForceClose();

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
			var offset = 0;
			foreach(var toast in Toasts)
				offset = toast.SetPosition(offset);
		}
	}
}