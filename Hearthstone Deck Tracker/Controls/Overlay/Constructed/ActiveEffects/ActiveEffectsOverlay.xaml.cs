using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.ActiveEffects;

public partial class ActiveEffectsOverlay : INotifyPropertyChanged
{
    public class ActiveEffect : ICardTooltip
    {
        public EntityBasedEffect Effect { get; }
        public int? Count { get; set; }
        public bool IsPlayer { get; set; }

        public ActiveEffect(EntityBasedEffect effect, bool isPlayer, int? count)
        {
            Effect = effect;
            Count = count;
            IsPlayer = isPlayer;
        }

        public string ShadowColor => Effect.IsControlledByPlayer ? "#c4bcd1" : "#e39d91";
        public string BorderColor => Effect.IsControlledByPlayer ? "#8c7ca3" : "#b83424";
        public string BorderDarkerColor => Effect.IsControlledByPlayer ? "#29293d" : "#671e14";

        public void UpdateTooltip(CardTooltipViewModel viewModel)
        {
	        viewModel.Card = Effect.CardToShowInUI;
        }
    }

    private readonly Hearthstone.EffectSystem.ActiveEffects _activeEffects;
    public ObservableCollection<ActiveEffect> VisibleEffects { get; } = new();
    public bool IsPlayer { get; set; }

    public ActiveEffectsOverlay()
    {
        _activeEffects = Core.Game.ActiveEffects;
        _activeEffects.EffectsChanged += ActiveEffects_EffectsChanged;
        InitializeComponent();
        UpdateVisibleEffects();
    }

    private void ActiveEffects_EffectsChanged(object sender, EventArgs e)
    {
        UpdateVisibleEffects();
	}

    public new int MaxHeight => (EffectSize + InnerMargin * 2) * 2 + OuterMargin * 2;
    public new int MaxWidth => (EffectSize + InnerMargin * 2) * MaxColumns + OuterMargin * 2;

    public static int OuterMargin => 20;
    public static int InnerMargin => 6;
    public static int EffectSize => 49;

    private static int MaxColumns => 4;

    public int ColumnCount => Math.Min(MaxColumns, VisibleEffects.Count);

    private void UpdateVisibleEffects()
    {
        VisibleEffects.Clear();
        var effectsByCardId = _activeEffects.GetVisibleEffects(IsPlayer).GroupBy(x => x.CardId);

        foreach (var effects in effectsByCardId)
        {
            var effect = effects.First();
            var effectCount = effects.Count();
            var effectWithCount = new ActiveEffect(effect, IsPlayer, null);

            if (effect.ShowNumberInPlay && effectCount > 1)
            {
                effectWithCount.Count = effectCount;
            }

            VisibleEffects.Add(effectWithCount);
        }
        OnPropertyChanged(nameof(VisibleEffects));
        OnPropertyChanged(nameof(ColumnCount));
    }

    public void ForceShowExampleEffects(bool isPlayer)
	{
		VisibleEffects.Clear();
		var preparation = new PreparationEnchantment(0, isPlayer);
		for(int i = 0; i < 8; i++)
		{
			VisibleEffects.Add(new ActiveEffect(preparation, isPlayer, null));
		}
		OnPropertyChanged(nameof(VisibleEffects));
		OnPropertyChanged(nameof(ColumnCount));
	}

	public void ForceHideExampleEffects()
	{
		UpdateVisibleEffects();
	}

	public void Reset()
	{
		_activeEffects.Reset();
		UpdateVisibleEffects();
	}

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
