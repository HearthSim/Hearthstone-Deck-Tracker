using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HearthDb;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Exceptions;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Inspiration;

public class BattlegroundsInspirationViewModel : ViewModel
{
	public List<BattlegroundsInspirationGameViewModel>? Games
	{
		get => GetProp<List<BattlegroundsInspirationGameViewModel>?>(null)?.Skip((Page - 1) * 4).Take(4).ToList();
		private set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HasNoGames));
		}
	}

	public int Page
	{
		get => GetProp(1);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Games));
			OnPropertyChanged(nameof(PageButtons));
		}
	}

	public record PageButton(int Page, bool IsActive);

	private List<int> Pages
	{
		get => GetProp<List<int>?>(null) ?? new List<int>();
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(PageButtons));
		}
	}

	public List<PageButton> PageButtons => Pages.Select(page => new PageButton(page, page == Page)).ToList();

	public ICommand SetPageCommand => new Command<int>(page => Page = page);

	public string TitleText
	{
		get => GetProp("") ?? "";
		set => SetProp(value);
	}

	public int MmrPercentile
	{
		get => GetProp(5);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(MMRText));
		}
	}

	[LocalizedProp]
	public string MMRText => string.Format(LocUtil.Get("BattlegroundsInspiration_Description_MMR"), MmrPercentile);

	public bool IsLoadingData
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HasNoGames));
		}
	}

	public event Action? OnClose;
	public ICommand CloseCommand => new Command(() => OnClose?.Invoke());

	public bool HasBeenActivated => _lastRequestKeyDbfIds.Count > 0;
	public bool HasNoGames => (Games == null || Games.Count == 0) && !IsLoadingData;

	private List<int> _lastRequestKeyDbfIds = new ();
	private List<int> _lastRequestBoardDbfIds = new ();

	public void SetKeyMinion(params Hearthstone.Card?[] keyCards) => SetKeyMinion(keyCards.WhereNotNull().FirstOrDefault()?.Name ?? "", keyCards);

	public async void SetKeyMinion(string title, params Hearthstone.Card?[] keyCards)
	{
		if(keyCards.Length == 0)
		{
			_lastRequestKeyDbfIds.Clear();
			return;
		}

		// Always use the normal dbf ids for now. The user may have incidentally clicked a tripled minion.
		// If we want to support triples in the future we should make this very explicit.
		var keyDbfIds = keyCards.WhereNotNull().Select(card => Cards.TripleToNormalDbfIds.TryGetValue(card.DbfId, out var normalDbfId) ? normalDbfId : card.DbfId).ToList();

		var boardDbfIds = _isInShopping ? Core.Game.Player.Board.Where(x => x.IsMinion).Select(x => x.Card.DbfId).OrderBy(x => x).ToList() : _endOfShoppingBoardState;
		if(keyDbfIds.SequenceEqual(_lastRequestKeyDbfIds) && boardDbfIds.SequenceEqual(_lastRequestBoardDbfIds))
			return;
		_lastRequestKeyDbfIds = keyDbfIds;
		_lastRequestBoardDbfIds = boardDbfIds;

		TitleText = title;
		Games = null;
		IsLoadingData = true;
		MmrPercentile = Core.Game.IsBattlegroundsDuosMatch ? 10 : 5;
		Page = 1;
		Pages = new List<int>();
		try
		{
			var data = await MakeRequest(keyDbfIds, boardDbfIds);
			var games  = data?.Data.Games.Take(20).Select(x => new BattlegroundsInspirationGameViewModel(x)).ToList();
			Games = games;
			var pageCount = (int)Math.Ceiling((games?.Count ?? 0) / 4.0);
			if(pageCount > 1)
				Pages = Enumerable.Range(1, pageCount).ToList();
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
		finally
		{
			IsLoadingData = false;
		}
	}

	private const string Url = "https://hsreplay.net/api/v1/battlegrounds/inspiration/";
	private async Task<InspirationApiResponse?> MakeRequest(List<int> minionDbfIds, List<int> boardDbfIds)
	{
		var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;
		if(!userOwnsTier7 && (Tier7Trial.RemainingTrials ?? 0) == 0)
			return null;


		var races = BattlegroundsUtils.GetAvailableRaces() ?? new HashSet<Race>();
		if(races.Count != 5)
			throw new HeroPickingException($"Invalid number of races: {string.Join(", ", races)}");

		if(boardDbfIds.Count > 7)
			throw new HeroPickingException($"Invalid number of BoardDbfIds: {boardDbfIds.Count()}");

		// Assemble payload before interacting with trial system
		var reqData = new InspirationApiRequestData(races, minionDbfIds)
		{
			GameType = (int)HearthDbConverter.GetBnetGameType(Core.Game.CurrentGameType, Core.Game.CurrentFormat),
			LineupDbfIds = boardDbfIds,
		};

		string? token = null;
		if(!userOwnsTier7)
		{
			var acc = Reflection.Client.GetAccountId();
			if(acc == null)
				throw new HeroPickingException("Unable to get trial token"); // TODO
			token = await Tier7Trial.ActivateOrContinue(acc.Hi, acc.Lo, Core.Game.MetaData.ServerInfo?.GameHandle);
			if(token == null)
				throw new HeroPickingException("Unable to get trial token"); // TODO
		}

		using HttpRequestMessage req = new(HttpMethod.Post, Url);
		req.Headers.Add("X-Trial-Token", token);
		var data = JsonConvert.SerializeObject(reqData);
		req.Content = new StringContent(data, Encoding.UTF8, "application/json");
		Log.Info(data);
		var resp = await HSReplayNetOAuth.SendAsyncWithAuth(req);
		if(resp is { StatusCode: HttpStatusCode.OK })
			return JsonConvert.DeserializeObject<InspirationApiResponse>(await resp.Content.ReadAsStringAsync());

		return null;
	}

	private bool _isInShopping = true;
	private List<int> _endOfShoppingBoardState = new ();

	public void OnShoppingStart()
	{
		_isInShopping = true;
	}

	public void OnShoppingEnd()
	{
		_isInShopping = false;
		_endOfShoppingBoardState = Core.Game.Player.Board.Where(x => x.IsMinion).Select(x => x.Card.DbfId).ToList();
	}

	public void Reset()
	{
		Games = null;
		Page = 1;
		TitleText = "";
		_isInShopping = true;
		_lastRequestKeyDbfIds = new List<int>();
		_endOfShoppingBoardState = new List<int>();
		_lastRequestBoardDbfIds = new List<int>();
	}
}
