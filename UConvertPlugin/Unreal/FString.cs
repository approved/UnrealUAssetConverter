using System.IO;
using System.Text;

namespace UConvertPlugin.Unreal
{
    public class FString
    {
        public string Value;

        public FString(string str)
        {
            this.Value = str;
        }

        public FString(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int size = br.ReadInt32();
                if (size >= 0)
                {

                    char[] chars = br.ReadChars(size);
                    if (chars.Length >= 0)
                    {
                        this.Value = new string(chars).Trim().Trim('\0');
                        return;
                    }
                }

                this.Value = string.Empty;
            }
        }

        public static implicit operator string(FString str) => str.Value;
    }
}
