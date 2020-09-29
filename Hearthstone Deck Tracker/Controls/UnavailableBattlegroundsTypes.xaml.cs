using Hearthstone_Deck_Tracker.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for UnavailableBattlegroundsTypes.xaml
	/// </summary>
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

		private string _unavailableRacesText;
		public string UnavailableRacesText
		{
			get => _unavailableRacesText;
			set
			{
				_unavailableRacesText = value;
				OnPropertyChanged();
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
