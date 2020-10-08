using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnrealUAssetConverter.Unreal
{
    public class FName
    {
        public const string NAME_None = "NAME_None";

        public string Name;

        public FName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                Name = NAME_None;
                return;
            }

            Name = str;
        }

        public FName(IEnumerable<FNameEntrySerialized> names, Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int nameIndex = br.ReadInt32();
                int nameNumber = br.ReadInt32();
                List<FNameEntrySerialized>? fNameEntries = names.ToList();
                if (fNameEntries.Count > nameIndex)
                {
                    Name = fNameEntries[nameIndex].Name;
                    if (nameNumber > 0)
                    {
                        Name += "_" + nameNumber;
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException("Bad name index");
                }
            }
        }

        public FName(IEnumerable<FNameEntrySerialized> names, string baseName = NAME_None)
        {
            int numCreated = 0;
            foreach (FNameEntrySerialized name in names)
            {
                if (name.Name.Equals(baseName))
                {
                    numCreated++;
                }
            }
            this.Name = baseName;
            if (numCreated > 0)
            {
                this.Name += $"_{numCreated}";
            }
        }

        public static implicit operator string(FName name) => name.Name;
    }
}
