using System.IO;

namespace UnrealUAssetConverter.Unreal
{
    public class FEngineVersion : FEngineVersionBase
    {
        /// <summary>
        /// Branch name.
        /// </summary>
        public FString Branch;

        public FEngineVersion(short major = 0, short minor = 0, short patch = 0, int changelist = 0, string branch = "") : base(major, minor, patch, changelist)
        {
            Branch.Value = branch;
        }

        public FEngineVersion(Stream stream) : base(stream)
        {
            this.Branch = new FString(stream);
        }
    }
}
