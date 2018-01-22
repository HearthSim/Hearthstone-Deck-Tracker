namespace HearthWatcher.EventArgs
{
	public class DialogVisibilityEventArgs : System.EventArgs
	{
		public bool DialogVisible { get; set; }

		public DialogVisibilityEventArgs(bool dialogVisible)
		{
			DialogVisible = dialogVisible;
		}
	}
}
