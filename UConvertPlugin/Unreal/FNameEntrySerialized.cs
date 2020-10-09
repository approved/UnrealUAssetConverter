using System.IO;
using System.Text;

namespace UConvertPlugin.Unreal
{
    public class FNameEntrySerialized
    {
        private const int MaxNameSize = 1024;

        public int NameIndex;
        public long FileOffset;
        public FString Name;
        public ushort NonCasePreservingHash;
        public ushort CasePreservingHash;

        public FNameEntrySerialized(string str, long fileOffset = 0)
        {
            this.FileOffset = fileOffset;
            this.Name.Value = str;
            this.NonCasePreservingHash = 0;
            this.CasePreservingHash = (ushort)(UnrealCrc.StrCrc32(str) & 0xFFFF);
        }

        public FNameEntrySerialized(Stream stream)
        {
            this.FileOffset = stream.Position;
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                this.Name = new FString(stream);
                if (this.Name.Value.Length > MaxNameSize)
                {
                    throw new InvalidDataException("Name longer than expected. Can not continue reading.");
                }
                this.NonCasePreservingHash = br.ReadUInt16();
                this.CasePreservingHash = br.ReadUInt16();

                ushort hash = (ushort)(UnrealCrc.StrCrc32(this.Name) & 0xFFFF);
                if (CasePreservingHash != hash)
                {
                    throw new InvalidDataException($"Invalid Case Hash found in UObject: {this.Name}. Expected {hash:X4} but found {this.CasePreservingHash:X4}.");
                }
            }
        }
    }
}
