namespace Hearthstone_Deck_Tracker.Controls.Error
{
	public class Error
	{
		public Error(string header, string text)
		{
			Header = header;
			Text = text;
		}

		public string Text { get; }
		public string Header { get; }

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
				return false;
			if(ReferenceEquals(this, obj))
				return true;
			if(obj.GetType() != GetType())
				return false;
			return Equals((Error)obj);
		}

		protected bool Equals(Error other) => string.Equals(Text, other.Text) && string.Equals(Header, other.Header);

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Text?.GetHashCode() ?? 0) * 397) ^ (Header?.GetHashCode() ?? 0);
			}
		}
	}
}
