using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Windows.Media;
using HearthDb;
using HearthMirror.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan.V2
{
	public class ConstructedMulliganV2SingleCardHeaderViewModel : ViewModel
	{
		public int Position { get; }

		public Hearthstone.Card? Card { get; set; }

		public MulliganState? MulliganState {
			get => GetProp<MulliganState?>(null);
			set
			{
				PreviousHandPosition = CurrentHandPosition + (float)(0.001f * new Random().NextDouble());
				SetProp(value);
				if(value?.WaitingForUserInput == true)
				{
					RecalculateConfidence();
				}
			}
		}

		public float? Confidence
		{
			get => GetProp<float?>(null);
			private set => SetProp(value);
		}

		public bool IsKeepingAll
		{
			get => GetProp(true);
			set {
				SetProp(value);
				OnPropertyChanged(nameof(TooltipTitle));
				OnPropertyChanged(nameof(TooltipText));
			}
		}

		public float? ContextualConfidence
		{
			get => GetProp<float?>(null);
			private set => SetProp(value);
		}

		public bool HasTooltip => Confidence.HasValue;

		public string? TooltipTitle
		{
			get {
				if(!Confidence.HasValue) return null;

				return IsKeepingAll
					? string.Format(LocUtil.Get("MulliganGV2_ContextualKeepRate_Title"), (int)Math.Round(Confidence.Value * 100))
					: string.Format(LocUtil.Get("MulliganGV2_DynamicKeepRate_Title"), (int)Math.Round(Confidence.Value * 100));
			}
		}

		public string? TooltipText
		{
			get
			{
				if(!Confidence.HasValue || !ContextualConfidence.HasValue) return null;

				var confidence = (int)Math.Round(Confidence.Value * 100);
				var contextualConfidence = (int)Math.Round(ContextualConfidence.Value * 100);

				var delta = confidence - contextualConfidence;

				return (IsKeepingAll, delta) switch
				{
					(true, _) => string.Format(
						LocUtil.Get("MulliganGV2_ContextualKeepRate_Description"),
						Card?.LocalizedName, confidence),
					(false, >= 0) => string.Format(
						LocUtil.Get("MulliganGV2_DynamicKeepRate_Positive_Description"),
						Card?.LocalizedName, delta, contextualConfidence),
					(false, < 0) => string.Format(
						LocUtil.Get("MulliganGV2_DynamicKeepRate_Negative_Description"),
						Card?.LocalizedName, -delta, contextualConfidence)
				};

			}

		}

		private void RecalculateConfidence()
		{
			Confidence = CalculateConfidence();

			OnPropertyChanged(nameof(Replaced));
			OnPropertyChanged(nameof(HasTooltip));
			OnPropertyChanged(nameof(CurrentHandPosition));
			OnPropertyChanged(nameof(HandPositionChangeWidth));
			OnPropertyChanged(nameof(HandPositionChangeLeft));

			OnPropertyChanged(nameof(LeftBand));
			OnPropertyChanged(nameof(RightBand));
			OnPropertyChanged(nameof(BandsWidth));

			OnPropertyChanged(nameof(TooltipTitle));
			OnPropertyChanged(nameof(TooltipText));
		}

		public List<MulliganJustification>? Justifications
		{
			get => GetProp<List<MulliganJustification>?>(null);
			set
			{
				SetProp(value);
				RecalculateConfidence();
			}
		}

		public ObservableCollection<MulliganTipViewModel>? MulliganTips
		{
			get => GetProp<ObservableCollection<MulliganTipViewModel>?>(null);
			set
			{
				SetProp(value);
			}
		}

		public bool Replaced => !Confidence.HasValue && !HasError;

		public OfferedCardStatus CardStatus
		{
			get => GetProp(OfferedCardStatus.VALID);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(HasError));
				OnPropertyChanged(nameof(Replaced));
				OnPropertyChanged(nameof(ErrorText));
			}
		}

		public bool HasError => CardStatus is OfferedCardStatus.UNKNOWN_CARD or OfferedCardStatus.NO_DATA;

		public string ErrorText
		{
			get
			{
				return CardStatus switch
				{
					OfferedCardStatus.NO_DATA => LocUtil.Get("MulliganGV2_Error_InsufficientData"),
					OfferedCardStatus.UNKNOWN_CARD => LocUtil.Get("MulliganGV2_Error_InsufficientData"),
					_ => ""
				};
			}
		}

		public bool HasWarning => CardStatus is OfferedCardStatus.LOW_DATA or OfferedCardStatus.VALID_WITH_INVALID_NEIGHBORS or OfferedCardStatus.LOW_DATA_WITH_INVALID_NEIGHBORS;

		public string WarningText
		{
			get
			{
				return CardStatus switch
				{
					OfferedCardStatus.LOW_DATA => LocUtil.Get("MulliganGV2_Warning_LowData"),
					OfferedCardStatus.VALID_WITH_INVALID_NEIGHBORS => LocUtil.Get("MulliganGV2_Warning_LowDataCombination"),
					OfferedCardStatus.LOW_DATA_WITH_INVALID_NEIGHBORS => LocUtil.Get("MulliganGV2_Warning_LowData"),
					_ => ""
				};
			}
		}

		public ConstructedMulliganV2SingleCardHeaderViewModel(
			int position,
			MulliganCard data,
			MulliganState? mulliganState
		)
		{
			Position = position;

			Card = new Hearthstone.Card(mulliganState?.MulliganCards.FirstOrDefault(c => c.ZonePosition == position)?.CardId ?? "");

			CardStatus = data.CardStatus;

			if(CardStatus is OfferedCardStatus.VALID or OfferedCardStatus.LOW_DATA or OfferedCardStatus.VALID_WITH_INVALID_NEIGHBORS or OfferedCardStatus.LOW_DATA_WITH_INVALID_NEIGHBORS)
			{
				Justifications = BuildJustifications(data.Justifications);

				MulliganState = mulliganState;

				MulliganTips = new ObservableCollection<MulliganTipViewModel>(data.Tips.Select(t => new MulliganTipViewModel(t, Card)));
			}
		}


		public void UpdateState(MulliganState state)
		{
			MulliganState = state;
			Card = new Hearthstone.Card(state?.MulliganCards.FirstOrDefault(c => c.ZonePosition == Position)?.CardId ?? "");
		}

		public void UpdateCard(MulliganCard data)
		{
			Justifications = BuildJustifications(data.Justifications);
			MulliganTips = new ObservableCollection<MulliganTipViewModel>(data.Tips.Select(t => new MulliganTipViewModel(t, Card)));
		}

		private float? CalculateConfidence()
		{
			{
				if(Justifications == null)
					return null;

				ContextualConfidence = (float?)Justifications.Last().Confidence;

				if(MulliganState == null)
					return (float?)Justifications.Last().Confidence;

				if(!MulliganState.WaitingForUserInput)
				{
					return (float?)Justifications.Last().Confidence;
				}

				IsKeepingAll = MulliganState.MulliganCards.All(c => c.State == ActorStateType.CARD_SELECTED);

				var keptCards = MulliganState.MulliganCards.Where(c => c.State == ActorStateType.CARD_SELECTED)
					.ToHashSet();

				var thisCard = keptCards.FirstOrDefault(c => c.ZonePosition == Position);
				if(thisCard == null)
					return null;

				keptCards.Remove(thisCard);
				var keptCardCounts = keptCards
					.GroupBy(c => c.CardId)
					.ToDictionary(g => g.Key, g => g.Count());

				return (float?) Justifications.FirstOrDefault(t =>
				{
					var justificationCounts = t.Cards
						.Select(ca => ca.Card?.Id)
						.Where(id => id != null)
						.GroupBy(id => id)
						.ToDictionary(g => g.Key, g => g.Count());

					return keptCardCounts.Count == justificationCounts.Count &&
					       keptCardCounts.All(k =>
						       justificationCounts.TryGetValue(k.Key, out var count) &&
						       count == k.Value);
				})?.Confidence;
			}
		}

		private static List<MulliganJustification> BuildJustifications(
			List<Tuple<int[], double>>? raw)
		{
			var source = raw ?? new();

			return source.Select(t =>
			{
				var cards = t.Item1
					.Select(id =>
						Cards.AllByDbfId.TryGetValue(id, out var card)
							? new CardAssetViewModel(
								new Hearthstone.Card(card),
								CardAssetType.Portrait)
							: null)
					.WhereNotNull();

				return new MulliganJustification(cards, t.Item2);
			}).ToList();
		}

		public Color NegativeColor
		{
			get
			{
				var converter = new ColorConverter();
				return (Color)((TypeConverter)converter).ConvertFromString(Helper.GetColorString(Helper.ColorStringMode.MULLIGAN_CONFIDENCE, -10, 75));
			}
		}

		public Color NeutralColor
		{
			get
			{
				var converter = new ColorConverter();
				return (Color)((TypeConverter)converter).ConvertFromString(Helper.GetColorString(Helper.ColorStringMode.MULLIGAN_CONFIDENCE, 0, 75));
			}
		}

		public Color PositiveColor
		{
			get
			{
				var converter = new ColorConverter();
				return (Color)((TypeConverter)converter).ConvertFromString(Helper.GetColorString(Helper.ColorStringMode.MULLIGAN_CONFIDENCE, 10, 75));
			}
		}

		public float? LeftBand
		{
			get
			{
				if(Justifications == null)
				{
					return null;
				}

				const float width = 212f;
				const float markerWidth = 10f;

				var lowestDelta = Justifications
					.Select(t => t.Confidence)
					.OrderBy(t => t)
					.FirstOrDefault();

				var pos = (float)lowestDelta * (width - markerWidth);
				return Math.Max(0, Math.Min(pos, width - markerWidth));

			}
		}

		public float? RightBand
		{
			get
			{
				if(Justifications == null)
				{
					return null;
				}

				const float width = 212f;
				const float markerWidth = 10f;

				var lowestDelta = Justifications
					.Select(t => t.Confidence)
					.OrderByDescending(t => t)
					.FirstOrDefault();

				var pos = (float)lowestDelta * (width - markerWidth);
				return Math.Max(0, Math.Min(pos, width - markerWidth));

			}
		}

		public float? BandsWidth {
			get
			{
				if(!LeftBand.HasValue || !RightBand.HasValue)
					return null;
				return RightBand.Value - LeftBand.Value;
			}
		}

		public float? CurrentHandPosition
		{
			get
			{
				if(!Confidence.HasValue)
				{
					return null;
				}

				const float width = 212f;
				const float markerWidth = 10f;
				var pos = Confidence.Value * (width - markerWidth);
				return Math.Max(0, Math.Min(pos, width - markerWidth));

			}
		}

		public float? HandPositionChangeWidth {
			get
			{
				if(!CurrentHandPosition.HasValue || !PreviousHandPosition.HasValue)
					return null;
				return Math.Abs(CurrentHandPosition.Value - PreviousHandPosition.Value);
			}
		}

		public float? PreviousHandPosition
		{
			get => GetProp<float?>(null);
			set => SetProp(value);
		}

		public float? HandPositionChangeLeft {
			get
			{
				if(!CurrentHandPosition.HasValue || !PreviousHandPosition.HasValue)
					return null;
				return Math.Min(CurrentHandPosition.Value, PreviousHandPosition.Value);
			}
		}
	}

	public sealed class MulliganJustification
	{
		public List<CardAssetViewModel> Cards { get; }
		public double Confidence { get; }

		public MulliganJustification(
			IEnumerable<CardAssetViewModel> cards,
			double confidence)
		{
			Cards = cards.ToList();
			Confidence = confidence;
		}
	}

	public class MulliganTipViewModel
	{
		public CardAssetViewModel? CardAsset { get; set; }

		public ObservableCollection<ArrowIndicator> ArrowIndicators { get; }
			= new ();

		public string? TooltipText { get; set; }

		public string? TooltipTitle { get; set; }

		public string? BaseKeepRate { get; set; }
		public string? AdjustedKeepRate { get; set; }

		public MulliganTipViewModel(MulliganTip tip, Hearthstone.Card? ownerCard)
		{

			var isUp = tip.Arrows > 0;

			for (int i = 0; i < Math.Abs(tip.Arrows); i++)
			{
				ArrowIndicators.Add(new ArrowIndicator
				{
					Color = isUp ? Brushes.LimeGreen : Brushes.Red,
					Rotation = isUp ? 0 : 180
				});
			}

			if (!Cards.AllByDbfId.TryGetValue(tip.DbfId, out var dbCard) || dbCard == null)
				return;


			var card =  new Hearthstone.Card(dbCard);

			CardAsset = new CardAssetViewModel(card, CardAssetType.Portrait);

			var baseKeepRate = Format(tip.BaseKeepRate);
			var adjustedKeepRate = Format(tip.AdjustedKeepRate);
			var delta = adjustedKeepRate - baseKeepRate;

			TooltipTitle = string.Format(LocUtil.Get("MulliganGV2_IconTooltip_Title"), delta > 0 ? "+" : "-", Math.Abs(delta));

			TooltipText = tip.TipType switch
			{
				TipType.KEPT_LESS => string.Format(LocUtil.Get("MulliganGV2_IconTooltip_KeptLess"),
					ownerCard?.LocalizedName, Math.Abs(delta), card.LocalizedName),
				TipType.KEPT_MORE => string.Format(LocUtil.Get("MulliganGV2_IconTooltip_KeptMore"),
					ownerCard?.LocalizedName, Math.Abs(delta), card.LocalizedName),
				TipType.KEPT_LESS_2ND_COPY => string.Format(LocUtil.Get("MulliganGV2_IconTooltip_KeptLess_SecondCopy"),
					ownerCard?.LocalizedName, Math.Abs(delta)),
				TipType.KEPT_MORE_2ND_COPY => string.Format(LocUtil.Get("MulliganGV2_IconTooltip_KeptMore_SecondCopy"),
					ownerCard?.LocalizedName, Math.Abs(delta)),
				_ => ""
			};

			BaseKeepRate = string.Format("Base Keep Rate: {0}%", baseKeepRate);
			AdjustedKeepRate = string.Format("Adjusted Keep Rate: {0}%", adjustedKeepRate);

		}

		private static int Format(double value)
		{
			return (int)Math.Round(value * 100);
		}
	}

	public class ArrowIndicator
	{
		public Brush? Color { get; set; }
		public double Rotation { get; set; }
	}
}
