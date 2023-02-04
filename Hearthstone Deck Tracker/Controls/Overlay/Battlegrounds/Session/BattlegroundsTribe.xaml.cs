﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsTribe : INotifyPropertyChanged
	{
		public BattlegroundsTribe()
		{
			InitializeComponent();
		}

		private Dictionary<Race, string> TribeImages = new Dictionary<Race, string>() {
			{Race.PET , "pet"},
			{Race.MECHANICAL , "mech"},
			{Race.MURLOC , "murloc"},
			{Race.DEMON , "demon"},
			{Race.DRAGON , "dragon"},
			{Race.PIRATE , "pirate"},
			{Race.ELEMENTAL , "elemental"},
			{Race.QUILBOAR , "quilboar"},
			{Race.NAGA , "naga"},
			{Race.UNDEAD , "undead"},
		};

		public string ImageSrc => $"/HearthstoneDeckTracker;component/Resources/TribeIcons/{TribeImages[Tribe]}.jpg";

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TribeProperty = DependencyProperty.Register(
			"Tribe",
			typeof(Race),
			typeof(BattlegroundsTribe),
			new FrameworkPropertyMetadata(Race.PET, (d, _) => ((BattlegroundsTribe)d).OnTribeChanged())
		);

		public Race Tribe
		{
			get {
				var tribe = (Race)GetValue(TribeProperty);
				if (TribeImages.ContainsKey(tribe))
					return tribe;
				return Race.PET;
			}
			set
			{
				SetValue(TribeProperty, value);
				OnTribeChanged();
			}
		}

		private string _tribeName = HearthDbConverter.RaceConverter(Race.BEAST) ?? "";
		public string TribeName
		{
			get => _tribeName;
			set
			{
				_tribeName = value;
				OnPropertyChanged();
			}
		}

		private string _color = "#D44040";
		public string Color =
		{
			get => _color;
			set
			{
				_color = value;
				OnPropertyChanged;
			}
		}

		private bool _available = false;
		public bool Available
		{
			get => _available;
			set
			{
				_available = value;
				if (_available)
				{
					_color = "#00A806";
				}
				else
				{
					_color = "#D44040";
				}
				OnPropertyChanged();
			}
		}

		private void OnTribeChanged()
		{
			TribeName = HearthDbConverter.RaceConverter(Tribe) ?? "";
			OnPropertyChanged(nameof(ImageSrc));
		}
	}
}
