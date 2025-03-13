using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Anomalies;

public class BattlegroundsAnomalyGuideListViewModel : ViewModel
{
	public Dictionary<int, BattlegroundsAnomalyGuide>? AnomalyGuides
	{
		get => GetProp<Dictionary<int, BattlegroundsAnomalyGuide>?>(null);
		private set => SetProp(value);
	}

	private const string Url = "https://hsreplay.net/api/v1/battlegrounds/anomaly_guides/";
	private async Task<AnomalyGuidesApiResponse?> MakeRequest()
	{
		using HttpRequestMessage req = new(HttpMethod.Get, Url + $"?game_language={Helper.GetCardLanguage()}");
		var resp = await Core.HttpClient.SendAsync(req);
		if(resp is { StatusCode: HttpStatusCode.OK })
			return JsonConvert.DeserializeObject<AnomalyGuidesApiResponse>(await resp.Content.ReadAsStringAsync());

		return null;
	}

	public BattlegroundsAnomalyGuide? GetAnomalyGuide(int anomalyDbfId)
	{
		if(AnomalyGuides == null)
			return null;

		AnomalyGuides.TryGetValue(anomalyDbfId, out var guide);
		return guide;
	}

	public async void Update()
	{
		if(AnomalyGuides != null)
			return;

		try
		{
			var data = await MakeRequest();
			if(data == null)
				return;

			AnomalyGuides = data.ToDictionary(h => h.Anomaly);
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	public void Reset()
	{
		AnomalyGuides = null;
	}
}
