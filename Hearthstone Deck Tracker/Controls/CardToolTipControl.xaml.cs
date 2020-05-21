using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class CardToolTipControl : INotifyPropertyChanged
	{
		private Visibility _createdByVisibility = Visibility.Collapsed;

		public CardToolTipControl()
		{
			InitializeComponent();
		}

		public Visibility CreatedByVisibility
		{
			get => _createdByVisibility;
			set
			{
				if(_createdByVisibility == value)
					return;
				_createdByVisibility = value;
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
