using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class SelectLanguageDialog : CustomDialog
	{
		private readonly TaskCompletionSource<SelectLanguageOperation> _tcs = new TaskCompletionSource<SelectLanguageOperation>();
		internal Task<SelectLanguageOperation> WaitForButtonPressAsync() => _tcs.Task;
		public string SelectedLanguage => Helper.LanguageDict.FirstOrDefault(x => x.Key == LanguagesComboBox.SelectedItem.ToString()).Value ?? Helper.defaultLanguageShort;

		public SelectLanguageDialog()
		{
			InitializeComponent();

			var LanguagesList = Helper.LanguageDict.Keys.ToList();
			LanguagesComboBox.ItemsSource = LanguagesList;
			LanguagesComboBox.SelectedIndex = LanguagesList.IndexOf(Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key);
		}

		private void ButtonCopy_OnClick(object sender, RoutedEventArgs e) => CloseDialog(SelectedLanguage);

		private void ButtonCancel_OnClick(object sender, RoutedEventArgs e) => CloseDialog(isCanceled:true);

		private void CloseDialog(string SelectedLanguage = Helper.defaultLanguageShort, bool isCanceled = false)
		{
			ButtonCopy.IsEnabled = false;
			ButtonCancel.IsEnabled = false;

			SelectLanguageOperation LanguageOperation = new SelectLanguageOperation
			{
				SelectedLanguage = SelectedLanguage,
				isCanceled = isCanceled
			};

			_tcs.SetResult(LanguageOperation);
		}
	}
}
