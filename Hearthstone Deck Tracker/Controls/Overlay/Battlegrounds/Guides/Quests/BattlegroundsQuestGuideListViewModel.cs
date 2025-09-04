using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Quests;

public class BattlegroundsQuestGuideListViewModel : ViewModel
{
	public Dictionary<int, BattlegroundsQuestGuide>? QuestGuides
	{
		get => GetProp<Dictionary<int, BattlegroundsQuestGuide>?>(null);
		private set => SetProp(value);
	}

	public ObservableCollection<BattlegroundsQuestGuideViewModel> SelectedQuests { get; } = new();

	public bool HasQuests => SelectedQuests.Count > 0;

	private const string Url = "https://hsreplay.net/api/v1/battlegrounds/quest_guides/";
	private async Task<QuestGuidesApiResponse?> MakeRequest()
	{
		using HttpRequestMessage req = new(HttpMethod.Get, Url + $"?game_language={Helper.GetCardLanguage()}");
		req.Headers.UserAgent.ParseAdd(Helper.GetUserAgent());
		var resp = await Core.HttpClient.SendAsync(req);
		if(resp is { StatusCode: HttpStatusCode.OK })
			return JsonConvert.DeserializeObject<QuestGuidesApiResponse>(await resp.Content.ReadAsStringAsync());

		return null;
	}

	public BattlegroundsQuestGuide? GetQuestGuide(int questRewardDbfId)
	{
		if(QuestGuides == null)
			return null;

		QuestGuides.TryGetValue(questRewardDbfId, out var guide);
		return guide;
	}

	public async void Update()
	{
		if(QuestGuides != null)
			return;

		try
		{
			var data = await MakeRequest();
			if(data == null)
				return;

			QuestGuides = data.ToDictionary(h => h.Quest);
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	public void Reset()
	{
		QuestGuides = null;
		SelectedQuests.Clear();
		OnPropertyChanged(nameof(HasQuests));
	}

	public void OnQuestSelected(Hearthstone.Card questRewardCard)
	{
		BattlegroundsQuestGuide? guide = null;

		QuestGuides?.TryGetValue(questRewardCard.DbfId, out guide);

		var questViewModel = new BattlegroundsQuestGuideViewModel
		{
			QuestCard = questRewardCard,
			QuestGuide = guide
		};

		SelectedQuests.Add(questViewModel);
		OnPropertyChanged(nameof(HasQuests));
	}
}
