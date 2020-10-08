using System.IO;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    public class FObjectResource
    {
        public FName ObjectName;
        public int Index;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        internal FObjectResource() { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public FObjectResource(UAssetConverter converter)
        {
            using (BinaryReader br = new BinaryReader(converter.GetAssetStream(), Encoding.UTF8, true))
            {
                this.Index = br.ReadInt32();
                this.ObjectName = new FName(converter.GetNameMap(), converter.GetAssetStream());
            }
        }
    }
}
