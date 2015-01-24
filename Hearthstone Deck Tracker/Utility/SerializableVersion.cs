#region

using System;
using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[XmlRoot("Version")]
	public class SerializableVersion : IComparable
	{
		public int Build;
		public int Major;
		public int Minor;
		public int Revision;

		public SerializableVersion()
		{
		}

		public SerializableVersion(int major, int minor)
		{
			Major = major;
			Minor = minor;
			Revision = 0;
			Build = 0;
		}

		public SerializableVersion(Version v)
		{
			Major = v.Major;
			Minor = v.Minor;
			Revision = v.Revision;
			Build = v.Build;
		}

		public static SerializableVersion Default
		{
			get { return new SerializableVersion(1, 0); }
		}

		public int CompareTo(object obj)
		{
			var other = obj as SerializableVersion;
			if(other == null)
				return -1;

			return new Version(Major, Minor, Build, Revision).CompareTo(new Version(other.Major, other.Minor, other.Revision, other.Build));
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
		}


		/// <summary>
		/// {M}: Major, {m}: Minor, {r}: Revision, {b}: Build
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format)
		{
			return format.Replace("{M}", Major.ToString())
			             .Replace("{m}", Minor.ToString())
			             .Replace("{r}", Revision.ToString())
			             .Replace("{b}", Build.ToString());
		}

		public override bool Equals(Object obj)
		{
			// If parameter is null return false.
			if(obj == null)
				return false;

			// If parameter cannot be cast to Point return false.
			var p = obj as SerializableVersion;
			if((Object)p == null)
				return false;
			// Return true if the fields match:
			return this.Equals(p);
		}

		public bool Equals(SerializableVersion sv)
		{
			// If parameter is null return false:
			if((object)sv == null)
				return false;

			// Return true if the fields match:
			return this.Major == sv.Major && this.Minor == sv.Minor && this.Revision == sv.Revision && this.Build == sv.Build;
		}

		public static SerializableVersion IncreaseMajor(SerializableVersion sv)
		{
			return new SerializableVersion(sv.Major + 1, 0);
		}

		public static SerializableVersion IncreaseMinor(SerializableVersion sv)
		{
			return new SerializableVersion(sv.Major, sv.Minor + 1);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(SerializableVersion a, SerializableVersion b)
		{
			// If both are null, or both are same instance, return true.
			if(ReferenceEquals(a, b))
				return true;

			// If one is null, but not both, return false.
			if(((object)a == null) || ((object)b == null))
				return false;

			// Return true if the fields match:
			return a.Equals(b);
		}

		public static bool operator !=(SerializableVersion a, SerializableVersion b)
		{
			return !(a == b);
		}

		public static SerializableVersion Parse(string verionString)
		{
			try
			{
				return new SerializableVersion(Version.Parse(verionString.Replace("v", "")));
			}
			catch(Exception ex)
			{
				throw new Exception("Invalid version string", ex);
			}
		}
	}
}