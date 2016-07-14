#region

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using static System.Windows.Visibility;
using static Hearthstone_Deck_Tracker.Enums.StatType;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for ManaCurve.xaml
	/// </summary>
	public partial class ManaCurve
	{
		private const string Weapon = "Weapon";
		private const string Enchantment = "Enchantment";
		private const string Spell = "Spell";
		private const string Minion = "Minion";
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

			ComboBoxStatType.ItemsSource = Enum.GetValues(typeof(StatType)).Cast<StatType>().Select(st => new StatTypeWrapper {StatType = st});
			ComboBoxStatType.SelectedIndex = (int)Config.Instance.ManaCurveFilter;
		}

		public void SetDeck(Deck deck)
		{
			if(deck == null)
			{
				ClearDeck();
				TextBlockNoMechanics.Visibility = Visible;
				return;
			}
			_deck = deck;
			deck.GetSelectedDeckVersion().Cards.CollectionChanged += (sender, args) => UpdateValues();
			UpdateValues();
			ItemsControlMechanics.ItemsSource = deck.Mechanics;
			TextBlockNoMechanics.Visibility = deck.Mechanics.Any() ? Collapsed : Visible;
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
					case Mana:
						statValue = card.Cost;
						break;
					case Health:
						statValue = card.Health;
						break;
					case Attack:
						statValue = card.Attack;
						break;
					case Overload:
						statValue = card.Overload;
						break;
				}
				if(statValue == -1)
					continue;
				if(statValue >= 7)
				{
					switch(card.Type)
					{
						case Weapon:
							weapons[7] += card.Count;
							break;
						case Enchantment:
						case Spell:
							spells[7] += card.Count;
							break;
						case Minion:
							minions[7] += card.Count;
							break;
					}
					counts[7] += card.Count;
				}
				else
				{
					if(Config.Instance.ManaCurveFilter == Mana || Config.Instance.ManaCurveFilter == Overload)
					{
						switch(card.Type)
						{
							case Weapon:
								weapons[statValue] += card.Count;
								break;
							case Enchantment:
							case Spell:
								spells[statValue] += card.Count;
								break;
							case Minion:
								minions[statValue] += card.Count;
								break;
						}
						counts[statValue] += card.Count;
					}
					else if(card.Type == Minion)
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

		private void ComboBoxStatType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selected = ComboBoxStatType.SelectedItem as StatTypeWrapper;
			if(selected != null)
			{
				if(Config.Instance.ManaCurveFilter != selected.StatType)
				{
					Config.Instance.ManaCurveFilter = selected.StatType;
					Config.Save();
				}
				UpdateValues();
			}
		}

		private void ManaCurveMechanics_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(BorderMechanics.Visibility != Visible)
			{
				BorderMechanics.Visibility = Visible;
				TextBlockManaCurveMechanics.Text = "HIDE";
			}
			else
			{
				BorderMechanics.Visibility = Collapsed;
				TextBlockManaCurveMechanics.Text = "MECHANICS";
			}
			TextBlockNoMechanics.Visibility = _deck != null && _deck.Mechanics.Any() ? Collapsed : Visible;
		}
	}

	public class StatTypeWrapper
	{
		public StatType StatType { get; set; }

		public string DisplayName => StatType.ToString().ToUpper();
	}
}