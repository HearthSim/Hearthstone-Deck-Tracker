#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.HearthStats.API;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.Controls
{
	/// <summary>
	/// Interaction logic for LoginControl.xaml
	/// </summary>
	public partial class LoginControl : UserControl
	{
		private readonly bool _inizialized;

		public LoginControl()
		{
			InitializeComponent();
			CheckBoxRememberLogin.IsChecked = Config.Instance.RememberHearthStatsLogin;
			_inizialized = true;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
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
			var result = await HearthStatsAPI.Login(TextBoxEmail.Text, TextBoxPassword.Password);
			if(result.Success)
				Helper.MainWindow.FlyoutHearthStatsLogin.IsOpen = false;
			else
				DisplayLoginError(result.Message);
		}

		private void DisplayLoginError(string error)
		{

			TextBlockErrorMessage.Text = "Error:\n" + error;
			TextBlockErrorMessage.Visibility = Visibility.Visible;
			IsEnabled = true;
		}

		private void CheckBoxRememberLogin_Checked(object sender, RoutedEventArgs e)
		{
			if(!_inizialized)
				return;
			Config.Instance.RememberHearthStatsLogin = true;
			Config.Save();
		}

		private void CheckBoxRememberLogin_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_inizialized)
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
				Logger.WriteLine("Error deleting hearthstats credentials file\n" + ex);
			}
		}
	}
}