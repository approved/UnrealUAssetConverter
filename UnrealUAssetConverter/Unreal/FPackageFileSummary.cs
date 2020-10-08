using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/* Base summaries provided by UE4 Source @ https://github.com/EpicGames/UnrealEngine */
namespace UnrealUAssetConverter.Unreal
{
    /// <summary>
    /// A "table of contents" for an Unreal package file.
    /// Stored at the top of the file.
    /// Located in the .uasset/.umap file when extracted.
    /// </summary>
    public class FPackageFileSummary
    {
        public const uint   PACKAGE_FILE_TAG                = 0x9E2A83C1;
        public const int    CURRENT_LEGACY_FILE_VERSION    = -7;

        private const int   MinimumPackageSize = 32;

        private bool _isFilterEditorOnly;
        public bool IsFilterEditorOnly => this._isFilterEditorOnly;

        /// <summary>
        /// Magic tag compared against PACKAGE_FILE_TAG to ensure that package is an Unreal package.
        /// </summary>
        private uint _tag;

        /// <summary>
        /// Legacy file version.
        /// </summary>
        private int _legacyFileVersion;

        /// <summary>
        /// Legacy UE3 file version.
        /// </summary>
        private int _legacyUE3Version;

        /// <summary>
        /// UE4 file version.
        /// </summary>
        private int _fileVersionUE4;

        /// <summary>
        /// UE4 file version.
        /// </summary>
        /// <returns></returns>
        public int GetUE4Version() => this._fileVersionUE4;

        /// <summary>
        /// Licensee file version.
        /// </summary>
        private int _fileVersionLicenseeUE4;

        /// <summary>
        /// Custom version numbers. Keyed off a unique tag for each custom component.
        /// </summary>
        public FCustomVersionContainer CustomVersion;

        /// <summary>
        /// Total size of all information that needs to be read.
        /// <para>his includes the package file summary, name table and import & export maps.</para>
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int TotalSize;

        /// <summary>
        /// The flags for the package.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public EPackageFlags PackageFlags;

        /// <summary>
        /// The Generic Browser Folder name that this package lives in.
        /// </summary>
        public FString FolderName;

        /// <summary>
        /// Number of names used in this package
        /// </summary>
        public int NameCount;

        /// <summary>
        /// Location into the file on disk for the name data.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int NameOffset;

        /// <summary>
        /// Localization ID of this package.
        /// </summary>
        public FString LocalizationId;

        /// <summary>
        /// Number of gatherable text data items in this package.
        /// </summary>
        public int GathereableTextDataCount;

        /// <summary>
        /// Location into the file on disk for the gatherable text data items.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int GatherableTextDataOffset;

        /// <summary>
        /// Number of exports contained in this package.
        /// </summary>
        public int ExportCount;

        /// <summary>
        /// Location into the file on disk for the ExportMap data.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int ExportOffset;

        /// <summary>
        /// Number of imports contained in this package.
        /// </summary>
        public int ImportCount;

        /// <summary>
        /// Location into the file on disk for the ImportMap data.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int ImportOffset;

        /// <summary>
        /// Location into the file on disk for the DependsMap data.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int DependsOffset;

        /// <summary>
        /// Number of soft package references contained in this package.
        /// </summary>
        public int SoftPackageReferencesCount;

        /// <summary>
        /// Location into the file on disk for the soft package reference list.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int SoftPackageReferencesOffset;

        /// <summary>
        /// Location into the file on disk for the SearchableNamesMap data.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int SearchableNamesOffset;

        /// <summary>
        /// Thumbnail table offset.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int ThumbnailTableOffset;

        /// <summary>
        /// Current id for this package.
        /// </summary>
        public FGuid Guid;

        /// <summary>
        /// Data about previous versions of this package
        /// </summary>
        public FGenerationInfo[] Generations;

        /// <summary>
        /// Engine version this package was saved with. 
        /// <para>For hotfix releases and engine versions which maintain strict binary compatibility with another version, this may differ from CompatibleWithEngineVersion.</para>
        /// </summary>
        public FEngineVersion SavedByEngineVersion;

