namespace HSReplay.Web
{
	internal class Header
	{
		public Header(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public string Value { get; }
	}
}