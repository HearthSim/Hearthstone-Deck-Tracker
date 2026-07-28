using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class PoolStatistics
{
	public string MedianCostText { get; }
	public string? MedianAttackText { get; }
	public string? MedianHealthText { get; }

	public IReadOnlyList<StatBar> CostBars { get; }
	public IReadOnlyList<StatBar>? AttackBars { get; }
	public IReadOnlyList<StatBar>? HealthBars { get; }

	public PoolStatistics(
		string medianCostText,
		string? medianAttackText,
		string? medianHealthText,
		IReadOnlyList<StatBar> costBars,
		IReadOnlyList<StatBar>? attackBars,
		IReadOnlyList<StatBar>? healthBars)
	{
		MedianCostText = medianCostText;
		MedianAttackText = medianAttackText;
		MedianHealthText = medianHealthText;
		CostBars = costBars;
		AttackBars = attackBars;
		HealthBars = healthBars;
	}
}
