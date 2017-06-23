#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Themes;
using CardIds = HearthDb.CardIds;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	/// <summary>
	/// Interaction logic for CardThemesInfo.xaml
	/// </summary>
	public partial class CardThemesInfo : INotifyPropertyChanged
	{
		private readonly List<Hearthstone.Card> _cards = new List<Hearthstone.Card>();
		private bool _update = true;

		private readonly string[] _demoCards =
		{
			CardIds.Collectible.Neutral.GilblinStalker,
			CardIds.Collectible.Priest.NorthshireCleric,
			CardIds.Collectible.Neutral.GilblinStalker,
			CardIds.Collectible.Priest.UpgradedRepairBot,
			CardIds.Collectible.Neutral.GarrisonCommander,
			CardIds.Collectible.Neutral.YouthfulBrewmaster,
			CardIds.Collectible.Priest.UpgradedRepairBot,
			CardIds.Collectible.Priest.NorthshireCleric
		};

		public CardThemesInfo()
		{
			InitializeComponent();
			UpdateAnimatedCardListAsync();
		}

		public Hearthstone.Card Card => Database.GetCardFromId(CardIds.Collectible.Neutral.RagnarosTheFirelord);
		public DrawingBrush ClassicCard => GetCardImage("classic");
		public DrawingBrush MinimalCard => GetCardImage("minimal");
		public DrawingBrush DarkCard => GetCardImage("dark");
		public DrawingBrush FrostCard => GetCardImage("frost");

		public DrawingBrush GetCardImage(string themeName)
		{
			var theme = ThemeManager.Themes.FirstOrDefault(x => x.Name == themeName);
			if(theme == null)
				return new DrawingBrush();
			var buildType = theme.BuildType ?? typeof(DefaultBarImageBuilder);
			return ((CardBarImageBuilder)Activator.CreateInstance(buildType, Card, theme.Directory)).Build();
		}

		public bool RarityGems
		{
			get { return Config.Instance.RarityCardGems; }
			set
			{
				Config.Instance.RarityCardGems = value;
				UpdateCards();
				OnPropertyChanged(nameof(ClassicCard));
				OnPropertyChanged(nameof(MinimalCard));
				OnPropertyChanged(nameof(DarkCard));
				OnPropertyChanged(nameof(FrostCard));
			}
		}

		public bool RarityFrames
		{
			get { return Config.Instance.RarityCardFrames; }
			set
			{
				Config.Instance.RarityCardFrames = value;
				UpdateCards();
				OnPropertyChanged(nameof(ClassicCard));
				OnPropertyChanged(nameof(MinimalCard));
				OnPropertyChanged(nameof(DarkCard));
				OnPropertyChanged(nameof(FrostCard));
			}
		}

		private async void UpdateAnimatedCardListAsync()
		{
			foreach(var cardId in _demoCards)
			{
				var card = _cards.FirstOrDefault(x => x.Id == cardId);
				if(card == null)
					_cards.Add(Database.GetCardFromId(cardId));
				else
					card.Count++;
			}
			AnimatedCardList.Update(_cards.ToSortedCardList().Select(x => (Hearthstone.Card)x.Clone()).ToList(), true);
			while(_update)
			{
				foreach(var cardId in _demoCards)
				{
					if(!_update)
						break;
					await Task.Delay(2000);
					var card = _cards.FirstOrDefault(x => x.Id == cardId);
					if(card != null)
					{
						if(card.Count == 1)
							_cards.Remove(card);
						else
							card.Count--;
					}
					AnimatedCardList.Update(_cards.ToSortedCardList().Select(x => (Hearthstone.Card)x.Clone()).ToList(), false);
				}
				foreach(var cardId in _demoCards)
				{
					if(!_update)
						break;
					await Task.Delay(2000);
					var card = _cards.FirstOrDefault(x => x.Id == cardId);
					if(card == null)
						_cards.Add(Database.GetCardFromId(cardId));
					else
						card.Count++;
					AnimatedCardList.Update(_cards.ToSortedCardList().Select(x => (Hearthstone.Card)x.Clone()).ToList(), false);
				}
			}
		}

		private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
			if(AnimatedCardList == null)
				return;
			var tb = sender as ToggleButton;
			if(tb != null)
			{
				var theme = tb.Content.ToString().ToLower();
				ThemeManager.SetTheme(theme);
				Config.Instance.CardBarTheme = theme;
			}
			UpdateCards();
		}

		private void UpdateCards()
		{
			foreach(var card in AnimatedCardList.Items.Cast<AnimatedCard>().Select(x => x.Card))
			{
				card.UpdateHighlight();
				card.Update();
			}
		}

		private void CardThemesInfo_OnUnloaded(object sender, RoutedEventArgs e)
		{
			_update = false;
			Config.Save();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
