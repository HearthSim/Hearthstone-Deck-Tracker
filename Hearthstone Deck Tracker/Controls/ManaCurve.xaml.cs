#region

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for ManaCurve.xaml
	/// </summary>
	public partial class ManaCurve
	{
		private readonly ManaCostBar[] _manaCostBars;
		private Deck _deck;

		public ManaCurve()
		{
			InitializeComponent();

			_manaCostBars = new[]
			{
				ManaCostBar0,
				ManaCostBar1,
				ManaCostBar2,
				ManaCostBar3,
				ManaCostBar4,
				ManaCostBar5,
				ManaCostBar6,
				ManaCostBar7
			};
		}

		public void SetDeck(Deck deck)
		{
			if(deck == null)
			{
				ClearDeck();
				return;
			}
			_deck = deck;
			deck.GetSelectedDeckVersion().Cards.CollectionChanged += (sender, args) => UpdateValues();
			UpdateValues();
		}

		public void ClearDeck()
		{
			_deck = null;
			for(var i = 0; i < 8; i++)
			{
				_manaCostBars[i].SetValues(0, 0, 0, 0);
				_manaCostBars[i].SetTooltipValues(0, 0, 0);
			}
		}

		public void UpdateValues()
		{
			if(_deck == null)
				return;

			var counts = new int[8];
			var weapons = new int[8];
			var spells = new int[8];
			var minions = new int[8];
			foreach(var card in _deck.GetSelectedDeckVersion().Cards)
			{
				var statValue = -1;
				switch(Config.Instance.ManaCurveFilter)
				{
					case StatType.Mana:
						statValue = card.Cost;
						break;
					case StatType.Health:
						statValue = card.Health;
						break;
					case StatType.Attack:
						statValue = card.Attack;
						break;
				}
				if(statValue == -1)
					continue;
				if(statValue >= 7)
				{
					switch(card.Type)
					{
						case "Weapon":
							weapons[7] += card.Count;
							break;
						case "Enchantment":
						case "Spell":
							spells[7] += card.Count;
							break;
						case "Minion":
							minions[7] += card.Count;
							break;
					}
					counts[7] += card.Count;
				}
				else
				{
					if(Config.Instance.ManaCurveFilter == StatType.Mana)
					{
						switch(card.Type)
						{
							case "Weapon":
								weapons[statValue] += card.Count;
								break;
							case "Enchantment":
							case "Spell":
								spells[statValue] += card.Count;
								break;
							case "Minion":
								minions[statValue] += card.Count;
								break;
						}
						counts[statValue] += card.Count;
					}
					else if(card.Type == "Minion")
					{
						minions[statValue] += card.Count;
						counts[statValue] += card.Count;
					}
				}
			}
			var max = 0;
			for(var i = 0; i < 8; i++)
			{
				var sum = weapons[i] + spells[i] + minions[i];
				if(sum > max)
					max = sum;
			}

			for(var i = 0; i < 8; i++)
			{
				if(max == 0)
				{
					_manaCostBars[i].SetValues(0, 0, 0, 0);
					_manaCostBars[i].SetTooltipValues(0, 0, 0);
				}
				else
				{
					_manaCostBars[i].SetValues(100d * weapons[i] / max, 100d * spells[i] / max, 100d * minions[i] / max, counts[i]);
					_manaCostBars[i].SetTooltipValues(weapons[i], spells[i], minions[i]);
				}
			}
		}

		private void ListViewStatType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(e.AddedItems.Count == 0)
			{
				if(e.RemovedItems.Count > 0)
				{
					var item = e.RemovedItems[0] as ListViewItem;
					if(item != null)
					{
						ListViewStatType.SelectedItem = item;
						return;
					}
				}
				//Config.Instance.ManaCurveFilter = StatType.Mana;
			}
			else
			{
				var item = e.AddedItems[0] as ListViewItem;
				if(item != null)
				{
					switch(item.Name)
					{
						case "ListViewItemMana":
							Config.Instance.ManaCurveFilter = StatType.Mana;
							break;
						case "ListViewItemHealth":
							Config.Instance.ManaCurveFilter = StatType.Health;
							break;
						case "ListViewItemAttack":
							Config.Instance.ManaCurveFilter = StatType.Attack;
							break;
					}
				}
			}
			Config.Save();
			UpdateValues();
		}
	}
}