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

		public string LinkMessage => OpponentUploadStateConverter.GetLinkMessage(_uploadState);

		public Visibility LinkMessageVisibility => !Config.Instance.SeenLinkOpponentDeck ? Visibility.Visible : Visibility.Collapsed;

		public string ErrorMessage => ClipboardImporter.ErrorMessage;

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

		private async void LinkOpponentDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = await ClipboardImporter.Import();
			if(deck != null)
			{
				Player.KnownOpponentDeck = deck;
				e.Handled = true;
				Core.UpdateOpponentCards();
				_uploadState = OpponentUploadState.InKnownDeckMode;
			}
			else
			{
				_uploadState = OpponentUploadState.Error;
				OnPropertyChanged(ErrorMessage);
			}

			OnPropertyChanged(nameof(LinkMessage));
		}

		public void Hide(bool force = false)
		{
			if(force || !_mouseIsOver)
			{
				UploadVisibility = Visibility.Hidden;
			}
		}

		public void Show()
		{
			UploadVisibility = Visibility.Visible;
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

		private void Hyperlink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if(!Config.Instance.SeenLinkOpponentDeck)
			{
				Config.Instance.SeenLinkOpponentDeck = true;
				Config.Save();
				Hide(true);
			}
			else
			{
				Player.KnownOpponentDeck = null;
				Core.UpdateOpponentCards();
				_uploadState = OpponentUploadState.Initial;
				OnPropertyChanged(nameof(LinkMessage));
			}
		}
	}
}
