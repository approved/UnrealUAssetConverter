using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UConvertPlugin.Unreal
{
    //TODO: Replace with dynamically compiled FGuid struct
    public class FGuid
    {
        private static readonly Regex matcher = new Regex("([A-F0-9]{8}-?){4}");

        private readonly int A;
        private readonly int B;
        private readonly int C;
        private readonly int D;

        public string Guid => this.ToString();

        public FGuid(string guid)
        {
            string[] sections = guid.Split('-');
            if (sections.Length != 4 || !matcher.IsMatch(guid))
            {
                throw new InvalidDataException("Provided GUID did not meet specification '([A-F0-9]{8}-?){4}'");
            }
            A = int.Parse(sections[0], NumberStyles.HexNumber);
            B = int.Parse(sections[1], NumberStyles.HexNumber);
            C = int.Parse(sections[2], NumberStyles.HexNumber);
            D = int.Parse(sections[3], NumberStyles.HexNumber);
        }

        public FGuid(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                A = br.ReadInt32();
                B = br.ReadInt32();
                C = br.ReadInt32();
                D = br.ReadInt32();
            }
        }

        public override string ToString() => $"{A:X}-{B:X}-{C:X}-{D:X}";
    }
}
