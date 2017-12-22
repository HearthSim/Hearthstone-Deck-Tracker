#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class WotogCounter : INotifyPropertyChanged
	{
		private bool _forceShow;
		private string _attack = "6";
		private string _health = "6";
		private string _spells = "0";
		private string _jade = "1";

		private WotogCounterStyle _wotogCounterStyle;
		private WotogCounterStyle _jadeCounterStyle;


		public WotogCounter()
		{
			InitializeComponent();
		}

		public string Attack
		{
			get { return _attack; }
			set
			{
				if(value == _attack)
					return;
				_attack = value;
				OnPropertyChanged();
			}
		}

		public string Health
		{
			get { return _health; }
			set
			{
				if(value == _health)
					return;
				_health = value;
				OnPropertyChanged();
			}
		}

		public string Spells
		{
			get { return _spells; }
			set
			{
				if(value == _spells)
					return;
				_spells = value;
				OnPropertyChanged();
			}
		}

		public string Jade
		{
			get { return _jade; }
			set {
				if (value == _jade)
					return;
				_jade = value;
				OnPropertyChanged();
			}
		}

		public WotogCounterStyle WotogCounterStyle
		{
			get { return _wotogCounterStyle; }
			set
			{
				if(value == _wotogCounterStyle)
					return;
				_wotogCounterStyle = value;
				OnPropertyChanged(nameof(CthunVisibility));
				OnPropertyChanged(nameof(SpellsVisibility));
				OnPropertyChanged(nameof(FullVisibility));
				OnPropertyChanged(nameof(IconWidth));
			}
		}

		public WotogCounterStyle JadeCounterStyle
		{
			get { return _jadeCounterStyle; }
			set {
				if (value == _jadeCounterStyle)
					return;
				_jadeCounterStyle = value;
				OnPropertyChanged(nameof(JadeVisibility));
			}
		}

		public int IconWidth => WotogCounterStyle == WotogCounterStyle.Full ? 226 : 145;
		public Visibility CthunVisibility => _forceShow || WotogCounterStyle == WotogCounterStyle.Cthun ? Visibility.Visible : Visibility.Collapsed;
		public Visibility SpellsVisibility => !_forceShow && WotogCounterStyle == WotogCounterStyle.Spells ? Visibility.Visible : Visibility.Collapsed;
		public Visibility FullVisibility => !_forceShow && WotogCounterStyle == WotogCounterStyle.Full ? Visibility.Visible : Visibility.Collapsed;
		public Visibility JadeVisibility => !_forceShow && JadeCounterStyle == WotogCounterStyle.Full ? Visibility.Visible : Visibility.Collapsed;

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void ForceShow(bool force)
		{
			_forceShow = force;
			OnPropertyChanged(nameof(CthunVisibility));
			OnPropertyChanged(nameof(SpellsVisibility));
			OnPropertyChanged(nameof(FullVisibility));
			OnPropertyChanged(nameof(JadeVisibility));
		}
	}

	public enum WotogCounterStyle
	{
		None,
		Full,
		Cthun,
		Spells
	}
}
