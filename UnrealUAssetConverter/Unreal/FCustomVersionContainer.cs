using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    public class FCustomVersionContainer
    {
        /// <summary>
        /// Array containing custom versions.
        /// </summary>
        public List<FCustomVersion> CustomVersion = new List<FCustomVersion>();

        public FCustomVersionContainer() { }

        public FCustomVersionContainer(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int count = br.ReadInt32();
                CustomVersion = new List<FCustomVersion>();
                for (int i = 0; i < count; i++)
                {
                    CustomVersion.Add(new FCustomVersion(stream));
                }
            }
        }
    }
}
