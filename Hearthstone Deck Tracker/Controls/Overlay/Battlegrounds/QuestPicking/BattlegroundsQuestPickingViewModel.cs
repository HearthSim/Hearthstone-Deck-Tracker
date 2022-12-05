using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Requests;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking
{
	using Quests = IEnumerable<BattlegroundsSingleQuestViewModel>;

	public partial class BattlegroundsQuestPickingViewModel : ViewModel
	{
		private readonly List<Entity> _entities = new();

		public Quests? Quests { get => GetProp<Quests?>(null); set { SetProp(value); } }

		public Visibility Visibility
		{
			get
			{
				if(!Config.Instance.ShowBattlegroundsQuestPicking)
					return Collapsed;
				return GetProp(Collapsed);
			}
			set => SetProp(value);
		}

		public double Scaling { get => GetProp(1.0); set => SetProp(value); }

		public OverlayMessageViewModel Message { get; } = new();


		private int? ExpectedQuestCount() => Core.Game.GetTurnNumber() switch
		{
			1 => 2,
			4 => 3,
			_ => null
		};

		public async Task OnBattlegroundsQuest(Entity questEntity)
		{
			_entities.Add(questEntity);
			if(_entities.Count == ExpectedQuestCount())
				await Update();
		}

		public void Reset()
		{
			_entities.Clear();
			Quests = null;
			Visibility = Collapsed;
			_watchChoices = false;
			Message.Clear();
		}

		private async Task Update()
		{
			// Trials not supported for now.
			if(Core.Game.Spectator)
				return;

			if(Quests != null)
				return;

			var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;

			// The trial would have been activated at hero picking. If it is
			// not active we do not try to activate it here.
			if(!userOwnsTier7 && !Tier7Trial.IsActive)
				return;

			if(_entities.Count != ExpectedQuestCount())
				return;

			Message.Loading();
			// delay to allow tag changes to update
			await Task.Delay(500);

			var requestParams = GetApiParams();
			if(requestParams == null)
			{
				Message.Error();
				return;
			}

			var questData = await HSReplayNetOAuth.MakeRequest(c => c.GetTier7QuestStats(requestParams));;
			if(questData == null)
			{
				Message.Error();
				return;
			}

			var choices = Reflection.GetCardChoices();
			if(choices == null)
			{
				Message.Error();
				return;
			}

			var orderedEntities = choices.Cards.Select(id => _entities.Find(x => x.CardId == id));
			Quests = orderedEntities.Select(quest => {
				var reward = quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID);
				var data = questData.FirstOrDefault(x => x.RewardDbfId == reward);
				return new BattlegroundsSingleQuestViewModel(data);
			});
			Message.Mmr(questData[0].MmrFilterValue, questData[0].MinMMR);
			if(choices.IsVisible)
				Visibility = Visible;

			WatchChoices();

			Core.Game.Metrics.Tier7QuestOverlayDisplayed = true;
		}

		private bool _watchChoices;
		private async void WatchChoices()
		{
			_watchChoices = true;
			while(_watchChoices)
			{
				await Task.Delay(200);
				var choices = Reflection.GetCardChoices();
				if(choices == null)
				{
					_watchChoices = false;
					Visibility = Collapsed;
					break;
				}
				Visibility = choices.IsVisible ? Visible : Collapsed;
			}
		}

		private BattlegroundsQuestStatsParams? GetApiParams()
		{
			var hero = Core.Game.Entities.Values.FirstOrDefault(x => x.IsPlayer && x.IsHero);
			var heroCardId = hero?.CardId != null ? BattlegroundsUtils.GetOriginalHeroId(hero.CardId) : null;
			var heroCard = heroCardId != null ? Database.GetCardFromId(heroCardId) : null;
			if(heroCard == null)
				return null;

			var availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId);
			if(availableRaces == null)
				return null;

			return new BattlegroundsQuestStatsParams
			{
				HeroDbfId = heroCard.DbfId,
				HeroPowerDbfIds = Core.Game.Player.PastHeroPowers.Select(x => Database.GetCardFromId(x)?.DbfId).Where(x => x.HasValue).Cast<int>().ToArray(),
				Turn = Core.Game.GetTurnNumber(),
				MinionTypes = availableRaces.Cast<int>().ToArray(),
				OfferedRewards = GetOffererdRewards().ToArray(),
				LanguageCode = Config.Instance.SelectedLanguage
			};
		}

		private IEnumerable<BattlegroundsQuestStatsParams.OfferedReward> GetOffererdRewards()
		{ 
			foreach(var quest in _entities)
			{
				if(!quest.HasCardId)
					continue;
				var rewardCardDbfId = quest.GetTag(GameTag.BACON_CARD_DBID_REWARD);
				yield return new BattlegroundsQuestStatsParams.OfferedReward
				{
					RewardDbfId = quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID),
					RewardCardDbfId = rewardCardDbfId != 0 ? rewardCardDbfId : null,
				};
			}
		}
	}
}
