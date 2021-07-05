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

		private bool _mouseIsOver = false;

		public string LinkMessage => LinkOpponentDeckStateConverter.GetLinkMessage(_linkOpponentDeckState);

		public Visibility LinkMessageVisibility => !Config.Instance.SeenLinkOpponentDeck ? Visibility.Visible : Visibility.Collapsed;

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
			Config.Instance.SeenLinkOpponentDeck = true;
			Config.Save();
			try
			{
				var deck = await ClipboardImporter.Import(true);
				if(deck != null)
				{
					Player.KnownOpponentDeck = deck;
					e.Handled = true;
					Core.UpdateOpponentCards();
					_linkOpponentDeckState = LinkOpponentDeckState.InKnownDeckMode;
				}
				else
					_linkOpponentDeckState = LinkOpponentDeckState.Error;
			}
			catch(Exception ex)
			{
				_linkOpponentDeckState = LinkOpponentDeckState.Error;
				ErrorMessage = ex.Message;
				OnPropertyChanged(ErrorMessage);
			}

			OnPropertyChanged(nameof(LinkMessage));
		}

		public void Hide(bool force = false)
		{
			if(force || !_mouseIsOver)
			{
				LinkOpponentDeckVisibility = Visibility.Hidden;
			}
		}

		public void Show()
		{
			LinkOpponentDeckVisibility = Visibility.Visible;
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
				OnPropertyChanged(nameof(LinkMessageVisibility));
				Config.Save();
				Hide(true);
			}
			else
			{
				Player.KnownOpponentDeck = null;
				Core.UpdateOpponentCards();
				_linkOpponentDeckState = LinkOpponentDeckState.Initial;
				OnPropertyChanged(nameof(LinkMessage));
				OnPropertyChanged(nameof(LinkMessageVisibility));
			}
		}
	}
}
