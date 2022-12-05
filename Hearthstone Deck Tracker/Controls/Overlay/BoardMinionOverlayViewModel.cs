using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Collections.Generic;
using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public enum AbilityAlignment
	{
		Top,
		Bottom
	}
	public class BoardMinionOverlayViewModel : ViewModel
	{
		public BoardMinionOverlayViewModel(AbilityAlignment abilityAlignment = AbilityAlignment.Top)
		{
			_abilityAlignment = abilityAlignment;
		}

		private Thickness _margin;
		public Thickness Margin
		{
			get => _margin;
			set { _margin = value; OnPropertyChanged(); }
		}

		private double _width;

		public double Width
		{
			get => _width;
			set { _width = value; OnPropertyChanged(); }
		}

		private double _height;

		public double Height
		{
			get => _height;
			set
			{
				_height = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(AbilitySize));
				OnPropertyChanged(nameof(AbilityPanelWidth));
				OnPropertyChanged(nameof(AbilityPanelTopMargin));
			}
		}

		public double AbilitySize => Height * 0.28;
		public double AbilityPanelWidth => AbilitySize * (MercenariesAbilities?.Count ?? 0);
		public double AbilityPanelTopMargin => _abilityAlignment == AbilityAlignment.Top ? -AbilitySize - Height * 0.12 : Height * 0.14;
		public VerticalAlignment VerticalAlignment => _abilityAlignment == AbilityAlignment.Top ? VerticalAlignment.Top : VerticalAlignment.Bottom;

		private Visibility _visibility = Visibility.Collapsed;
		public Visibility Visibility
		{
			get => _visibility;
			set { _visibility = value; OnPropertyChanged(); }
		}

		private List<MercenariesAbilityViewModel>? _mercenariesAbilities;

		private readonly AbilityAlignment _abilityAlignment;

		public List<MercenariesAbilityViewModel>? MercenariesAbilities
		{
			get => _mercenariesAbilities;
			set
			{
				_mercenariesAbilities = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(AbilityPanelWidth));
			}
		}

		private Visibility _abilitiesVisibility;
		public Visibility AbilitiesVisibility
		{
			get => _abilitiesVisibility;
			set
			{
				if(value == _abilitiesVisibility)
					return;
				_abilitiesVisibility = value;
				OnPropertyChanged();
			}
		}

	}
}
