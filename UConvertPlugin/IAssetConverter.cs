using System.Collections.Generic;
using System.IO;
using UConvertPlugin.Unreal;

namespace UConvertPlugin
{
    public interface IAssetConverter
    {
        public Stream GetAssetStream();
        public Stream GetExportStream();
        public FPackageFileSummary GetSummary();
        public List<FNameEntrySerialized> GetNameMap();
        public List<FObjectImport> GetImportMap();
        public List<FObjectExport> GetExportMap();
        public List<int> GetDependencyMap();
        public List<int> GetPreloadDependencyMap();
    }
}
