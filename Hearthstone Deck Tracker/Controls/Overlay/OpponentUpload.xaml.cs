using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Importing;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class OpponentUpload : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private OpponentUploadState _uploadState;
		public OpponentUploadState UploadState
		{
			get => _uploadState;
			set
			{
				_uploadState = value;
				OnPropertyChanged();
			}
		}

		public string Message => OpponentUploadStateConverter.GetStatusMessage(_uploadState);

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public OpponentUpload()
		{
			InitializeComponent();
		}

		private void OpponentUpload_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Console.WriteLine("Upload mousedown clicked on");
			var deck = await ClipboardImporter.Import();
			if(deck != null)
			{
				
				e.Handled = true;
			}

		}

		private void CloseOpponentUpload_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{

		}
	}
}
