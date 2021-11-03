using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class UnavailableBattlegroundsTypes : UserControl, INotifyPropertyChanged
	{
		public UnavailableBattlegroundsTypes()
		{
			InitializeComponent();
		}

		private Visibility _unavailableTypesVisibility = Visibility.Collapsed;
		public Visibility UnavailableTypesVisibility
		{
			get => _unavailableTypesVisibility;
			set
			{
				_unavailableTypesVisibility = value;
				OnPropertyChanged();
			}
		}

		private string _unavailableRacesText = string.Empty;
		public string UnavailableRacesText
		{
			get => _unavailableRacesText;
			set
			{
				_unavailableRacesText = value;
				OnPropertyChanged();
			}
		}


		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
