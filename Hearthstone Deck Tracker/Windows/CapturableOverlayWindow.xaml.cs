using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Overlay;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class CapturableOverlayWindow : INotifyPropertyChanged
	{
		private bool _initialized;

		public CapturableOverlayWindow()
		{
			InitializeComponent();
			Core.Overlay.GridMain.SizeChanged += (sender, args) => UpdateSize();
			Core.Overlay.LocationChanged += (sender, args) => UpdatePosition();
			DataContext = this;
			UpdateSize();
			UpdatePosition();
		}

		public Visibility ContentVisibility => Core.Game.IsRunning ? Visibility.Visible : Visibility.Hidden;

		public WindowState? ForcedWindowState { get; internal set; } = WindowState.Minimized;

		public Visual Visual => Core.Overlay.GridMain;

		public SolidColorBrush? BackgroundColor => Helper.BrushFromHex(Config.Instance.StreamingOverlayBackground);

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Update()
		{
			var state = Helper.GameWindowState;
			if(state == WindowState.Maximized)
				state = WindowState.Normal;
			if(ForcedWindowState == state)
				return;
			ForcedWindowState = state;
			if(!_initialized)
			{
				_initialized = true;
				UpdateSize();
				UpdatePosition();
			}
			if(WindowState != state)
				WindowState = state;
		}

		private void UpdatePosition()
		{
			var rect = User32.GetHearthstoneRect(true);
			Left = rect.Left;
			Top = rect.Top;
		}

		private void UpdateSize()
		{
			var rect = User32.GetHearthstoneRect(true);
			ContentGrid.Width = Width = rect.Width;
			ContentGrid.Height = Height = rect.Height;
		}
		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateOpacityMask(OverlayOpacityMask? opacityMask) => ContentGrid.OpacityMask = opacityMask?.Mask;
		public void UpdateBackground() => OnPropertyChanged(nameof(BackgroundColor));

		public void UpdateContentVisibility() => OnPropertyChanged(nameof(ContentVisibility));

		private void CapturableOverlayWindow_OnSourceInitialized(object sender, EventArgs e)
		{
			var hwnd = new WindowInteropHelper(this).Handle;
			User32.SetWindowExStyle(hwnd, User32.WsExNoActivate | User32.WsExTransparent);
			User32.SendWindowToBack(hwnd);
		}

		private void CapturableOverlayWindow_OnStateChanged(object sender, EventArgs e)
		{
			if(ForcedWindowState.HasValue)
				WindowState = ForcedWindowState.Value;

			// restoring from minimized raises the window, so re-pin it behind Hearthstone
			if(WindowState != WindowState.Minimized)
				User32.SendWindowToBack(new WindowInteropHelper(this).Handle);
		}

		// WsExNoActivate only blocks click activation, alt-tab can still bring this to the front
		private void CapturableOverlayWindow_OnActivated(object sender, EventArgs e)
		{
			User32.SendWindowToBack(new WindowInteropHelper(this).Handle);
			if(Helper.GameWindowState != WindowState.Minimized)
				User32.BringHsToForeground();
		}
	}
}
