namespace Hearthstone_Deck_Tracker.Controls.Error
{
	public class Error
	{
		public Error(string header, string text)
		{
			Header = header;
			Text = text;
		}

		public string Text { get; set; }
		public string Header { get; set; }

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

		protected bool Equals(Error other)
		{
			return string.Equals(Text, other.Text) && string.Equals(Header, other.Header);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Text != null ? Text.GetHashCode() : 0) * 397) ^ (Header != null ? Header.GetHashCode() : 0);
			}
		}
	}
}