#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for ArenaRewards.xaml
	/// </summary>
	public partial class ArenaRewards
	{
		public static RoutedEvent SaveEvent = EventManager.RegisterRoutedEvent("Save", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
		                                                                       typeof(ArenaRewards));

		private readonly Dictionary<object, string> _invalidFields = new Dictionary<object, string>();

		private readonly CardSet[] _validSets =
			Enum.GetValues(typeof(ArenaRewardPacks))
				.Cast<CardSet>()
				.Skip(1)
				.ToArray();

		private List<string>? _cardNames;
		private bool _deletingSelection;

		public ArenaRewards()
		{
			InitializeComponent();

			var packs = Enum.GetValues(typeof(ArenaRewardPacks)).Cast<ArenaRewardPacks>().ToList();
			foreach(var pack in packs.GetRange(0, 1).Concat(packs.Skip(1).Reverse()))
			{
				ComboBoxPack1.Items.Add(pack);
				ComboBoxPack2.Items.Add(pack);
				ComboBoxPack3.Items.Add(pack);
			}
		}

		public ArenaReward Reward { get; set; } = new ArenaReward();

		private IEnumerable<string> CardNames => _cardNames ??=
													 Database.GetActualCards().Where(x => _validSets.Any(set => x.CardSet == set))
																.Select(x => x.LocalizedName)
																.WhereNotNull()
																.OrderBy(x => x.Length).ToList();

		private void AddInvalidField(object obj, string error)
		{
			if(_invalidFields.ContainsKey(obj))
				_invalidFields[obj] = error;
			else
				_invalidFields.Add(obj, error);
		}

		private void RemoveInvalidField(object obj)
		{
			if(_invalidFields.ContainsKey(obj))
				_invalidFields.Remove(obj);
		}

		private void CardNamePrediction(object sender, TextChangedEventArgs e)
		{
			var textBox = sender as TextBox;
			if(textBox == null)
				return;
			if(_deletingSelection)
			{
				RemoveInvalidField(textBox);
				textBox.BorderBrush = (Brush)FindResource("TextBoxBorderBrush");
				UpdateCardReward("", textBox);
				return;
			}
			if(string.IsNullOrEmpty(textBox.Text))
			{
				RemoveInvalidField(textBox);
				textBox.BorderBrush = (Brush)FindResource("TextBoxBorderBrush");
				UpdateCardReward("", textBox);
				return;
			}
			var prediction = CardNames.FirstOrDefault(x => x.StartsWith(textBox.Text, StringComparison.InvariantCultureIgnoreCase));
			if(prediction == null)
			{
				AddInvalidField(textBox, "Invalid card name: " + textBox.Text);
				textBox.BorderBrush = Brushes.Red;
				UpdateCardReward("", textBox);
				return;
			}
			RemoveInvalidField(textBox);
			textBox.BorderBrush = (Brush)FindResource("TextBoxBorderBrush");
			var selectionStart = textBox.Text.Length - 1;
			textBox.Text = prediction;
			textBox.Select(selectionStart + 1, textBox.Text.Length - selectionStart);
			UpdateCardReward(prediction, textBox);
		}

		private void UpdateCardReward(string cardName, TextBox textBox)
		{
			var card = Database.GetCardFromName(cardName, true, false);
			if(textBox == TextBoxCard1)
			{
				Reward.Cards[0] = card != null
					                  ? new ArenaReward.CardReward {CardId = card.Id, Golden = CheckBoxGolden1.IsChecked == true} : null;
			}
		}

		private void TextBoxCard_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Back)
				return;
			var textBox = sender as TextBox;
			if(string.IsNullOrEmpty(textBox?.Text))
				return;
			_deletingSelection = true;
			textBox!.SelectedText = "";
			_deletingSelection = false;
		}

		public bool Validate(out string error)
		{
			error = _invalidFields.Any() ? _invalidFields.First().Value : "";
			return !_invalidFields.Any();
		}

		private void UpdateCardRewardGolden(bool golden, CheckBox checkBox)
		{
			if(checkBox == CheckBoxGolden1 && Reward.Cards[0] != null)
				Reward.Cards[0]!.Golden = golden;
		}

		private void TextBoxGold_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var gold = 0;
			if(!string.IsNullOrEmpty(TextBoxGold.Text) && !int.TryParse(TextBoxGold.Text.Trim(), out gold))
			{
				if(TextBoxGold.Text.Contains("+"))
				{
					try
					{
						gold = TextBoxGold.Text.Split('+').Select(x => int.Parse(x.Trim())).Sum();
					}
					catch
					{
						AddInvalidField(TextBoxGold, "Invalid gold value: " + TextBoxGold.Text);
						TextBoxGold.BorderBrush = Brushes.Red;
						return;
					}
				}
				else
				{
					AddInvalidField(TextBoxGold, "Invalid gold value: " + TextBoxGold.Text);
					TextBoxGold.BorderBrush = Brushes.Red;
					return;
				}
			}
			RemoveInvalidField(TextBoxGold);
			TextBoxGold.BorderBrush = (Brush)FindResource("TextBoxBorderBrush");
			Reward.Gold = gold;
		}

		private void CheckBoxCrowdsFavor_OnChecked(object sender, RoutedEventArgs e)
		{
			Reward.CrowdsFavor = true;
			if(Reward.Gold < 1000)
			{
				Reward.Gold += 1000;
				TextBoxGold.Text = Reward.Gold.ToString();
			}
		}

		private void CheckBoxCrowdsFavor_OnUnchecked(object sender, RoutedEventArgs e)
		{
			Reward.CrowdsFavor = false;
			if(Reward.Gold >= 1000)
			{
				Reward.Gold -= 1000;
				TextBoxGold.Text = Reward.Gold.ToString();
			}
		}


		private void TextBoxTavernTickets_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var tickets = 0;
			if(!string.IsNullOrEmpty(TextBoxTavernTickets.Text) && !int.TryParse(TextBoxTavernTickets.Text.Trim(), out tickets))
			{
				if(TextBoxTavernTickets.Text.Contains("+"))
				{
					try
					{
						tickets = TextBoxTavernTickets.Text.Split('+').Select(x => int.Parse(x.Trim())).Sum();
					}
					catch
					{
						AddInvalidField(TextBoxTavernTickets, "Invalid tavern ticket value: " + TextBoxTavernTickets.Text);
						TextBoxTavernTickets.BorderBrush = Brushes.Red;
						return;
					}
				}
				else
				{
					AddInvalidField(TextBoxTavernTickets, "Invalid dust value: " + TextBoxTavernTickets.Text);
					TextBoxTavernTickets.BorderBrush = Brushes.Red;
					return;
				}
			}
			RemoveInvalidField(TextBoxTavernTickets);
			TextBoxTavernTickets.BorderBrush = (Brush)FindResource("TextBoxBorderBrush");
			Reward.TavernTickets = tickets;
		}

		private void ComboBoxPack1_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => Reward.Packs[0] = (ArenaRewardPacks)ComboBoxPack1.SelectedValue;

		private void ComboBoxPack2_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => Reward.Packs[1] = (ArenaRewardPacks)ComboBoxPack2.SelectedValue;

		private void ComboBoxPack3_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => Reward.Packs[2] = (ArenaRewardPacks)ComboBoxPack3.SelectedValue;

		private void CheckBoxGolden_OnChecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			if(checkBox == null)
				return;
			UpdateCardRewardGolden(true, checkBox);
		}

		private void CheckBoxGolden_OnUnchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			if(checkBox == null)
				return;
			UpdateCardRewardGolden(false, checkBox);
		}

		public void LoadArenaReward(ArenaReward reward)
		{
			TextBoxGold.Text = reward.Gold.ToString();
			CheckBoxCrowdsFavor.IsChecked = reward.CrowdsFavor;
			TextBoxTavernTickets.Text = reward.TavernTickets.ToString();
			ComboBoxPack1.SelectedItem = reward.Packs[0];
			ComboBoxPack2.SelectedItem = reward.Packs[1];
			ComboBoxPack3.SelectedItem = reward.Packs.Length >= 3 ? reward.Packs[2] : ArenaRewardPacks.None;
			if(!string.IsNullOrEmpty(reward.Cards[0]?.CardId))
				TextBoxCard1.Text = Database.GetCardFromId(reward.Cards[0]?.CardId!)?.LocalizedName;
			if(reward.Cards[0] != null)
				CheckBoxGolden1.IsChecked = reward.Cards[0]?.Golden ?? false;
		}

		public event RoutedEventHandler Save
		{
			add { AddHandler(SaveEvent, value); }
			remove { RemoveHandler(SaveEvent, value); }
		}

		private void ButtonSave_OnClick(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(SaveEvent, this));

		private void TextBox_OnGotKeyboardFocus(object sender, RoutedEventArgs routedEventArgs) => ((TextBox)sender).SelectAll();

		private void TextBox_OnGotMouseCapture(object sender, MouseEventArgs e) => ((TextBox)sender).SelectAll();
	}

	public class ArenaReward
	{
		private CardReward?[] _cards = new CardReward?[1];
		public int Gold { get; set; }
		public int TavernTickets { get; set; }
		public bool CrowdsFavor { get; set; }

		public CardReward?[] Cards
		{
			get
			{
				if(_cards.Length != 3 || _cards.Any(x => x?.CardId == null))
				{
					var valid = _cards.Where(x => x?.CardId != null).ToArray();
					_cards = new CardReward[1];
					for(var i = 0; i < valid.Length; i++)
						_cards[i] = valid[i];
				}
				return _cards;
			}
			set { _cards = value; }
		}

		public ArenaRewardPacks[] Packs { get; set; } = new ArenaRewardPacks[3];

		public class CardReward
		{
			public string? CardId { get; set; }
			public bool Golden { get; set; }
		}
	}
}
