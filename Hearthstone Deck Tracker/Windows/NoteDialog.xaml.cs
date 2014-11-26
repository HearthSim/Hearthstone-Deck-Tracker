using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls;

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
			_initialized = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if(_game != null)
				_game.Note = TextBoxNote.Text;
			Close();
		}

		private void TextBoxNote_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter && Config.Instance.EnterToSaveNote)
			{
				if(_game != null)
					_game.Note = TextBoxNote.Text;
				Close();
			}
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
