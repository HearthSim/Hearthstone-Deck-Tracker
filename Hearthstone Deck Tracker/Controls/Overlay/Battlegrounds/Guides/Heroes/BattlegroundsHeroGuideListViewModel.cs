using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Heroes;

public class BattlegroundsHeroGuideListViewModel : ViewModel
{
	public Dictionary<int, BattlegroundsHeroGuide>? HeroGuides
	{
		get => GetProp<Dictionary<int, BattlegroundsHeroGuide>?>(null);
		private set => SetProp(value);
	}

	public bool HasQuests
	{
		get => GetProp(false);
		private set => SetProp(value);
	}

	private const string Url = "https://hsreplay.net/api/v1/battlegrounds/hero_guides/";
	private async Task<HeroGuidesApiResponse?> MakeRequest()
	{
		using HttpRequestMessage req = new(HttpMethod.Get, Url + $"?game_language={Helper.GetCardLanguage()}");
		req.Headers.UserAgent.ParseAdd(Helper.GetUserAgent());
		var resp = await Core.HttpClient.SendAsync(req);
		if(resp is { StatusCode: HttpStatusCode.OK })
			return JsonConvert.DeserializeObject<HeroGuidesApiResponse>(await resp.Content.ReadAsStringAsync());

		return null;
	}

	public BattlegroundsHeroGuide? GetHeroGuide(int heroDbfId)
	{
		if(HeroGuides == null)
			return null;

		HeroGuides.TryGetValue(heroDbfId, out var guide);
		return guide;
	}

	public async void Update()
	{
		if(Core.Game.BattlegroundsHeroPickState.PickedHeroDbfId != null)
			OnMulliganEnded();

		if(HeroGuides != null)
			return;

		try
		{
			var data = await MakeRequest();
			if(data == null)
				return;

			HeroGuides = data.ToDictionary(h => h.Hero);
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	public void Reset()
	{
		HeroGuides = null;
		SelectedHero.HeroCard = null;
		SelectedHero.HeroGuide = null;
		HasQuests = false;
	}

	public BattlegroundsHeroGuideViewModel SelectedHero { get; } = new ();

	public void OnMulliganEnded()
	{
		var heroDbfid = Core.Game.BattlegroundsHeroPickState.PickedHeroDbfId;

		if(heroDbfid == null)
			return;

		var heroCard = Database.GetCardFromDbfId(heroDbfid.Value, false);

		// The hero ID can be a skin, so we need to get the base hero id.
		var baseHeroDbfid = heroCard?.BattlegroundsSkinParentId;

		if(baseHeroDbfid is > 0)
			heroCard = Database.GetCardFromDbfId(baseHeroDbfid.Value, false);

		BattlegroundsHeroGuide? guide = null;
		if(heroCard == null)
			return;

		HeroGuides?.TryGetValue(heroCard.DbfId, out guide);

		SelectedHero.HeroCard = heroCard;
		SelectedHero.HeroGuide = guide;
	}

	public void OnQuestSelected(bool hasQuests)
	{
		HasQuests = hasQuests;
	}
}
