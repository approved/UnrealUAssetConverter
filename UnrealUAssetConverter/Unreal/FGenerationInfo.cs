using System.IO;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    public class FGenerationInfo
    {
        /// <summary>
        /// Number of exports in the ExportMap for this generation.
        /// </summary>
        public int ExportCount;

        /// <summary>
        /// Number of names in the NameMap for this generation.
        /// </summary>
        public int NameCount;

        public FGenerationInfo(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                ExportCount = br.ReadInt32();
                NameCount = br.ReadInt32();
            }
        }
    }
}
