#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for StartupWindow.xaml
	/// </summary>
	public partial class LoginWindow : INotifyPropertyChanged
	{
	    private readonly GameV2 _game;
	    private readonly bool _initialized;
		private ProgressDialogController _controller;
		private Visibility _loginRegisterVisibility;

		public LoginWindow()
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			_game = new GameV2();
            Card.SetGame(_game);
			API.Core.Game = _game;
		    InitializeComponent();
			Logger.Initialzie();
			Config.Load();
			if(HearthStatsAPI.LoadCredentials() || !Config.Instance.ShowLoginDialog)
				StartMainApp();
			CheckBoxRememberLogin.IsChecked = Config.Instance.RememberHearthStatsLogin;
			_initialized = true;
		}

		public double TabWidth
		{
			get { return ActualWidth / 2; }
		}

		public Visibility LoginRegisterVisibility
		{
			get { return _loginRegisterVisibility; }
			set
			{
				_loginRegisterVisibility = value;
				OnPropertyChanged();
				OnPropertyChanged("ContinueAsGuestVisibility");
			}
		}

		public Visibility ContinueAsGuestVisibility
		{
			get { return LoginRegisterVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		private void StartMainApp()
		{
			IsEnabled = false;
			var mainWindow = new MainWindow(_game);
			try
			{
				mainWindow.Show();
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error showing main window: " + ex, "LoginWindow");
			}
			Close();
		}

		private async void BtnLogin_Click(object sender, RoutedEventArgs e)
		{
			var email = TextBoxEmail.Text;
			if(string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @".*@.*\..*"))
			{
				DisplayLoginError("Please enter an valid email address");
				return;
			}
			if(string.IsNullOrEmpty(TextBoxPassword.Password))
			{
				DisplayLoginError("Please enter a password");
				return;
			}
			IsEnabled = false;
			_controller = await this.ShowProgressAsync("Logging in...", "");
			var result = await HearthStatsAPI.LoginAsync(TextBoxEmail.Text, TextBoxPassword.Password);
			TextBoxPassword.Clear();
			if(result.Success)
				StartMainApp();
			else if(result.Message.Contains("401"))
				DisplayLoginError("Invalid email or password");
			else
				DisplayLoginError(result.Message);
		}

		private async void DisplayLoginError(string error)
		{
			TextBlockErrorMessage.Text = error;
			TextBlockErrorMessage.Visibility = Visibility.Visible;
			IsEnabled = true;
            if (_controller != null)
            {
                if (_controller.IsOpen)
                {
                    await _controller.CloseAsync();
                }
            }
				
		}

		private void CheckBoxRememberLogin_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RememberHearthStatsLogin = true;
			Config.Save();
		}

		private void CheckBoxRememberLogin_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RememberHearthStatsLogin = false;
			Config.Save();
			try
			{
				if(File.Exists(Config.Instance.HearthStatsFilePath))
					File.Delete(Config.Instance.HearthStatsFilePath);
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error deleting hearthstats credentials file\n" + ex, "HearthStatsAPI");
			}
		}

		private async void BtnRegister_Click(object sender, RoutedEventArgs e)
		{
			if(!CheckBoxPrivacyPolicy.IsChecked == true)
				return;

			var email = TextBoxRegisterEmail.Text;
			if(string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @".*@.*\..*"))
			{
				DisplayLoginError("Please enter an valid email address");
				return;
			}
			if(string.IsNullOrEmpty(TextBoxRegisterPassword.Password))
			{
				DisplayLoginError("Please enter a password");
				return;
			}
			if(TextBoxRegisterPassword.Password.Length < 6)
			{
				DisplayLoginError("Your password needs to be at least 6 characters");
				return;
			}
			if(string.IsNullOrEmpty(TextBoxRegisterPasswordConfirm.Password))
			{
				DisplayLoginError("Please confirm your password");
				return;
			}
			if(!TextBoxRegisterPassword.Password.Equals(TextBoxRegisterPasswordConfirm.Password))
			{
				DisplayLoginError("Entered passwords do not match");
				return;
			}
			IsEnabled = false;
			_controller = await this.ShowProgressAsync("Registering account...", "");
			var result = await HearthStatsAPI.RegisterAsync(email, TextBoxRegisterPassword.Password);
			if(result.Success)
			{
				_controller.SetTitle("Logging in...");
				result = await HearthStatsAPI.LoginAsync(email, TextBoxRegisterPassword.Password);
			}
			else if(result.Message.Contains("422"))
				DisplayLoginError("Email already registered");
			else
				DisplayLoginError(result.Message);
			TextBoxRegisterPassword.Clear();
			TextBoxRegisterPasswordConfirm.Clear();
			if(result.Success)
			{
				var mw = new MainWindow(_game);
				mw.Show();
				Close();
			}
		}

		private void CheckBoxPrivacyPolicy_Checked(object sender, RoutedEventArgs e)
		{
			BtnRegister.IsEnabled = true;
		}

		private void CheckBoxPrivacyPolicy_OnUnchecked(object sender, RoutedEventArgs e)
		{
			BtnRegister.IsEnabled = false;
		}

		private void Button_Continue(object sender, RoutedEventArgs e)
		{
			LoginRegisterVisibility = Visibility.Collapsed;
			TabControlLoginRegister.SelectedIndex = 2;
		}

		private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
		{
			LoginRegisterVisibility = Visibility.Visible;
			TabControlLoginRegister.SelectedIndex = 1;
		}

		private void Button_ContinueAnyway(object sender, RoutedEventArgs e)
		{
			Logger.WriteLine("Continuing as guest...");
			StartMainApp();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}