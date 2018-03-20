using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using HSReplay.OAuth;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class OAuthLogin : INotifyPropertyChanged
	{
		public static readonly DependencyProperty ScopeConsideredLoggedInProperty =
			DependencyProperty.Register("ScopeConsideredLoggedIn", typeof(Scope), typeof(OAuthLogin),
				new PropertyMetadata(default(Scope)));

		private bool _isAuthenticating;
		private bool _showContactUs;
		private bool _showTryAgain;

		public static readonly DependencyProperty SuccessUrlProperty =
			DependencyProperty.Register("SuccessUrl", typeof(string), typeof(OAuthLogin),
				new PropertyMetadata(null));

		public static readonly DependencyProperty ErrorUrlProperty =
			DependencyProperty.Register("ErrorUrl", typeof(string), typeof(OAuthLogin),
				new PropertyMetadata(null));

		public OAuthLogin()
		{
			InitializeComponent();
			HSReplayNetHelper.Authenticating += OnAuthenticating;
			HSReplayNetOAuth.Authenticated += () => OnPropertyChanged(nameof(IsAuthenticated));
			HSReplayNetOAuth.LoggedOut += () => OnPropertyChanged(nameof(IsAuthenticated));
		}

		public bool IsAuthenticated => HSReplayNetOAuth.IsFullyAuthenticated
										|| ScopeConsideredLoggedIn != null
										&& HSReplayNetOAuth.IsAuthenticatedFor(ScopeConsideredLoggedIn);

		public ICommand LoginCommand
			=> new Command(() => HSReplayNetHelper.TryAuthenticate(SuccessUrl, ErrorUrl).Forget());

		public ICommand TryAgainCommand => new Command(() =>
		{
			LoginCommand.Execute(null);
			ShowTryAgain = false;
		});

		public bool IsAuthenticating
		{
			get => _isAuthenticating;
			set
			{
				if(_isAuthenticating != value)
				{
					_isAuthenticating = value;
					OnPropertyChanged();
				}
			}
		}

		public bool ShowTryAgain
		{
			get => _showTryAgain;
			set
			{
				if(_showTryAgain != value)
				{
					_showTryAgain = value;
					OnPropertyChanged();
				}
			}
		}

		public bool ShowContactUs
		{
			get => _showContactUs;
			set
			{
				if(_showContactUs != value)
				{
					_showContactUs = value;
					OnPropertyChanged();
				}
			}
		}

		public Scope ScopeConsideredLoggedIn
		{
			get => (Scope) GetValue(ScopeConsideredLoggedInProperty);
			set => SetValue(ScopeConsideredLoggedInProperty, value);
		}

		public string SuccessUrl
		{
			get => (string)GetValue(SuccessUrlProperty);
			set => SetValue(SuccessUrlProperty, value);
		}

		public string ErrorUrl
		{
			get => (string)GetValue(ErrorUrlProperty);
			set => SetValue(ErrorUrlProperty, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnAuthenticating(bool authenticating)
		{
			IsAuthenticating = authenticating;
			if(authenticating)
			{
				Task.Run(async () =>
				{
					await Task.Delay(TimeSpan.FromSeconds(20));
					if(IsAuthenticated)
						return;
					ShowTryAgain = true;
					await Task.Delay(TimeSpan.FromSeconds(20));
					if(IsAuthenticated)
						return;
					ShowContactUs = true;
				}).Forget();
			}
			else
			{
				ShowTryAgain = false;
				ShowContactUs = false;
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
