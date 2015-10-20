#region

using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for NoteDialog.xaml
	/// </summary>
	public partial class NoteDialog
	{
		private readonly GameStats _game;
		private readonly bool _initialized;

		public NoteDialog(GameStats game)
		{
			InitializeComponent();
			_game = game;
			CheckBoxEnterToSave.IsChecked = Config.Instance.EnterToSaveNote;
			TextBoxNote.Text = game.Note;
			Show();
			Activate();
			TextBoxNote.Focus();
			_initialized = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SaveAndClose();
		}

		private void SaveAndClose()
		{
			if(_game != null)
			{
				_game.Note = TextBoxNote.Text;
				DeckStatsList.Save();
				(Config.Instance.StatsInWindow ? Core.Windows.StatsWindow.StatsControl : Core.MainWindow.DeckStatsFlyout).Refresh();
			}
			Close();
		}

		private void TextBoxNote_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter && Config.Instance.EnterToSaveNote)
				SaveAndClose();
		}

		private void CheckBoxEnterToSave_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnterToSaveNote = true;
			Config.Save();
		}

		private void CheckBoxEnterToSave_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnterToSaveNote = false;
			Config.Save();
		}
	}
}