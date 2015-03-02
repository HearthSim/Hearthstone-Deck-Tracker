#region

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Other
{
	/// <summary>
	/// Interaction logic for OtherLogging.xaml
	/// </summary>
	public partial class OtherLogging
	{
		private bool _initialized;

		public OtherLogging()
		{
			InitializeComponent();
		}

		public void Load()
		{
			ComboBoxLogLevel.SelectedValue = Config.Instance.LogLevel.ToString();
			_initialized = true;
		}

		private async void BtnSaveLog_OnClick(object sender, RoutedEventArgs e)
		{
			var date = DateTime.Now;
			var logName = string.Format("log_{0}{1}{2}-{3}{4}{5}.txt", date.Day, date.Month, date.Year, date.Hour, date.Minute, date.Second);
			var fileName = Helper.ShowSaveFileDialog(logName, "txt");

			if(fileName != null)
			{
				using(var sr = new StreamWriter(fileName, false))
					sr.Write(TextBoxLog.Text);

				await Helper.MainWindow.ShowSavedFileMessage(fileName);
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
	}
}