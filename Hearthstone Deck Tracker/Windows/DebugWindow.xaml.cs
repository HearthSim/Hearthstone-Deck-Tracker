#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for DebugWindow.xaml
	/// </summary>
	public partial class DebugWindow : Window
	{
		private readonly GameV2 _game;
		private readonly List<string> _expanded = new List<string>();
		private List<object> _previous = new List<object>();
		private bool _update;

		public DebugWindow(GameV2 game)
		{
			_game = game;
			InitializeComponent();
			_update = true;
			Closing += (sender, args) => _update = false;
			Update();
		}

		private async void Update()
		{
			while(_update)
			{
				if(TabControlDebug.SelectedIndex == 0)
					UpdateCards();
				else if(TabControlDebug.SelectedIndex == 2)
					UpdateBoardDamage();
				else
				{
					switch((string)ComboBoxData.SelectedValue)
					{
						case "Game":
							FilterGame();
							break;
						case "Entities":
							FilterEntities();
							break;
					}
				}
				await Task.Delay(500);
			}
		}

		private void UpdateCards()
		{
			TreeViewCards.Items.Clear();
			var collections = new[]
			{
				new CollectionItem(_game.Player.Hand, "Player Hand"),
				new CollectionItem(_game.Player.Board, "Player Board"),
				new CollectionItem(_game.Player.Deck, "Player Deck"),
				new CollectionItem(_game.Player.Graveyard, "Player Graveyard"),
				new CollectionItem(_game.Player.Secrets, "Player Secrets"),
				new CollectionItem(_game.Player.RevealedEntities, "Player RevealedEntities"),
				new CollectionItem(_game.Player.SetAside, "Player SetAside"),
				new CollectionItem(_game.Opponent.Hand, "Opponent Hand"),
				new CollectionItem(_game.Opponent.Board, "Opponent Board"),
				new CollectionItem(_game.Opponent.Deck, "Opponent Deck"),
				new CollectionItem(_game.Opponent.Graveyard, "Opponent Graveyard"),
				new CollectionItem(_game.Opponent.Secrets, "Opponent Secrets"),
				new CollectionItem(_game.Opponent.RevealedEntities, "Opponent RevealedEntities"),
				new CollectionItem(_game.Opponent.SetAside, "Opponent SetAside")
			};
			foreach(var collection in collections)
			{
				var tvi = new TreeViewItem();
				tvi.Header = collection.Name;
				tvi.IsExpanded = _expanded.Contains(tvi.Header);
				tvi.Expanded += OnItemExpanded;
				tvi.Collapsed += OnItemCollapsed;
				foreach(var item in collection.Collection)
					tvi.Items.Add(item.ToString());
				TreeViewCards.Items.Add(tvi);
			}
		}

		private void UpdateBoardDamage()
		{
			if(!Core.Game.SetupDone)
				return;
			var board = new BoardState();
			PlayerDataGrid.ItemsSource = board.Player.Cards;
			OpponentDataGrid.ItemsSource = board.Opponent.Cards;
			PlayerHeader.Text = "Player " + board.Player;
			OpponentHeader.Text = "Opponent " + board.Opponent;
			DamageView.UpdateLayout();
		}

		private void FilterEntities()
		{
			var list = new List<object>();
			foreach(var entity in _game.Entities)
			{
				var tags = entity.Value.Tags.Select(GetTagKeyValue).Aggregate((c, n) => c + " | " + n);
				var card = Database.GetCardFromId(entity.Value.CardId);
				var cardName = card != null ? card.Name : "";
				var name = string.IsNullOrEmpty(entity.Value.Name) ? cardName : entity.Value.Name;
				list.Add(new {Name = name, entity.Value.CardId, Tags = tags});
			}

			var firstNotSecond = list.Except(_previous).ToList();
			var secondNotFirst = _previous.Except(list).ToList();
			if(firstNotSecond.Any() || secondNotFirst.Any())
			{
				DataGridProperties.ItemsSource = list;
				DataGridProperties.UpdateLayout();
				foreach(var item in firstNotSecond)
				{
					if(DataGridProperties.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
						row.Background = new SolidColorBrush(Color.FromArgb(50, 0, 205, 0));
				}
				_previous = list;
			}
		}

		private string GetTagKeyValue(KeyValuePair<GameTag, int> pair)
		{
			var value = pair.Value.ToString();
			switch(pair.Key)
			{
				case GameTag.ZONE:
					value = Enum.Parse(typeof(Zone), value).ToString();
					break;
				case GameTag.CARDTYPE:
					value = Enum.Parse(typeof(CardType), value).ToString();
					break;
				case GameTag.MULLIGAN_STATE:
					value = Enum.Parse(typeof(Mulligan), value).ToString();
					break;
				case GameTag.PLAYSTATE:
					value = Enum.Parse(typeof(PlayState), value).ToString();
					break;
			}
			return pair.Key + ":" + value;
		}

		private void FilterGame()
		{
			var list = new List<object>();
			var props = typeof(GameV2).GetProperties().OrderBy(x => x.Name);
			foreach(var prop in props)
			{
				if(prop.Name == "HSLogLines" || prop.Name == "Entities")
					continue;
				var val = "";
				var propVal = prop.GetValue(_game, null);
				if(propVal != null)
				{
					var enumerable = propVal as IEnumerable<object>;
					var collection = propVal as ICollection;
					if(enumerable != null)
					{
						enumerable = enumerable.Where(x => x != null);
						if(enumerable.Any())
							val = enumerable.Select(x => x.ToString()).Aggregate((c, n) => c + ", " + n);
					}
					else if(collection != null)
					{
						var objects = collection.Cast<object>().Where(x => x != null);
						if(objects.Any())
							val = objects.Select(x => x.ToString()).Aggregate((c, n) => c + ", " + n);
					}
					else
						val = propVal.ToString();
				}
				list.Add(new {Property = prop.Name, Value = val});
			}
			var firstNotSecond = list.Except(_previous).ToList();
			var secondNotFirst = _previous.Except(list).ToList();
			if(firstNotSecond.Any() || secondNotFirst.Any())
			{
				DataGridProperties.ItemsSource = list;
				DataGridProperties.UpdateLayout();
				foreach(var item in firstNotSecond)
				{
					if(DataGridProperties.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
						row.Background = new SolidColorBrush(Color.FromArgb(50, 0, 205, 0));
				}
				_previous = list;
			}
		}

		private void OnItemCollapsed(object sender, RoutedEventArgs e)
		{
			var item = sender as TreeViewItem;
			var header = item.Header.ToString();
			if(_expanded.Contains(header))
				_expanded.Remove(header);
		}

		private void OnItemExpanded(object sender, RoutedEventArgs e)
		{
			var item = sender as TreeViewItem;
			var header = item.Header.ToString();
			if(_expanded.Contains(header) == false)
				_expanded.Add(header);
		}

		private void ExpandAllBtn_Click(object sender, RoutedEventArgs e)
		{
			foreach(var item in TreeViewCards.Items)
			{
				var tvi = item as TreeViewItem;
				tvi.IsExpanded = true;
			}
		}

		private void CollapseAllBtn_Click(object sender, RoutedEventArgs e)
		{
			foreach(var item in TreeViewCards.Items)
			{
				var tvi = item as TreeViewItem;
				tvi.IsExpanded = false;
			}
		}

		public class CollectionItem
		{
			public CollectionItem(IEnumerable<Entity> collection, string name)
			{
				Collection = collection;
				Name = name;
			}

			public IEnumerable<Entity> Collection { get; set; }
			public string Name { get; set; }
		}
	}
}
