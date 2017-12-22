#region

using System;
using System.Windows.Markup;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	//https://summergoat.wordpress.com/2008/07/08/enum-getvalues-markup-extension/
	public class EnumValuesExtension : MarkupExtension
	{
		private readonly Type _enumType;

		public EnumValuesExtension(Type enumType)
		{
			if(enumType == null)
				throw new ArgumentNullException("enumType");
			if(!enumType.IsEnum)
				throw new ArgumentException("Argument enumType must derive from type Enum.");

			_enumType = enumType;
		}

		public override object ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues(_enumType);
	}
}
