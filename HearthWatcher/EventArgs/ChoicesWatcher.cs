using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class ChoicesWatcher : System.EventArgs
	{
		public CardChoices? CurrentChoice { get; }

		public ChoicesWatcher(CardChoices? currentChoice)
		{
			CurrentChoice = currentChoice;
		}

		protected bool Equals(ChoicesWatcher other) => Equals(CurrentChoice, other.CurrentChoice);

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ChoicesWatcher)obj);
		}

		public override int GetHashCode() => CurrentChoice?.GetHashCode() ?? 0;
	}
}
