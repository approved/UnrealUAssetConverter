using System.IO;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    public class FObjectImport : FObjectResource
    {
        public FName ClassPackage;
        public FName ClassName;

        public FObjectImport(UAssetConverter converter) : base(converter)
        {
            using (BinaryReader br = new BinaryReader(converter.GetAssetStream(), Encoding.UTF8, true))
            {
                this.ClassPackage = new FName(converter.GetNameMap(), converter.GetAssetStream());
                this.ClassName = new FName(converter.GetNameMap(), converter.GetAssetStream());
            }
        }
    }
}
