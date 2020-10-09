using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UConvertPlugin.Unreal
{
    public class FName
    {
        public const string NAME_None = "NAME_None";

        public string Name;
        public int NameCount = 0;

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
                this.NameCount = br.ReadInt32();
                List<FNameEntrySerialized>? fNameEntries = names.ToList();
                if (fNameEntries.Count > nameIndex)
                {
                    this.Name = fNameEntries[nameIndex].Name;
                    if (this.NameCount > 0)
                    {
                        this.Name += "_" + this.NameCount;
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
            this.NameCount = 0;
            foreach (FNameEntrySerialized name in names)
            {
                if (name.Name.Equals(baseName))
                {
                    this.NameCount++;
                }
            }
            this.Name = baseName;
            if (this.NameCount > 0)
            {
                this.Name += $"_{this.NameCount}";
            }
        }

        public static implicit operator string(FName name) => name.Name;
    }
}
