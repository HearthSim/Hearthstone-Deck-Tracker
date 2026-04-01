﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HearthMirror.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan.V2;

public class ConstructedMulliganGuideV2ViewModel : ViewModel
{
	public Visibility StatsVisibility
	{
		get => GetProp(Collapsed);
		set
		{
			SetProp(value);
			UpdateMetrics();
		}
	}

	public ObservableCollection<ConstructedMulliganV2SingleCardViewModel>? CardStats
	{
		get => GetProp<ObservableCollection<ConstructedMulliganV2SingleCardViewModel>?>(null);
		set => SetProp(value);
	}

	public OverlayMessageViewModel Message { get; } = new();
	public MulliganState? MulliganState { get; set; }

	public string? Error
	{
		get => GetProp<string?>(null);
		set => SetProp(value);
	}

	public void Reset()
	{
		CardStats = null;
		StatsVisibility = Collapsed;
		MulliganState = null;
		Message.Clear();
	}

	public double Scaling { get => GetProp(1.0); set => SetProp(value); }

	public void SetMulliganData(MulliganV2Data? data)
	{
		CardStats = new ObservableCollection<ConstructedMulliganV2SingleCardViewModel>();

		if(data == null)
		{
			return;
		}

		var deckStatus = data.Data.GeneralInfo.DeckStatus;

		if(deckStatus is DeckStatus.SUPPORTED or DeckStatus.PARTIAL)
		{
			foreach (var cardByPosition in data.Data.CardsByPosition)
			{
				if(int.TryParse(cardByPosition.Key, out var position))
				{
					CardStats.Add(
						new ConstructedMulliganV2SingleCardViewModel(position, cardByPosition.Value, MulliganState)
					);
				}
			}

			Error = null;
			StatsVisibility = Visible;
			UpdateMetrics();
		}
		else
		{
			Error = LocUtil.Get("MulliganGV2_Error_NotAvailable");
		}
	}

	public void UpdateMulliganState(MulliganState state)
	{
		var prevWaitingForUserInput = MulliganState?.WaitingForUserInput;
		MulliganState = state;
		if(CardStats == null) return;

		if(prevWaitingForUserInput != null && prevWaitingForUserInput.Value && !state.WaitingForUserInput)
			return;

		if(state.MulliganCards.Any(c => c.State == ActorStateType.CARD_PLAYABLE_MOUSE_OVER))
			return;

		foreach(var card in CardStats)
		{
			card.CardHeaderVM.UpdateState(state);
		}
	}

	public void UpdateMulliganDataAfterMulligan(MulliganV2Data? data)
	{
		if(CardStats is null)
			return;

		if(data == null)
			return;

		foreach (var cardByPosition in data.Data.CardsByPosition)
		{
			if(int.TryParse(cardByPosition.Key, out var position) && position > 0 && position <= CardStats.Count)
			{
				CardStats[position - 1]?.CardHeaderVM.UpdateCard(cardByPosition.Value);
			}
		}
	}

	private void UpdateMetrics()
	{
		if(StatsVisibility == Visible)
			Core.Game.Metrics.ConstructedMulliganGuideOverlayDisplayed = true;
	}
}
