#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for DebugWindow.xaml
	/// </summary>
	public partial class DebugWindow : Window
	{
	    private readonly GameV2 _game;
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
				switch((string)ComboBoxData.SelectedValue)
				{
					case "Game":
						FilterGame();
						break;
					case "Entities":
						FilterEntities();
						break;
				}
				await Task.Delay(500);
			}
		}

		private void FilterEntities()
		{
			List<object> list = new List<object>();
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
					var row = DataGridProperties.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
					if(row != null)
						row.Background = new SolidColorBrush(Color.FromArgb(50, 0, 205, 0));
				}
				_previous = list;
			}
		}

		private string GetTagKeyValue(KeyValuePair<GAME_TAG, int> pair)
		{
			string value = pair.Value.ToString();
			switch(pair.Key)
			{
				case GAME_TAG.ZONE:
					value = Enum.Parse(typeof(TAG_ZONE), value).ToString();
					break;
				case GAME_TAG.CARDTYPE:
					value = Enum.Parse(typeof(TAG_CARDTYPE), value).ToString();
					break;
				case GAME_TAG.MULLIGAN_STATE:
					value = Enum.Parse(typeof(TAG_MULLIGAN), value).ToString();
					break;
				case GAME_TAG.PLAYSTATE:
					value = Enum.Parse(typeof(TAG_PLAYSTATE), value).ToString();
					break;
			}
			return pair.Key + ":" + value;
		}

		private void FilterGame()
		{
			List<object> list = new List<object>();
			var props = typeof(GameV2).GetProperties().OrderBy(x => x.Name);
			foreach(var prop in props)
			{
				if(prop.Name == "HSLogLines" || prop.Name == "Entities")
					continue;
				string val = "";
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
					var row = DataGridProperties.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
					if(row != null)
						row.Background = new SolidColorBrush(Color.FromArgb(50, 0, 205, 0));
				}
				_previous = list;
			}
		}
	}
}