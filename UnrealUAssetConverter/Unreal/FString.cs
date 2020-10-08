using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    public class FString
    {
        public string Value;

        public FString(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int size = br.ReadInt32();
                if (size == 0)
                {
                    return;
                }

                char[] chars = br.ReadChars(size);
                if (chars.Length == 0)
                {
                    return;
                }

                this.Value = new string(chars).Trim().Trim('\0');
            }
        }

        public static implicit operator string(FString str) => str.Value;
    }
}
