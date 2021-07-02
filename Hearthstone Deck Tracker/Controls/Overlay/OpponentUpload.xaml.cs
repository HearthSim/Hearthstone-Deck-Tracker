using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
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

		private Visibility _uploadVisibility;
		public Visibility UploadVisibility
		{
			get => _uploadVisibility;
			set
			{
				_uploadVisibility = value;
				OnPropertyChanged();
			}
		}

		public bool WasClosed = false;

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

		private async void OpponentUpload_MouseDownAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Console.WriteLine("Upload mousedown clicked on");
			if(UploadState == OpponentUploadState.Initial || UploadState == OpponentUploadState.Error)
			{
				var deck = await ClipboardImporter.Import();
				if(deck != null)
				{
					Player.KnownOpponentDeck = deck;
					Console.WriteLine("successfully uploaded deck");
					e.Handled = true;
					UploadState = OpponentUploadState.UploadSucceeded;
				}
				else
					UploadState = OpponentUploadState.Error;
			}
			else if(UploadState == OpponentUploadState.UploadSucceeded) { }

			OnPropertyChanged(nameof(Message));

		}

		private void CloseOpponentUpload_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			UploadVisibility = Visibility.Collapsed;
			WasClosed = true;
			Console.WriteLine("wants to close thing");
		}
	}
}
