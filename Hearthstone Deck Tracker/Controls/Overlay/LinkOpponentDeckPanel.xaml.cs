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
	public partial class LinkOpponentDeckPanel : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private LinkOpponentDeckState _linkOpponentDeckState;

		private Visibility _linkOpponentDeckVisibility = Visibility.Hidden;
		public Visibility LinkOpponentDeckVisibility
		{
			get => _linkOpponentDeckVisibility;
			set
			{
				_linkOpponentDeckVisibility = value;
				OnPropertyChanged();
			}
		}

		public Visibility DescriptorVisibility => !Config.Instance.InteractedWithLinkOpponentDeck ? Visibility.Visible : Visibility.Collapsed;
		
		private string _errorMessage;
		public string ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				OnPropertyChanged();
			}
		}

		public bool AutoShown = false;

		private bool _shownByOpponentStack = false;

		private bool _mouseIsOver = false;

		private bool _hasLinkedDeck = false;

		public string LinkMessage => LinkOpponentDeckStateConverter.GetLinkMessage(_linkOpponentDeckState);

		public Visibility LinkMessageVisibility => (AutoShown && !Config.Instance.InteractedWithLinkOpponentDeck) || _hasLinkedDeck ? Visibility.Visible : Visibility.Collapsed;

		public Visibility ErrorMessageVisibility => !string.IsNullOrEmpty(ErrorMessage) ? Visibility.Visible : Visibility.Collapsed;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public LinkOpponentDeckPanel()
		{
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
					Core.UpdateOpponentCards();
					_linkOpponentDeckState = LinkOpponentDeckState.InKnownDeckMode;
					_hasLinkedDeck = true;
					ErrorMessage = "";
				}
				else
					_linkOpponentDeckState = LinkOpponentDeckState.Error;
			}
			catch(Exception ex)
			{
				_linkOpponentDeckState = LinkOpponentDeckState.Error;
				ErrorMessage = $"No valid deck code found on clipboard";
			}

			OnPropertyChanged(nameof(ErrorMessage));
			OnPropertyChanged(nameof(ErrorMessageVisibility));
			OnPropertyChanged(nameof(LinkMessage));
			OnPropertyChanged(nameof(LinkMessageVisibility));
		}

		public void HideByOpponentStack()
		{
			_shownByOpponentStack = false;
			Hide(false);
		}

		public void Hide(bool force = false)
		{
			if(force || !_mouseIsOver)
			{
				LinkOpponentDeckVisibility = Visibility.Hidden;
			}
			AutoShown = false;
			ErrorMessage = "";
			OnPropertyChanged(nameof(ErrorMessage));
			OnPropertyChanged(nameof(ErrorMessageVisibility));
		}

		public void ShowByOpponentStack()
		{
			_shownByOpponentStack = true;
			Show();
		}

		public void Show()
		{
			LinkOpponentDeckVisibility = Visibility.Visible;
			OnPropertyChanged(nameof(LinkMessageVisibility));
		}

		private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_mouseIsOver = true;
		}

		private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_mouseIsOver = false;
			if(!_shownByOpponentStack)
				Hide();
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
			}
		}
	}
}