        /// <summary>
        /// Engine version this package is compatible with.
        /// <para>See <see cref="SavedByEngineVersion"/>.</para>
        /// </summary>
        public FEngineVersion CompatibleWithEngineVersion;

        /// <summary>
        /// Flags used to compress the file on save and uncompress on load.
        /// </summary>
        public ECompressionFlags CompressionFlags;

        /// <summary>
        /// Value that is used to determine if the package was saved by Epic (or licensee) or by a modder, etc
        /// </summary>
        public int PackageSource;

        /// <summary>
        /// If true, this file will not be saved with version numbers or was saved without version numbers. In this case they are assumed to be the current version.
        /// <para>This is only used for full cooks for distribution because it is hard to guarantee correctness.</para>
        /// </summary>
        public bool IsUnversioned;

        /// <summary>
        /// Location into the file on disk for the asset registry tag data.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int AssetRegistryDataOffset;

        /// <summary>
        /// Offset to the location in the file where the bulkdata starts.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public long BulkDataStartOffset;

        /// <summary>
        /// Offset to the location in the file where the FWorldTileInfo data starts.
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int WorldTileInfoDataOffset;

        /// <summary>
        /// Streaming install ChunkIDs.
        /// </summary>
        public List<int> ChunkIds;

        /// <summary>
        /// Number of data preload dependenciese contatined in the package.
        /// </summary>
        public int PreloadDependencyCount;

        /// <summary>
        /// Location into the file on disk for the preload dependency data
        /// <para>This will need to be recalculated every time a size change is made to any field.</para>
        /// </summary>
        public int PreloadDependencyOffset;

        private FPackageFileSummary(byte[] bytes) : this(new MemoryStream(bytes)) { }

