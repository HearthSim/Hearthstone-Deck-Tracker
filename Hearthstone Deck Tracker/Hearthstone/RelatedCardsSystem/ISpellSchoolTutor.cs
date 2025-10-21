namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public interface ISpellSchoolTutor : ICardWithHighlight
{
	int[] TutoredSpellSchools { get; }
}
