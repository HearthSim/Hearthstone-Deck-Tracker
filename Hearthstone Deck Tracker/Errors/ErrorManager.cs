#region

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Error
{
	public class ErrorManager
	{
		public static Visibility ErrorIconVisibility => Errors.Any() ? Visibility.Visible : Visibility.Collapsed;

		public static ObservableCollection<Error> Errors { get; } = new ObservableCollection<Error>();

		public static async void AddError(Error error)
		{
			if(Errors.Contains(error))
				return;
			Log.Info($"New error: {error.Header}\n{error.Text}");
			Errors.Add(error);
			while(!Core.Initialized)
				await Task.Delay(500);
			Core.MainWindow.ErrorsPropertyChanged();
		}

		public static void AddError(string header, string text) => AddError(new Error(header, text));

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