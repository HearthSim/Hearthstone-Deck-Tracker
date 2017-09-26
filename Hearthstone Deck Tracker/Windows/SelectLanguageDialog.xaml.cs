using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class SelectLanguageDialog : CustomDialog
	{
		private const string DefaultLanguage = "enUS";
		private readonly TaskCompletionSource<SelectLanguageOperation> _tcs = new TaskCompletionSource<SelectLanguageOperation>();
		internal Task<SelectLanguageOperation> WaitForButtonPressAsync() => _tcs.Task;
		public string SelectedLanguage => Helper.LanguageDict.FirstOrDefault(x => x.Key == LanguagesComboBox.SelectedItem.ToString()).Value ?? DefaultLanguage;

		public SelectLanguageDialog()
		{
			InitializeComponent();

			var languages = Helper.LanguageDict.Keys.Where(x => x != "English (Great Britain)").ToList();
			LanguagesComboBox.ItemsSource = languages;
			LanguagesComboBox.SelectedIndex = languages.IndexOf(Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key);
		}

		private void ButtonCopy_OnClick(object sender, RoutedEventArgs e) => CloseDialog(SelectedLanguage);

		private void ButtonCancel_OnClick(object sender, RoutedEventArgs e) => CloseDialog(isCanceled:true);

		private void CloseDialog(string selectedLanguage = DefaultLanguage, bool isCanceled = false)
		{
			ButtonCopy.IsEnabled = false;
			ButtonCancel.IsEnabled = false;

			var languageOperation = new SelectLanguageOperation
			{
				SelectedLanguage = selectedLanguage,
				IsCanceled = isCanceled
			};

			_tcs.SetResult(languageOperation);
		}
	}
}
