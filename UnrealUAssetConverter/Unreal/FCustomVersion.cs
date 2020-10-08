using System.IO;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    /// <summary>
    /// Structure to hold unique custom key with its version.
    /// </summary>
    public class FCustomVersion
    {
        /// <summary>
        /// Unique custom key.
        /// </summary>
        public FGuid Key;

        /// <summary>
        /// Custom version.
        /// </summary>
        public int Version;

        /// <summary>
        /// Number of times this GUID has been registered
        /// </summary>
        public int ReferenceCount;

        public FCustomVersion(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                Key = new FGuid(stream);
                Version = br.ReadInt32();
                ReferenceCount = br.ReadInt32();
            }
        }
    }
}
