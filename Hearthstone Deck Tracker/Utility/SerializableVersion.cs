#region

using Hearthstone_Deck_Tracker.Utility.Logging;
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
			if(v == null)
				return;
			Major = v.Major > 0 ? v.Major : 0;
			Minor = v.Minor > 0 ? v.Minor : 0;
			Revision = v.Revision > 0 ? v.Revision : 0;
			Build = v.Build > 0 ? v.Build : 0;
		}

		public static SerializableVersion Default => new SerializableVersion(1, 0);

		[XmlIgnore]
		public string ShortVersionString => ToString("v{M}.{m}");

		public int CompareTo(object obj)
		{
			var other = obj as SerializableVersion;
			if(other == null)
				return -1;

			return new Version(Major, Minor, Build, Revision).CompareTo(new Version(other.Major, other.Minor, other.Revision, other.Build));
		}

		public override string ToString() => string.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);

		public string ToString(bool reverseRevBuild) => string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);

		/// <summary>
		/// {M}: Major, {m}: Minor, {r}: Revision, {b}: Build
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format) => format.Replace("{M}", Major.ToString())
													   .Replace("{m}", Minor.ToString())
													   .Replace("{r}", Revision.ToString())
													   .Replace("{b}", Build.ToString());

		public override bool Equals(object obj)
		{
			// If parameter is null return false.
			if(obj == null)
				return false;

			// If parameter cannot be cast to Point return false.
			var p = obj as SerializableVersion;
			if((object)p == null)
				return false;
			// Return true if the fields match:
			return Equals(p);
		}

		public bool Equals(SerializableVersion sv)
		{
			// If parameter is null return false:
			if((object)sv == null)
				return false;

			// Return true if the fields match:
			return Major == sv.Major && Minor == sv.Minor && Revision == sv.Revision && Build == sv.Build;
		}

		public static SerializableVersion IncreaseMajor(SerializableVersion sv) => new SerializableVersion(sv.Major + 1, 0);

		public static SerializableVersion IncreaseMinor(SerializableVersion sv) => new SerializableVersion(sv.Major, sv.Minor + 1);

		public override int GetHashCode() => base.GetHashCode();

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

		public static bool operator !=(SerializableVersion a, SerializableVersion b) => !(a == b);

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

		public static SerializableVersion ParseOrDefault(string verionString)
		{
			try
			{
				if(string.IsNullOrEmpty(verionString))
					return Default;
				if(!verionString.Contains("."))
					verionString += ".0";
				if(Version.TryParse(verionString.Replace("v", ""), out var version))
					return new SerializableVersion(version);
				return Default;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return Default;
			}
		}
	}
}
