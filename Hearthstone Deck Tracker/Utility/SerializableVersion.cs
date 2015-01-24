#region

using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker
{
    [XmlRoot("Version")]
    public class SerializableVersion : System.IComparable
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

        public SerializableVersion(System.Version v)
        {
            Major = v.Major;
            Minor = v.Minor;
            Revision = v.Revision;
            Build = v.Build;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            SerializableVersion p = obj as SerializableVersion;
            if ((System.Object)p == null)
            {
                return false;
            }
            // Return true if the fields match:
            return this.Equals(p);
        }

        public bool Equals(SerializableVersion sv)
        {
            // If parameter is null return false:
            if ((object)sv == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this.Major == sv.Major
                && this.Minor == sv.Minor
                && this.Revision == sv.Revision
                && this.Build == sv.Build;
        }

        public static SerializableVersion IncreaseMajor(SerializableVersion sv)
        {
            return new SerializableVersion(sv.Major + 1, 0);
        }

        public static SerializableVersion IncreaseMinor(SerializableVersion sv)
        {
            return new SerializableVersion(sv.Major, sv.Minor + 1);
        }

        public static SerializableVersion Default { get { return new SerializableVersion(1, 0); } }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            SerializableVersion other = obj as SerializableVersion;
            if (other == null)
            {
                return -1;
            }

            return new System.Version(Major, Minor, Build, Revision).CompareTo(
                new System.Version(other.Major, other.Minor, other.Revision, other.Build));
        }

        public static bool operator ==(SerializableVersion a, SerializableVersion b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(SerializableVersion a, SerializableVersion b)
        {
            return !(a == b);
        }
    }
}