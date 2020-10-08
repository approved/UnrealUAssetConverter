using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using UnrealUAssetConverter.Unreal;

namespace UnrealUAssetConverter
{
    /* Alias for Unreal's Linker */
    public class UAssetConverter : IDisposable
    {
        private bool _isDisposed = false;

        private readonly Stream _openAssetStream;
        public Stream GetAssetStream() => this._openAssetStream;

        private readonly Stream? _openExportStream;
        public Stream GetExportStream()
        {
            if (this._openExportStream is not null)
            {
                return this._openExportStream;
            }

            throw new NullReferenceException($"Tried to access a null Export Stream. Export file or stream must be passed to the constructor for {nameof(UAssetConverter)}.");
        }

        private FPackageFileSummary? _packageFileSummary;
        private readonly List<FNameEntrySerialized> _names = new List<FNameEntrySerialized>();
        private readonly List<FObjectImport> _imports = new List<FObjectImport>();
        private readonly List<FObjectExport> _exports = new List<FObjectExport>();
        private readonly List<int> _dependencies = new List<int>();
        private readonly List<int> _preloadDependencies = new List<int>();

        public UAssetConverter(FileInfo assetFile)
        {
            Contract.Requires(assetFile.Exists);

            this._openAssetStream = assetFile.OpenRead();
        }

        public UAssetConverter(FileInfo assetFile, FileInfo exportFile)
        {
            Contract.Requires(assetFile.Exists);
            Contract.Requires(exportFile.Exists);

            this._openAssetStream = assetFile.OpenRead();
            this._openExportStream = exportFile.OpenRead();
        }

        public UAssetConverter(string assetFile, string exportFile = "")
        {
            Contract.Requires(!string.IsNullOrEmpty(assetFile));

            this._openAssetStream = new FileStream(assetFile, FileMode.Open, FileAccess.Read);
            if(!string.IsNullOrEmpty(exportFile))
            {
                this._openExportStream = new FileStream(exportFile, FileMode.Open, FileAccess.Read);
            }
        }

        public UAssetConverter(byte[] assetData)
        {
            Contract.Requires(assetData.Length > 0);

            this._openAssetStream = new MemoryStream(assetData);
        }

        public UAssetConverter(byte[] assetData, byte[] exportData)
        {
            Contract.Requires(assetData.Length > 0);

            this._openAssetStream = new MemoryStream(assetData);
            this._openExportStream = new MemoryStream(exportData);
        }

        public FPackageFileSummary GetSummary()
        {
            if (this._packageFileSummary is null)
            {
                this._packageFileSummary = FPackageFileSummary.Deserialize(this._openAssetStream);
            }

            return this._packageFileSummary;
        }

        public List<FNameEntrySerialized> GetNameMap()
        {
            if (this._names.Count == 0)
            {
                FPackageFileSummary? summ = GetSummary();
                if (summ.NameCount > 0)
                {
                    if (this._openAssetStream.Position != summ.NameOffset)
                    {
                        this._openAssetStream.Seek(summ.NameOffset, SeekOrigin.Begin);
                    }

                    for (int i = 0; i < summ.NameCount; i++)
                    {
                        FNameEntrySerialized name = new FNameEntrySerialized(this._openAssetStream)
                        {
                            NameIndex = i
                        };
                        this._names.Add(name);
                    }
                }
            }

            return this._names;
        }

        public List<FObjectImport> GetImportMap()
        {
            if (this._imports.Count == 0)
            {
                FPackageFileSummary? summ = GetSummary();
                if (summ.ImportCount > 0)
                {
                    if (this._openAssetStream.Position != summ.ImportOffset)
                    {
                        this._openAssetStream.Seek(summ.ImportOffset, SeekOrigin.Begin);
                    }

                    for (int i = 0; i < summ.ImportCount; i++)
                    {
                        this._imports.Add(new FObjectImport(this));
                    }
                }
            }

            return this._imports;
        }

        public List<FObjectExport> GetExportMap()
        {
            if (this._exports.Count == 0)
            {
                FPackageFileSummary? summ = GetSummary();
                if (summ.ExportCount > 0)
                {
                    if (this._openAssetStream.Position != summ.ExportOffset)
                    {
                        this._openAssetStream.Seek(summ.ExportOffset, SeekOrigin.Begin);
                    }

                    for (int i = 0; i < summ.ExportCount; i++)
                    {
                        this._exports.Add(new FObjectExport(this));
                    }
                }
            }

            return this._exports;
        }

        public List<int> GetDependencyMap()
        {
            if (this._dependencies.Count == 0)
            {
                FPackageFileSummary? summ = GetSummary();
                if (summ.DependsOffset == 0)
                {
                    throw new InvalidDataException("Package was saved improperly. Can not load any further");
                }

                if (summ.ExportCount > 0)
                {
                    if(this._openAssetStream.Position != summ.DependsOffset)
                    {
                        this._openAssetStream.Seek(summ.DependsOffset, SeekOrigin.Begin);
                    }

                    using (BinaryReader br = new BinaryReader(this._openAssetStream, Encoding.UTF8, true))
                    {
                        for (int i = 0; i < summ.ExportCount; i++)
                        {
                            this._dependencies.Add(br.ReadInt32());
                        }
                    }
                }
            }

            return this._dependencies;
        }

        public List<int> GetPreloadDependencyMap()
        {
            if (this._preloadDependencies.Count == 0)
            {
                FPackageFileSummary? summ = GetSummary();
                if (summ.PreloadDependencyCount >= 1 && summ.PreloadDependencyOffset != 0)
                {
                    if (this._openAssetStream.Position != summ.PreloadDependencyOffset)
                    {
                        this._openAssetStream.Seek(summ.PreloadDependencyOffset, SeekOrigin.Begin);
                    }

                    using (BinaryReader br = new BinaryReader(this._openAssetStream, Encoding.UTF8, true))
                    {
                        for (int i = 0; i < summ.PreloadDependencyCount; i++)
                        {
                            this._preloadDependencies.Add(br.ReadInt32());
                        }
                    }
                }
            }

            return this._preloadDependencies;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._isDisposed) return;

            if (disposing)
            {
                if (this._openAssetStream is not null)
                {
                    this._openAssetStream.Close();
                }

                if(this._openExportStream is not null)
                {
                    this._openExportStream.Close();
                }
            }

            this._isDisposed = true;
        }
    }
}
