using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Anomalies
{
    public partial class AnomalyGuideTooltip
    {
        public AnomalyGuideTooltip()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
	        nameof(Cards), typeof(IEnumerable<Hearthstone.Card>), typeof(AnomalyGuideTooltip), new PropertyMetadata(new List<Hearthstone.Card>(), (d, _) => (d as AnomalyGuideTooltip)?.Update()));

        public IEnumerable<Hearthstone.Card>? Cards
        {
	        get => (IEnumerable<Hearthstone.Card>?)GetValue(CardsProperty);
	        set => SetValue(CardsProperty, value);
        }

        public static Race GetRace(int raceNumber)
        {
	        if (Enum.IsDefined(typeof(Race), raceNumber))
	        {
		        return (Race)raceNumber;
	        }
	        return Race.INVALID;
        }

        private void Update()
        {
            var card = Cards?.FirstOrDefault();
            if(card == null)
                return;

            if(card.TypeEnum != CardType.BATTLEGROUND_ANOMALY)
				return;

            ViewModel.HoveredAnomalyDbfid = card.DbfId;

            var availableRaces = BattlegroundsUtils.GetAvailableRaces()?.ToList();
            var anomalyGuide = Core.Overlay.BattlegroundsAnomalyGuideListViewModel.GetAnomalyGuide(card.DbfId);

            ViewModel.FavorableTribes =  anomalyGuide?.FavorableTribes?
	            .Select(GetRace)
	            .Where(race => availableRaces?.Contains(race) == true);

            if (anomalyGuide != null && !string.IsNullOrEmpty(anomalyGuide.PublishedGuide))
                ViewModel.PublishedGuide = ReferencedCardRun.ParseCardsFromText(anomalyGuide.PublishedGuide).FirstOrDefault();
            else
            {
                ViewModel.PublishedGuide = null;
                ViewModel.FavorableTribes = null;
            }
        }

        public BattlegroundsAnomalyGuideTooltipViewModel ViewModel { get; } = new();
    }

    public class BattlegroundsAnomalyGuideTooltipViewModel : ViewModel
    {
	    public int? HoveredAnomalyDbfid
	    {
		    get => GetProp<int?>(null);
		    set
		    {
			    SetProp(value);
			    OnPropertyChanged(nameof(AnomalyGuideVisibility));
		    }
	    }

	    public IEnumerable<Inline>? PublishedGuide
	    {
		    get => GetProp<IEnumerable<Inline>?>(null);
		    set
		    {
			    SetProp(value);
			    OnPropertyChanged(nameof(IsGuidePublished));
		    }
	    }

	    public IEnumerable<Race>? FavorableTribes
	    {
		    get => GetProp<IEnumerable<Race>?>(null);
		    set
		    {
			    SetProp(value);
			    OnPropertyChanged(nameof(FavorableTribesVisibility));
		    }
	    }

        public Visibility AnomalyGuideVisibility
        {
            get
            {
                if(HoveredAnomalyDbfid == null ||
                   Config.Instance.ShowBattlegroundsBrowser == false ||
                   Config.Instance.ShowBattlegroundsGuides == false)
                    return Collapsed;

                return Visible;
            }
        }
        public bool IsGuidePublished => PublishedGuide != null && PublishedGuide.Any();
        public Visibility FavorableTribesVisibility => FavorableTribes != null && FavorableTribes.Any() ? Visible : Collapsed;
    }
}
