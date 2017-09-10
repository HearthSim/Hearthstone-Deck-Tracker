using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Streaming
{
	public partial class StreamingCapturableOverlay : INotifyPropertyChanged
	{
		private readonly bool _initialized;

		public StreamingCapturableOverlay()
		{
			InitializeComponent();
			CheckBoxShowCapOverlay.IsChecked = Config.Instance.ShowCapturableOverlay;
			CheckBoxDisableOpacityTransition.IsChecked = !Config.Instance.OverlayCardAnimationsOpacity;
			_initialized = true;
		}

		public SolidColorBrush SelectedColor => Helper.BrushFromHex(Config.Instance.StreamingOverlayBackground);

		public event PropertyChangedEventHandler PropertyChanged;

		private void TextboxBackground_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var background = Helper.BrushFromHex(TextboxCustomBackground.Text);
			if(background == null)
				return;
			Config.Instance.StreamingOverlayBackground = TextboxCustomBackground.Text;
			Config.Save();
			OnPropertyChanged(nameof(SelectedColor));
			Core.Windows.CapturableOverlay?.UpdateBackground();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void CheckBoxShowCapOverlay_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowCapturableOverlay = true;
			Config.Save();
			Core.Windows.CapturableOverlay = new CapturableOverlayWindow();
			Core.Windows.CapturableOverlay.Show();
		}

		private void CheckBoxShowCapOverlay_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowCapturableOverlay = false;
			Config.Save();
			Core.Windows.CapturableOverlay?.Close();
		}


		private void Hyperlink_OnClick(object sender, RoutedEventArgs e) 
			=> Helper.TryOpenUrl("https://github.com/HearthSim/Hearthstone-Deck-Tracker/wiki/Streaming-Instructions");

		private void CheckBoxDisableOpacityTransition_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardAnimationsOpacity = false;
			Config.Save();
		}

		private void CheckBoxDisableOpacityTransition_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardAnimationsOpacity = true;
			Config.Save();
		}
	}
}
