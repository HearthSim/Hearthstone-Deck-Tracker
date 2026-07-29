using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.BoardOrder
{
	public class BoardOrderSlotViewModel : ViewModel
	{
		public bool IsOccupied
		{
			get => GetProp(false);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(SlotVisibility));
			}
		}

		public Visibility SlotVisibility => IsOccupied ? Visibility.Visible : Visibility.Collapsed;

		public string? Label
		{
			get => GetProp<string?>(null);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(LabelVisibility));
			}
		}

		public Visibility LabelVisibility
			=> string.IsNullOrEmpty(Label) ? Visibility.Collapsed : Visibility.Visible;

		public double Width
		{
			get => GetProp(0.0);
			set => SetProp(value);
		}

		public double Height
		{
			get => GetProp(0.0);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(BadgeSize));
				OnPropertyChanged(nameof(FontSize));
				OnPropertyChanged(nameof(BadgeMargin));
			}
		}

		public Thickness Margin
		{
			get => GetProp(new Thickness());
			set => SetProp(value);
		}

		public const double BadgeSizeFactor = 0.14;

		public const double BadgeOverhangFactor = 0.6;

		public double BadgeSize => Height * BadgeSizeFactor;

		public double FontSize => Math.Max(1.0, BadgeSize * 0.6);

		public Thickness BadgeMargin => new(0, -BadgeSize * BadgeOverhangFactor, 0, 0);
	}
}
