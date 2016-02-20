#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedStats : INotifyPropertyChanged
	{
		public static ConstructedStats Instance { get; } = new ConstructedStats();
		public IEnumerable<GameStats> FilteredGames => GetFilteredGames();

		public event PropertyChangedEventHandler PropertyChanged;


		public IEnumerable<GameStats> GetFilteredGames(bool archivedFilter = true, bool classFilter = true, bool regionFilter = true,
													   bool timeframeFilter = true)
		{
			IEnumerable<Deck> decks = DeckList.Instance.Decks;
			if(archivedFilter && !Config.Instance.ArenaStatsIncludeArchived)
				decks = decks.Where(x => !x.Archived);

			var filtered = decks.SelectMany(x => x.DeckStats.Games);

			//if(includenodeck)
			filtered = filtered.Concat(DefaultDeckStats.Instance.DeckStats.SelectMany(x => x.Games));


			if(classFilter && Config.Instance.ArenaStatsClassFilter != HeroClassStatsFilter.All)
				filtered = filtered.Where(x => x.PlayerHero == Config.Instance.ArenaStatsClassFilter.ToString());
			if(regionFilter && Config.Instance.ArenaStatsRegionFilter != RegionAll.ALL)
			{
				var region = (Region)Enum.Parse(typeof(Region), Config.Instance.ArenaStatsRegionFilter.ToString());
				filtered = filtered.Where(x => x.Region == region);
			}
			if(timeframeFilter)
			{
				switch(Config.Instance.ArenaStatsTimeFrameFilter)
				{
					case DisplayedTimeFrame.AllTime:
						break;
					case DisplayedTimeFrame.CurrentSeason:
						filtered = filtered.Where(g => g.StartTime > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
						break;
					case DisplayedTimeFrame.ThisWeek:
						filtered = filtered.Where(g => g.StartTime > DateTime.Today.AddDays(-((int)g.StartTime.DayOfWeek + 1)));
						break;
					case DisplayedTimeFrame.Today:
						filtered = filtered.Where(g => g.StartTime > DateTime.Today);
						break;
					case DisplayedTimeFrame.Custom:
						var start = (Config.Instance.ArenaStatsTimeFrameCustomStart ?? DateTime.MinValue).Date;
						var end = (Config.Instance.ArenaStatsTimeFrameCustomEnd ?? DateTime.MaxValue).Date;
						filtered = filtered.Where(g => g.EndTime.Date >= start && g.EndTime.Date <= end);
						break;
				}
			}
			return filtered;
		}

		public void UpdateConstructedStats()
		{
			OnPropertyChanged(nameof(FilteredGames));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}