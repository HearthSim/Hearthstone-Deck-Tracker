#region

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb;
using Hearthstone_Deck_Tracker.Stats;
using static HearthDb.CardIds;
using static System.Windows.Visibility;

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
			DeckList.ItemsSource = game.OpponentCards
				.Select(x => new NoteCard(x))
				.OrderBy(x => x.Cost);
			Show();
			Activate();
			TextBoxNote.Focus();
			_initialized = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e) => SaveAndClose();

		private void SaveAndClose()
		{
			if(_game != null)
			{
				_game.Note = TextBoxNote.Text;
				DeckStatsList.Save();
			}
			Close();
		}

		private void DeckPanelVisibility(Visibility visibility, int span, string prefix)
		{
			DeckListContainer.Visibility = visibility;
			TextBoxNote.SetValue(Grid.ColumnSpanProperty, span);
			BtnDeck.Content = $"{prefix} OPP DECK";
		}

		private void BtnDeck_Click(object sender, RoutedEventArgs e)
		{
			if(DeckListContainer.Visibility == Visible)
				DeckPanelVisibility(Collapsed, 3, "SHOW");
			else
				DeckPanelVisibility(Visible, 2, "HIDE");

			TextBoxNote.Focus();
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

		private class NoteCard
		{
			public string Name { get; }
			public string CountText { get; }
			public Brush TextColor { get; }
			public int Cost { get; }

			public NoteCard() : this(null)
			{
			}

			public NoteCard(TrackedCard tracked)
			{
				Card card = null;
				if(tracked == null || !Cards.All.ContainsKey(tracked.Id))
				{
					card = Cards.All[NonCollectible.Neutral.Noooooooooooo];
					CountText = "x0";
					TextColor = Brushes.Red;
				}					
				else
				{
					card = Cards.All[tracked.Id];
					CountText = $"x{tracked.Count}";
					TextColor = tracked.Unconfirmed == 0 ? Brushes.Black : Brushes.Red;
				}
				Name = card.Name;				
				Cost = card.Cost;
			}
		}
	}
}