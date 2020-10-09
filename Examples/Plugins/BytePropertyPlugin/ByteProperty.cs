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
        public bool IsPostProperty() => true;
        public bool IsPreProperty() => true;

        public void Deserialize(IAssetConverter converter, bool isPre = false)
        {
            using (BinaryReader br = new BinaryReader(converter.GetAssetStream(), Encoding.UTF8, true))
            {
                if (isPre)
                {
                    this.Name = new FName(converter.GetNameMap(), converter.GetAssetStream());
                }
                else
                {
                    this.Value = br.ReadByte();
                }
            }
        }

        public byte[] Serialize(IAssetConverter converter, bool isPre = false)
        {
            if(isPre)
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
            else
            {
                return new byte[] { this.Value };
            }
        }
    }
}
