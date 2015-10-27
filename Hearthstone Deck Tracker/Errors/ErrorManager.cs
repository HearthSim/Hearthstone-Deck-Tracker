#region

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Error
{
	public class ErrorManager
	{
		private static readonly ObservableCollection<Error> _errors = new ObservableCollection<Error>();

		public static Visibility ErrorIconVisibility
		{
			get { return Errors.Any() ? Visibility.Visible : Visibility.Collapsed; }
		}

		public static ObservableCollection<Error> Errors
		{
			get { return _errors; }
		}

		public static void AddError(Error error)
		{
			if(!Errors.Contains(error))
			{
				Logger.WriteLine(string.Format("New error: {0}\n{1}", error.Header, error.Text), "ErrorManager");
				Errors.Add(error);
				Core.MainWindow.ErrorsPropertyChanged();
			}
		}

		public static void AddError(string header, string text)
		{
			AddError(new Error(header, text));
		}

		public static void RemoveError(Error error)
		{
			if(Errors.Contains(error))
			{
				Errors.Remove(error);
				Core.MainWindow.ErrorsPropertyChanged();
				if(!Errors.Any())
					Core.MainWindow.FlyoutErrors.IsOpen = false;
			}
		}
	}
}