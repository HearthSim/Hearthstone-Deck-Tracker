using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Config_With_Attribute_Defaults
{
	public class Config
	{

		[DefaultValue(36)]
		public double SecretsHeight { get; set; }
		[DefaultValue(35)]
		public double SecretsLeft { get; set; }
		[DefaultValue(35)]
		public double SecretsTop { get; set; }


		private Config() { }


		private static Config _Instance;
		public static Config Instance
		{
			get
			{
				if(_Instance == null)
				{
					_Instance = new Config();
					_Instance.ResetAll();
				}

				return _Instance;
			}
		}

		public void ResetAll()
		{
			// Use the DefaultValue property of each property to actually set it, via reflection.
			foreach(PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
			{
				var attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
				if(attr != null)
				{
					prop.SetValue(this, attr.Value);
				}
			}
		}

		public void Reset(string PropertyName)
		{
			//TODO: Upgrade to use LINQ and not the property's name!!
			var property = this.GetType().GetProperty(PropertyName);
			var attribute = property.CustomAttributes.OfType<DefaultValueAttribute>().First();

			property.SetValue(this, attribute.Value);
		}
	}
}
