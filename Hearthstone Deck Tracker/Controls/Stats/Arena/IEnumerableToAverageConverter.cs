using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	public class IEnumerableToAverageConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			IEnumerable<ArenaRun> runs = value as IEnumerable<ArenaRun>;
			string property = parameter as string;
			switch (property)
			{
				case "Gold":
					return runs?.Average(x => x.Gold);
				case "Dust":
					return runs?.Average(x => x.Dust);
				case "Packs":
					return runs?.Average(x => x.PackCount);
				case "CardCount":
					return runs?.Average(x => x.CardCount - x.CardCountGolden);
				case "CardCountGolden":
					return runs?.Average(x => x.CardCountGolden);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
