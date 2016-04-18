using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Utility.Toasts
{
	internal class ToastHelper
	{
		private const double FadeInDuration = 0.4;
		private const int FadeOutSpeedup = 2;

		private readonly ToastWindow _window;
		private bool _forceClose;
		private DateTime _startUpTime;

		public ToastHelper(UserControl control)
		{
			_window = new ToastWindow(control);
		}

		// ReSharper disable once PossibleUnintendedReferenceComparison
		public bool IsToastWindow(UserControl control) => _window.ContentControl.Content == control;

		public void Show()
		{
			_window.Show();
			_window.Visibility = Visibility.Hidden;
			_startUpTime = DateTime.UtcNow;
		}

		public async Task HandleToast(int fadeOutDelay = 0)
		{
			fadeOutDelay = fadeOutDelay > 0 ? fadeOutDelay : Config.Instance.NotificationFadeOutDelay;
			_window.Visibility = Visibility.Visible;
			_window.SizeChanged += (sender, args) =>
			{
				_window.Left -= args.NewSize.Width - args.PreviousSize.Width;
				_window.Top -= args.NewSize.Height - args.PreviousSize.Height;
				ToastManager.UpdateToasts();
			};
			while(DateTime.UtcNow - _startUpTime < TimeSpan.FromSeconds(fadeOutDelay + FadeInDuration))
			{
				if(_forceClose)
					break;
				await Task.Delay(100);
				if(!_window.IsMouseOver)
					continue;
				_startUpTime = DateTime.UtcNow - TimeSpan.FromSeconds(FadeOutSpeedup);
			}
			await _window.FadeOut();
		}

		public int SetPosition(int offset)
		{
			_window.Left = SystemParameters.WorkArea.Right - _window.Width - 5;
			if(_window.IsMouseOver)
				return (int)(SystemParameters.WorkArea.Bottom - _window.Top);
			offset += (int)_window.Height + 5;
			_window.Top = _window.IsMouseOver ? _window.Top : SystemParameters.WorkArea.Bottom - offset;
			return offset;
		}

		public void ForceClose() => _forceClose = true;
	}
}