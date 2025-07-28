using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Trinkets;

public class BattlegroundsTrinketGuideListViewModel : ViewModel
{
	public Dictionary<int, BattlegroundsTrinketGuide>? TrinketGuides
	{
		get => GetProp<Dictionary<int, BattlegroundsTrinketGuide>?>(null);
		private set => SetProp(value);
	}

	private const string Url = "https://hsreplay.net/api/v1/battlegrounds/trinket_guides/";
	private async Task<TrinketGuidesApiResponse?> MakeRequest()
	{
		using HttpRequestMessage req = new(HttpMethod.Get, Url + $"?game_language={Helper.GetCardLanguage()}");
		req.Headers.UserAgent.ParseAdd(Helper.GetUserAgent());
		var resp = await Core.HttpClient.SendAsync(req);
		if(resp is { StatusCode: HttpStatusCode.OK })
			return JsonConvert.DeserializeObject<TrinketGuidesApiResponse>(await resp.Content.ReadAsStringAsync());

		return null;
	}

	public BattlegroundsTrinketGuide? GetTrinketGuide(int trinketDbfId)
	{
		if(TrinketGuides == null)
			return null;

		TrinketGuides.TryGetValue(trinketDbfId, out var guide);
		return guide;
	}

	public async void Update()
	{
		if(TrinketGuides != null)
			return;

		try
		{
			var data = await MakeRequest();
			if(data == null)
				return;

			TrinketGuides = data.ToDictionary(h => h.Trinket);
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	public void Reset()
	{
		TrinketGuides = null;
	}
}
