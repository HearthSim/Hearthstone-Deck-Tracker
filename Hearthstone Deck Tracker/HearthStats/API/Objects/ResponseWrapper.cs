namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class ResponseWrapper<T>
	{
		public string status { get; set; }
		public T data { get; set; }
	}
}