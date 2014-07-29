using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
    [XmlRoot("Version")]
    public class SerializableVersion
    {
        public int Major;
        public int Minor;
        public int Revision;
        public int Build;
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
        }
    }
}
