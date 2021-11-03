using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class LinkOpponentDeckPanel : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private LinkOpponentDeckState _linkOpponentDeckState;

		public Visibility DescriptorVisibility => !Config.Instance.InteractedWithLinkOpponentDeck || !_sessionStartHasInteracted ? Visibility.Visible : Visibility.Collapsed;
		
		private string? _errorMessage;
		public string? ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				OnPropertyChanged();
			}
		}

		private bool _autoShown = false;

		public bool IsFriendlyMatch = false;

		private bool _hasLinkedDeck = false;

		private bool _sessionStartHasInteracted = false;

		public string LinkMessage => LinkOpponentDeckStateConverter.GetLinkMessage(_linkOpponentDeckState);

		public Visibility LinkMessageVisibility => (_autoShown && !Config.Instance.InteractedWithLinkOpponentDeck) || _hasLinkedDeck ? Visibility.Visible : Visibility.Collapsed;

		public Visibility ErrorMessageVisibility => !string.IsNullOrEmpty(ErrorMessage) ? Visibility.Visible : Visibility.Collapsed;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public LinkOpponentDeckPanel()
		{
			_sessionStartHasInteracted = Config.Instance.InteractedWithLinkOpponentDeck;
			InitializeComponent();
		}

		private async void LinkOpponentDeck_Click(object sender, RoutedEventArgs e)
		{
			Config.Instance.InteractedWithLinkOpponentDeck = true;
			Config.Save();
			OnPropertyChanged(nameof(DescriptorVisibility));
			try
			{
				var deck = await ClipboardImporter.Import(true);
				if(deck != null)
				{
					Player.KnownOpponentDeck = deck;
					e.Handled = true;
					Core.UpdateOpponentCards(true);
					_linkOpponentDeckState = LinkOpponentDeckState.InKnownDeckMode;
					_hasLinkedDeck = true;
					ErrorMessage = "";
				}
				else
					_linkOpponentDeckState = LinkOpponentDeckState.Error;
			}
			catch
			{
				_linkOpponentDeckState = LinkOpponentDeckState.Error;
				ErrorMessage = LocUtil.Get("LinkOpponentDeck_NoValidDeckOnClipboardMessage");
			}

			OnPropertyChanged(nameof(ErrorMessage));
			OnPropertyChanged(nameof(ErrorMessageVisibility));
			OnPropertyChanged(nameof(LinkMessage));
			OnPropertyChanged(nameof(LinkMessageVisibility));
		}

		private bool _show;
		public async void Hide(bool force = false)
		{
			_show = false;
			if(!force)
			{
				await Task.Delay(200);
				if(_show)
					return;
			}
			Visibility = Visibility.Hidden;
			_autoShown = false;
			ErrorMessage = "";
			OnPropertyChanged(nameof(ErrorMessage));
			OnPropertyChanged(nameof(ErrorMessageVisibility));
		}

		public async void Show(bool auto)
		{
			if(IsFriendlyMatch || Config.Instance.EnableLinkOpponentDeckInNonFriendly)
			{
				_show = true;
				if(!auto)
				{
					await Task.Delay(200);
					if(!_show)
						return;
				}
				Visibility = Visibility.Visible;
				OnPropertyChanged(nameof(LinkMessageVisibility));
			}
		}

		private void Hyperlink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if(!Config.Instance.InteractedWithLinkOpponentDeck)
			{
				Config.Instance.InteractedWithLinkOpponentDeck = true;
				Config.Save();
				OnPropertyChanged(nameof(LinkMessageVisibility));
				OnPropertyChanged(nameof(DescriptorVisibility));
				Hide(true);
			}
			else
			{
				Player.KnownOpponentDeck = null;
				Core.UpdateOpponentCards();
				_linkOpponentDeckState = LinkOpponentDeckState.Initial;
				_hasLinkedDeck = false;
				OnPropertyChanged(nameof(LinkMessage));
				OnPropertyChanged(nameof(LinkMessageVisibility));
				Hide(true);
			}
		}
	}
}
