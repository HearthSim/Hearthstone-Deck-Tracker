using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.PlayerResourcesWidget;

public class PlayerResourcesViewModel : INotifyPropertyChanged
{
	public record Resource(string Icon, int Value);

	private int _initialMaxHealth;
	private int _initialMaxMana;
	private int _initialMaxHandSize;

	private bool _healthChanged;
	private bool _manaChanged;
	private bool _handSizeChanged;

	private List<Resource> _changedResources = new();
	public List<Resource> ChangedResources
	{
		get => _changedResources;
		private set
		{
			_changedResources = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(HasVisibleResources));
		}
	}

	public bool HasVisibleResources => ChangedResources.Count > 0;

	public void Initialize(int maxHealth, int maxMana, int maxHandSize)
	{
		_initialMaxHealth = maxHealth;
		_initialMaxMana = maxMana;
		_initialMaxHandSize = maxHandSize;

		_healthChanged = false;
		_manaChanged = false;
		_handSizeChanged = false;
	}

	public void UpdatePlayerResourcesWidget(int maxHealth, int maxMana, int maxHandSize)
	{
		_healthChanged |= maxHealth != _initialMaxHealth;
		_manaChanged |= maxMana != _initialMaxMana;
		_handSizeChanged |= maxHandSize != _initialMaxHandSize;

		var updated = new List<Resource>();

		if (_healthChanged)
			updated.Add(new Resource("/Images/health.png", maxHealth));

		if (_manaChanged)
			updated.Add(new Resource("/Images/mana.png", maxMana));

		if (_handSizeChanged)
			updated.Add(new Resource("/Images/card-icon-drawn.png", maxHandSize));

		ChangedResources = updated;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
