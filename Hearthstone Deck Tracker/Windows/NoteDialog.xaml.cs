﻿#region

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
            Show();
            Activate();
            TextBoxNote.Focus();
            if (game.GameMode == Enums.GameMode.Ranked)
            {
                ComboBoxOpponentRank.SelectedIndex = game.Rank - 1;
            }
            else
            {
                FormGrid.RowDefinitions[1].Height = new GridLength(0);
            }            
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
                if (_game.GameMode == Enums.GameMode.Ranked)
                {
                    _game.OpponentRank = ComboBoxOpponentRank.SelectedIndex + 1;
                }
                DeckStatsList.Save();
				(Config.Instance.StatsInWindow ? Helper.MainWindow.StatsWindow.StatsControl : Helper.MainWindow.DeckStatsFlyout).Refresh();
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