using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

		private Visibility _uploadVisibility = Visibility.Hidden;
		public Visibility UploadVisibility
		{
			get => _uploadVisibility;
			set
			{
				_uploadVisibility = value;
				OnPropertyChanged();
			}
		}

		private Visibility _descriptorVisibility = Visibility.Visible;
		public Visibility DescriptorVisibility
		{
			get => _descriptorVisibility;
			set
			{
				_descriptorVisibility = value;
				OnPropertyChanged();
			}
		}

		private bool _mouseIsOver = false;

		public bool WasClosed = false;

		public string Message => OpponentUploadStateConverter.GetStatusMessage(_uploadState);

		public string LinkMessage => "Dismiss/Clear";

		public Visibility LinkMessageVisibility => !string.IsNullOrEmpty(LinkMessage) ? Visibility.Visible : Visibility.Collapsed;

		public string ErrorMessage => "";

		public Visibility ErrorMessageVisibility => !string.IsNullOrEmpty(ErrorMessage) ? Visibility.Visible : Visibility.Collapsed;

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
			if(WasClosed)
				return;
			if(_uploadState == OpponentUploadState.Initial || _uploadState == OpponentUploadState.Error)
			{
				var deck = await ClipboardImporter.Import();
				if(deck != null)
				{
					Player.KnownOpponentDeck = deck;
					e.Handled = true;
					_uploadState = OpponentUploadState.UploadSucceeded;
					OnPropertyChanged(nameof(Message));
					Core.UpdateOpponentCards();
					await Task.Delay(2000);
					_uploadState = OpponentUploadState.InKnownDeckMode;
				}
				else
					_uploadState = OpponentUploadState.Error;
			}
			else if(_uploadState == OpponentUploadState.InKnownDeckMode)
			{
				Player.KnownOpponentDeck = null;
				Core.UpdateOpponentCards();

				await Task.Delay(2000);
				_uploadState = OpponentUploadState.Initial;
			}

			OnPropertyChanged(nameof(Message));
		}

		private void CloseOpponentUpload_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Hide(true);
			WasClosed = true;
		}

		public void Hide(bool force = false)
		{
			if(force || !_mouseIsOver)
			{
				UploadVisibility = Visibility.Hidden;
			}
		}

		public void Show(bool force = false)
		{
			if(force || !WasClosed)
			{
				UploadVisibility = Visibility.Visible;
			}
		}

		private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_mouseIsOver = true;
		}

		private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_mouseIsOver = false;
			Hide();
		}
	}
}
