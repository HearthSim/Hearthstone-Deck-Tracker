#region

// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;

// ReSharper enable RedundantUsingDirective

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherLogging.xaml
	/// </summary>
	public partial class TrackerLogging
	{
		private GameV2 _game;
		private bool _initialized;

		public TrackerLogging()
		{
			InitializeComponent();
		}

		public void Load(GameV2 game)
		{
			_game = game;
			ComboBoxLogLevel.SelectedValue = Config.Instance.LogLevel.ToString();
			_initialized = true;
		}

		private async void BtnSaveLog_OnClick(object sender, RoutedEventArgs e)
		{
			var date = DateTime.Now;
			var logName = $"log_{date.Day}{date.Month}{date.Year}-{date.Hour}{date.Minute}{date.Second}.txt";
			var fileName = Helper.ShowSaveFileDialog(logName, "txt");

			if(fileName != null)
			{
				using(var sr = new StreamWriter(fileName, false))
					sr.Write(TextBoxLog.Text);

				await Core.MainWindow.ShowSavedFileMessage(fileName);
			}
		}

		private void BtnClear_OnClick(object sender, RoutedEventArgs e)
		{
			TextBoxLog.Text = "";
		}

		private void ComboBoxLogLevel_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.LogLevel = int.Parse(ComboBoxLogLevel.SelectedValue.ToString());
			Config.Save();
		}

		private void BtnDebugWindow_OnClick(object sender, RoutedEventArgs e)
		{
			var debugWindow = new DebugWindow(_game);
			debugWindow.Show();
		}
	}
}