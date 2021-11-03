using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Assets;
using System;
using System.Windows;
using System.Windows.Media;
using static Hearthstone_Deck_Tracker.Windows.OverlayWindow;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries
{
	public class MercenariesAbilityViewModel : CardAssetViewModel
	{
		private readonly Entity? _entity;
		private readonly Hearthstone.Card? _card;
		private readonly bool _active;
		private readonly int _gameTurn;
		private readonly SolidColorBrush _white = new SolidColorBrush(Colors.White);
		private readonly SolidColorBrush _red = new SolidColorBrush(Colors.Red);
		private readonly SolidColorBrush _green = new SolidColorBrush(Color.FromRgb(0, 255, 0));

		// Using FullImage here since they will be loaded anyways on hover
		public MercenariesAbilityViewModel(MercAbilityData data) : base(data.Entity?.Card ?? data.Card, CardAssetType.FullImage)
		{
			_card = data.Card;
			_entity = data.Entity;
			_active = data.Active;
			_gameTurn = data.GameTurn;
		}

		private int TurnsElapsed => Math.Max(0, _gameTurn - 1);

		public int Cooldown => (_entity?.GetTag(GameTag.LETTUCE_CURRENT_COOLDOWN)) ?? Math.Max(0, (_card?.LettuceCooldown ?? 0) - TurnsElapsed);
		public int Speed => _entity?.GetTag(GameTag.COST) ?? _card?.Cost ?? 0;
		public int BaseSpeed => _entity?.Card?.Cost ?? _card?.Cost ?? 0;

		public double Opacity => Cooldown > 0 ? 0.5 : 1;

		public ImageSource? CheckmarkAsset => _active ? Find("MercsCheckmark") : null;
		public ImageSource? CooldownAsset => Cooldown > 0 ? Find("MercsAbilityCooldown") : null;
		public Visibility CooldownShadingVisibility => Cooldown > 0 ? Visibility.Visible: Visibility.Collapsed;
		public string? CooldownText => Cooldown > 0 ? Cooldown.ToString() : null;
		public string SpeedText => Speed.ToString();
		public Visibility SpeedUncertainIndicatorVisibility => _entity == null ? Visibility.Visible : Visibility.Collapsed;

		public SolidColorBrush SpeedColorBrush => Speed > BaseSpeed ? _red : Speed < BaseSpeed ? _green : _white;

		public Visibility ActiveIndicatorVisibility => _active ? Visibility.Visible : Visibility.Collapsed;

		private ImageSource Find(string assetName) => (ImageSource)Application.Current.TryFindResource(assetName);
	}
}
