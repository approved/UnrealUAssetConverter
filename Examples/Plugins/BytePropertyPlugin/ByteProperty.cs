using System;
using System.IO;
using System.Text;
using UConvertPlugin;
using UConvertPlugin.Unreal;

namespace BytePropertyPlugin
{
    public class ByteProperty : IConverterPlugin
    {
        public FName Name;
        public byte Value;

        public string GetPropertyName() => "ByteProperty";
        public bool HasTagData() => true;
        public bool HasTagValue() => true;

        public void DeserializePropertyTagData(IAssetConverter converter)
        {
            this.Name = new FName(converter.GetNameMap(), converter.GetExportStream());
        }

        public void DeserializePropertyTagValue(IAssetConverter converter)
        {
            using (BinaryReader br = new BinaryReader(converter.GetExportStream(), Encoding.UTF8, true))
            {
                this.Value = br.ReadByte();
            }
        }

        public byte[] SerializePropertyTagData(IAssetConverter converter)
        {
            byte[] bytes = new byte[8];
            int i = 0;
            foreach (FNameEntrySerialized name in converter.GetNameMap())
            {
                if (name.Equals(this.Name))
                {
                    Array.Copy(BitConverter.GetBytes(i), bytes, 4);
                    break;
                }
                i++;
            }
            Array.Copy(BitConverter.GetBytes(this.Name.NameCount), 0, bytes, 4, 4);
            return bytes;
        }

        public byte[] SerializePropertyTagValue(IAssetConverter converter)
        {
            return new byte[] { this.Value };
        }
    }
}