        private FPackageFileSummary(Stream stream)
        {
            if (stream.Length < 32)
            {
                Console.WriteLine("Failed to read Package File Summary - File too small");
                return;
            }

            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                this._tag = br.ReadUInt32();
                if (this._tag != PACKAGE_FILE_TAG)
                {
                    throw new InvalidDataException("The file contains unrecognizable data, check that it is of the expected type.");
                }

                this._legacyFileVersion = br.ReadInt32();
                if (this._legacyFileVersion < 0)
                {
                    if (this._legacyFileVersion < CURRENT_LEGACY_FILE_VERSION)
                    {
                        this._fileVersionUE4 = 0;
                        this._fileVersionLicenseeUE4 = 0;
                        return;
                    }

                    if (this._legacyFileVersion != -4)
                    {
                        this._legacyUE3Version = br.ReadInt32();
                    }

                    this._fileVersionUE4 = br.ReadInt32();
                    this._fileVersionLicenseeUE4 = br.ReadInt32();

                    if (this._legacyFileVersion <= -2)
                    {
                        this.CustomVersion = new FCustomVersionContainer(stream);
                    }

                    if (this._fileVersionUE4 == 0 && this._fileVersionLicenseeUE4 == 0)
                    {
                        this.IsUnversioned = true;
                        this._fileVersionUE4 = 516;
                        CustomVersion = new FCustomVersionContainer();
                    }
                }
                else
                {
                    this._fileVersionUE4 = 0;
                    this._fileVersionLicenseeUE4 = 0;
                }

                this.TotalSize = br.ReadInt32();
                this.FolderName = new FString(stream);
                this.PackageFlags = (EPackageFlags)br.ReadInt32();

                if (this.PackageFlags.HasFlag(EPackageFlags.PKG_FilterEditorOnly))
                {
                    this._isFilterEditorOnly = true;
                }

                this.NameCount = br.ReadInt32();
                this.NameOffset = br.ReadInt32();

                if (this._fileVersionUE4 >= 459)
                {
                    this.GathereableTextDataCount = br.ReadInt32();
                    this.GatherableTextDataOffset = br.ReadInt32();
                }

                this.ExportCount = br.ReadInt32();
                this.ExportOffset = br.ReadInt32();

                this.ImportCount = br.ReadInt32();
                this.ImportOffset = br.ReadInt32();

                this.DependsOffset = br.ReadInt32();

                if (this._fileVersionUE4 < 214 || this._fileVersionUE4 > 516)
                {
                    Console.WriteLine($"Unsupported Package Version '{this._fileVersionUE4}'. Can not read further.");
                    return;
                }

                if (this._fileVersionUE4 >= 384)
                {
                    this.SoftPackageReferencesCount = br.ReadInt32();
                    this.SoftPackageReferencesOffset = br.ReadInt32();
                }

                if (this._fileVersionUE4 >= 510)
                {
                    this.SearchableNamesOffset = br.ReadInt32();
                }

                this.ThumbnailTableOffset = br.ReadInt32();

                this.Guid = new FGuid(stream);
                int generationCount = br.ReadInt32();
                this.Generations = new FGenerationInfo[generationCount];
                for (int i = 0; i < generationCount; i++)
                {
                    this.Generations[i] = new FGenerationInfo(stream);
                }

                if (this._fileVersionUE4 >= 336)
                {
                    this.SavedByEngineVersion = new FEngineVersion(stream);
                }
                else
                {
                    this.SavedByEngineVersion = new FEngineVersion();
                }

                if (this._fileVersionUE4 >= 444)
                {
                    this.CompatibleWithEngineVersion = new FEngineVersion(stream);
                }
                else
                {
                    this.CompatibleWithEngineVersion = this.SavedByEngineVersion;
                }

                this.CompressionFlags = (ECompressionFlags)br.ReadInt32();

                if (((int)this.CompressionFlags & (int)~(CompressionFlagMasks.COMPRESSION_FLAGS_TYPE_MASK | CompressionFlagMasks.COMPRESSION_FLAGS_OPTIONS_MASK)) != 0)
                {
                    Console.WriteLine("Invalid Compression Flags");
                    return;
                }

                int compressedChunkCount = br.ReadInt32();
                if (compressedChunkCount > 0)
                {
                    // This file has package level compression, we won't load it.
                    Console.WriteLine("Failed to read package file summary, the file has package level compression (and is probably cooked). These old files cannot be loaded.");
                    this._fileVersionUE4 = 213;
                    return; // We can't safely load more than this because we just changed the version to something it is not.
                }

                this.PackageSource = br.ReadInt32();

                int numAddPackages = br.ReadInt32();
                FString[] AdditionalPackagesToCook = new FString[numAddPackages];
                for (int i = 0; i < numAddPackages; i++)
                {
                    AdditionalPackagesToCook[i] = new FString(stream);
                }

                if (this._legacyFileVersion > -7)
                {
                    // Read unused texture allocation info
                    br.ReadInt32();
                }

                this.AssetRegistryDataOffset = br.ReadInt32();
                this.BulkDataStartOffset = br.ReadInt64();

                if(this._fileVersionUE4 >= 224)
                {
                    this.WorldTileInfoDataOffset = br.ReadInt32();
                }

                if(this._fileVersionUE4 >= 326)
                {
                    int numChunks = br.ReadInt32();
                    this.ChunkIds = new List<int>();
                    for (int i = 0; i < numChunks; i++)
                    {
                        this.ChunkIds.Add(br.ReadInt32());
                    }
                }
                else if(this._fileVersionUE4 >= 278)
                {
                    this.ChunkIds = new List<int>() { br.ReadInt32() };
                }

                this.PreloadDependencyCount = -1;
                this.PreloadDependencyOffset = 0;
            }
        }

        public static FPackageFileSummary Deserialize(byte[] bytes)
        {
            return new FPackageFileSummary(bytes);
        }

        public static FPackageFileSummary Deserialize(Stream stream)
        {
            return new FPackageFileSummary(stream);
        }
    }
}
