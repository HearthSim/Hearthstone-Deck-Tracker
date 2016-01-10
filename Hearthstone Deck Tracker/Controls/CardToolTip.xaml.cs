#region

using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Controls;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for CardToolTip.xaml
	/// </summary>
	public partial class CardToolTip
	{
		private static Type _toolTipType = typeof(CardToolTipControl);
		private static readonly List<CardToolTip> Instances = new List<CardToolTip>();

		public CardToolTip()
		{
			InitializeComponent();
			Instances.Add(this);
			SetContent();
		}

		public static Type ToolTipType
		{
			get { return _toolTipType; }
			set
			{
				_toolTipType = value;
				foreach(var i in Instances)
					i.SetContent();
			}
		}

		private void SetContent() => Content = Activator.CreateInstance(_toolTipType);
	}
}