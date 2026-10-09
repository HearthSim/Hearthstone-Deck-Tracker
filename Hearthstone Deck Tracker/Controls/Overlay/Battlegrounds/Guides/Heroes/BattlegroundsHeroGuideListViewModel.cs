using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;
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
		UpdateHeroes();

		if(HeroGuides != null)
			return;

		try
		{
			var data = await MakeRequest();
			if(data == null)
				return;

			HeroGuides = data.ToDictionary(h => h.Hero);
			UpdateHeroes();
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	public void Reset()
	{
		HeroGuides = null;
		Heroes = null;
		_heroPowerDbfIds = new List<int>();
		HasQuests = false;
	}

	public List<BattlegroundsHeroGuideViewModel>? Heroes
	{
		get => GetProp<List<BattlegroundsHeroGuideViewModel>?>(null);
		private set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsHeroSelected));
		}
	}

	public bool IsHeroSelected => Heroes is { Count: > 0 };

	private List<int> _heroPowerDbfIds = new();

	// these guides are only about choosing a hero power, so the chosen hero's guide replaces them
	private static readonly HashSet<string> ReplaceableHeroes = new()
	{
		HearthDb.CardIds.NonCollectible.Neutral.SirFinleyMrrggltonTavernBrawl1,
		HearthDb.CardIds.NonCollectible.Neutral.GennWorgenKing,
	};

	public void OnMulliganEnded() => UpdateHeroes();

	public void OnHeroPowers(IEnumerable<Entity> heroPowers)
	{
		_heroPowerDbfIds = heroPowers
			.OrderBy(x => x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX))
			.Select(x => x.Card.DbfId)
			.ToList();
		UpdateHeroes();
	}

	private void UpdateHeroes()
	{
		var pickedHero = GetPickedHero();
		if(pickedHero == null)
			return;

		// some heroes' own hero power points at another hero (e.g. Genn's points at Sir Finley)
		var ownHeroPower = pickedHero.GetTag(GameTag.HERO_POWER);
		var heroDbfIds = new[] { pickedHero.DbfId }
			.Concat(_heroPowerDbfIds.Select(heroPower => heroPower == ownHeroPower
				? pickedHero.DbfId
				: Database.GetCardFromDbfId(heroPower, false)?.GetTag(GameTag.BACON_HEROPOWER_BASE_HERO_ID) ?? 0
			))
			.Where(x => x > 0)
			.Distinct()
			.ToList();

		if(heroDbfIds.Count > 1 && ReplaceableHeroes.Contains(pickedHero.Id))
			heroDbfIds.RemoveAt(0);

		if(!heroDbfIds.SequenceEqual(Heroes?.Select(x => x.HeroCard?.DbfId ?? 0) ?? Enumerable.Empty<int>()))
		{
			Heroes = heroDbfIds
				.Select(dbfId => Database.GetCardFromDbfId(dbfId, false))
				.WhereNotNull()
				.Select(card => new BattlegroundsHeroGuideViewModel { HeroCard = card })
				.ToList();
		}

		foreach(var hero in Heroes!)
		{
			BattlegroundsHeroGuide? guide = null;
			HeroGuides?.TryGetValue(hero.HeroCard!.DbfId, out guide);
			hero.HeroGuide = guide;
		}
	}

	private static Hearthstone.Card? GetPickedHero()
	{
		var heroDbfid = Core.Game.BattlegroundsHeroPickState.PickedHeroDbfId;
		if(heroDbfid == null)
			return null;

		var heroCard = Database.GetCardFromDbfId(heroDbfid.Value, false);

		// The hero ID can be a skin, so we need to get the base hero id.
		var baseHeroDbfid = heroCard?.BattlegroundsSkinParentId;
		if(baseHeroDbfid is > 0)
			heroCard = Database.GetCardFromDbfId(baseHeroDbfid.Value, false);

		return heroCard;
	}

	public void OnQuestSelected(bool hasQuests)
	{
		HasQuests = hasQuests;
	}
}
