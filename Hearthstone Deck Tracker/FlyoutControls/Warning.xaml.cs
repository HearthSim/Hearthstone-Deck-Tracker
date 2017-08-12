using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class Warning : INotifyPropertyChanged
	{
		private string _warningText;
		private ConfigWarning _warning;

		public event Action OnComplete;

		public Warning()
		{
			InitializeComponent();
		}

		public string WarningText
		{
			get => _warningText;
			set
			{
				if(value == _warningText)
					return;
				_warningText = value;
				OnPropertyChanged();
			}
		}

		private void ButtonEnable_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.Reset(_warning.ToString());
			Config.Save();
			OnComplete?.Invoke();

		}

		private void ButtonIgnore_OnClick(object sender, RoutedEventArgs e)
		{
			var list = Config.Instance.IgnoredConfigWarnings.ToList();
			list.Add(_warning);
			Config.Instance.IgnoredConfigWarnings = list.ToArray();
			Config.Save();
			OnComplete?.Invoke();
		}

		public void SetConfigWarning(ConfigWarning warning)
		{
			_warning = warning;
			WarningText = EnumDescriptionConverter.GetDescription(warning);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
