using System.IO;
using System.Text;

namespace UConvertPlugin.Unreal
{
    public class FEngineVersionBase
    {
        /// <summary>
        /// Major version number.
        /// </summary>
        protected short Major;

        /// <summary>
        /// Minor version number.
        /// </summary>
        protected short Minor;

        /// <summary>
        /// Patch version number.
        /// </summary>
        protected short Patch;

        /// <summary>
        /// Changelist number. This is used to arbitrate when Major/Minor/Patch version numbers match.
        /// </summary>
        protected int Changelist;

        public FEngineVersionBase(short major = 0, short minor = 0, short patch = 0, int changelist = 0)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Changelist = changelist;
        }

        public FEngineVersionBase(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                Major = br.ReadInt16();
                Minor = br.ReadInt16();
                Patch = br.ReadInt16();
                Changelist = br.ReadInt32();
            }
        }
    }
}
