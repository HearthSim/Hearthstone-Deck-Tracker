using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Enums;
using HearthMirror.Objects;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class ArenaStateWatcher
{
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private readonly IArenaStateProvider _provider;
	private ArenaState.DraftChoice? _choice;
	private double _scroll;
	private List<Card>? _deckList;
	private List<ArenaState.DraftChoice>? _choices;
	private int? _choicesVersion;
	private int? _deckListVersion;
	private List<Card>? _redraftDeckList;
	private int? _redraftDeckListVersion;
	private string _hero = "";
	private long _deckId;
	private ArenaState.ActorInfo? _zoomedHero;
	private bool _isAnimating;
	private bool _isPackageSelectOpen;
	private bool _isUnderground;
	private int _arenaSeasonId;
	private ArenaState.BigCard? _trayBigCard;
	private (ArenaClientStateType, ArenaSessionState) _clientState = (ArenaClientStateType.None, ArenaSessionState.INVALID);
	private (List<float>, int)? _tooltip;

	private ArenaState.ScryCache? _cache;

	public ArenaStateWatcher(IArenaStateProvider provider, int delay = 15)
	{
		_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		_delay = delay;
	}

	public event Action<ArenaState.ActorInfo?>? OnHeroZoomed;
	public event Action<bool>? OnIsPackageSelectOpen;
	public event Action<ArenaState.DraftChoice?>? OnCardHover;
	public event Action<double>? OnScrollChange;
	public event Action<List<Card>>? OnDeckListChange;
	public event Action<List<Card>>? OnRedraftDeckListChange;
	public event Action<List<ArenaState.DraftChoice>>? OnChoicesChanged;
	public event Action<(ArenaClientStateType ClientState, ArenaSessionState SessionState)>? OnClientStateChanged;
	public event Action<bool>? OnIsAnimatingChanged;
	public event Action<string>? OnHeroPicked;
	public event Action<ArenaState.BigCard?>? OnTrayBigCardChanged;
	public event Action<(List<float>, int)?>? OnTooltipChanged;
	public event Action<bool>? OnIsUndergroundChanged;
	public event Action<int>? OnArenaSeasonIdChanged;
	public event Action<long>? OnDeckIdChanged;

	public void Run()
	{
		_watch = true;
		if(!_running)
			Watch();
	}

	public void Stop()
	{
		_watch = false;
		_cache = null;
	}

	private async void Watch()
	{
		_running = true;
		_choice = null;
		_scroll = 0;
		_deckList = null;
		_choices = null;
		_choicesVersion = null;
		_deckListVersion = null;
		_redraftDeckList = null;
		_redraftDeckListVersion = null;
		_hero = "";
		_zoomedHero = null;
		_isAnimating = false;
		_isPackageSelectOpen = false;
		_trayBigCard = null;
		_tooltip = null;
		_clientState = (ArenaClientStateType.None, ArenaSessionState.INVALID);
		_cache = null;

		while(_watch)
		{
			await Task.Delay(_delay);
			if(!_watch)
				break;
			Update();
		}
		_running = false;
	}

	public void Update()
	{
		var state = _provider.GetState(_deckListVersion, _redraftDeckListVersion, _cache);
		if(state == null)
		{
			_cache = null;
			return;
		}
		_cache = state.Cache;

		if(_clientState.Item1 != state.ClientState || _clientState.Item2 != state.SessionState)
		{
			_cache = null;
			_clientState = (state.ClientState, state.SessionState);
			OnClientStateChanged?.Invoke(_clientState);
		}

		if(_isAnimating != state.IsAnimating)
		{
			_isAnimating = state.IsAnimating;
			OnIsAnimatingChanged?.Invoke(_isAnimating);
		}

		if(_isUnderground != state.IsUnderground)
		{
			_cache = null;
			_isUnderground = state.IsUnderground;
			OnIsUndergroundChanged?.Invoke(_isUnderground);
		}

		if(_arenaSeasonId != state.ArenaSeasonId)
		{
			_arenaSeasonId = state.ArenaSeasonId;
			OnArenaSeasonIdChanged?.Invoke(_arenaSeasonId);
		}

		if(_isPackageSelectOpen != state.IsPackageSelectOpen)
		{
			_isPackageSelectOpen = state.IsPackageSelectOpen;
			OnIsPackageSelectOpen?.Invoke(_isPackageSelectOpen);
		}

		if(_zoomedHero?.CardId != state.ZoomedHero?.CardId)
		{
			_zoomedHero = state.ZoomedHero;
			OnHeroZoomed?.Invoke(_zoomedHero);
		}

		if(_hero != state.ChosenHero)
		{
			_hero = state.ChosenHero;
			OnHeroPicked?.Invoke(_hero);
		}

		if(_deckId != state.DeckId)
		{
			_deckId = state.DeckId;
			OnDeckIdChanged?.Invoke(_deckId);
		}

		if(!Equals(state.DeckListBigCard?.PositionY, _trayBigCard?.PositionY))
		{
			_trayBigCard = state.DeckListBigCard;
			OnTrayBigCardChanged?.Invoke(_trayBigCard);
		}

		if(Math.Abs(state.DeckListScroll - _scroll) > 0.001)
		{
			_scroll = state.DeckListScroll;
			OnScrollChange?.Invoke(_scroll);
		}

		// DeckListData will be null if the list version matches _deckListVersion.
		if(state.DeckListData != null)
		{
			_deckListVersion = state.DeckListData.Version;
			_deckList = state.DeckListData.CardIds;
			OnDeckListChange?.Invoke(_deckList);
		}

		if(state.RedraftDeckListData != null)
		{
			_redraftDeckListVersion = state.RedraftDeckListData.Version;
			_redraftDeckList = state.RedraftDeckListData.CardIds;
			OnRedraftDeckListChange?.Invoke(_redraftDeckList);
		}

		if(_choices?.Count != state.Choices.Count || _choicesVersion != state.ChoicesVersion)
		{
			// Choices are available in memory while e.g. in the landing screen.
			var isDrafting = state.ClientState is ArenaClientStateType.Normal_Draft
				or ArenaClientStateType.Underground_Draft or ArenaClientStateType.Normal_Redraft
				or ArenaClientStateType.Underground_Redraft;

			var isValidChoiceState = state.Choices.Count is 0 or 3;

			if(isDrafting && isValidChoiceState)
			{
				_choices = state.Choices;
				_choicesVersion = state.ChoicesVersion;
				OnChoicesChanged?.Invoke(_choices);
			}
		}

		if(_choice?.Actor.CardId != state.HoveredChoice?.Actor.CardId || _choice?.Actor.Index != state.HoveredChoice?.Actor.Index)
		{
			_choice = state.HoveredChoice;
			OnCardHover?.Invoke(_choice);
		}

		if(_tooltip?.Item2 != _choice?.Actor.Index || !Equals(_tooltip?.Item1.Sum(), state.TooltipHeights?.Sum()))
		{
			_tooltip = _choice?.Actor.Index == null || state.TooltipHeights == null ? null : (state.TooltipHeights, _choice.Actor.Index);
			OnTooltipChanged?.Invoke(_tooltip);
		}
	}
}
