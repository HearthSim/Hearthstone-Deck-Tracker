using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides
{
    public class GuideTooltipContainer : ContentControl
    {
        public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
            nameof(Cards), typeof(IEnumerable<Hearthstone.Card>), typeof(GuideTooltipContainer),
            new PropertyMetadata(null, OnCardsChanged));

        public IEnumerable<Hearthstone.Card>? Cards
        {
            get => (IEnumerable<Hearthstone.Card>)GetValue(CardsProperty);
            set => SetValue(CardsProperty, value);
        }

        private static void OnCardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GuideTooltipContainer container)
                container.UpdateContent();
        }

        private void UpdateContent()
        {
	        // The Cards property accepts an IEnumerable<Hearthstone.Card> instead of a single Hearthstone.Card
	        // because the underlying ViewModel is shared and expects a collection of cards. This allows the
	        // tooltip to handle multiple cards if necessary, even though this is not the case for the guide related
	        // tooltips.

            var card = Cards?.FirstOrDefault();
            if (card == null)
                return;
            if (card.TypeEnum == CardType.BATTLEGROUND_TRINKET)
            {
	            var trinketGuide = new Trinkets.TrinketGuideTooltip
	            {
		            Cards = Cards
	            };
	            Content = trinketGuide;
            }
            else if (card.TypeEnum == CardType.BATTLEGROUND_QUEST_REWARD)
            {
	            var questGuide = new Quests.QuestGuideTooltip()
	            {
		            Cards = Cards
	            };
	            Content = questGuide;
            }
	        else if (card.TypeEnum == CardType.BATTLEGROUND_ANOMALY)
            {
                var anomalyGuide = new Anomalies.AnomalyGuideTooltip
                {
                    Cards = Cards
                };
                Content = anomalyGuide;
            }
            else
            {
                var heroGuide = new Heroes.HeroGuideTooltip
                {
                    Cards = Cards
                };
                Content = heroGuide;
            }
        }
    }
}
