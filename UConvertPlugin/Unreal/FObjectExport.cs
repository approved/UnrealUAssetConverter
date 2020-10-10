using System.IO;
using System.Text;

namespace UConvertPlugin.Unreal
{
    public class FObjectExport : FObjectResource
    {
        /// <summary>
        /// Location of the resource for this export's class (if non-zero).
        /// <para>A value of zero indicates that this export represents a UClass object; there is no resource for this export's class object.
        /// </para>
        /// </summary>
        public int ClassIndex;

        /// <summary>
        /// Location of this resource in export map. Used for export fixups while loading packages.
        /// <para>Value of zero indicates resource is invalid and shouldn't be loaded.</para>
        /// </summary>
        public int ThisIndex;

        /// <summary>
        /// Location of the resource for this export's SuperField (parent).
        /// <para>Only valid if this export represents a UStruct object. A value of zero indicates that the object represented by this export isn't a UStruct-derived object.</para>
        /// </summary>
        public int SuperIndex;

        /// <summary>
        /// Location of the resource for this export's template/archetypes.
        /// <para>A value of zero indicates that the value of GetArchetype was zero at cook time, which is more or less impossible and checked.</para>
        /// </summary>
        public int TemplateIndex;

        /// <summary>
        /// The object flags for the UObject represented by this resource.
        /// <para>Only flags that match the RF_Load combination mask will be loaded from disk and applied to the UObject.</para>
        /// </summary>
        public EObjectFlags ObjectFlags;

        /// <summary>
        /// The number of bytes to serialize when saving/loading this export's UObject.
        /// </summary>
        public long SerialSize;

        /// <summary>
        /// The location of the beginning of the data for this export's UObject.
        /// </summary>
        public long SerialOffset;

        /// <summary>
        /// The location at the beginning of the export data for this export's UObject.
        /// </summary>
        public long ExportFileOffset;

        /// <summary>
        /// The location of the beginning of the portion of this export's data that is serialized using script serialization.
        /// </summary>
        public long ScriptSerializationStartOffset;

        /// <summary>
        /// The location of the end of the portion of this export's data that is serialized using script serialization.
        /// </summary>
        public long ScriptSerializationEndOffset;

        /// <summary>
        /// Whether the export was forced into the export table via OBJECTMARK_ForceTagExp.
        /// </summary>
        public bool IsForcedExport;

        /// <summary>
        /// Whether the export should be loaded on clients.
        /// </summary>
        public bool IsNotForClient;

        /// <summary>
        /// Whether the export should be loaded on servers.
        /// </summary>
        public bool IsNotForServer;

        /// <summary>
        ///  Whether the export should be always loaded in editor game.
        /// </summary>
        public bool IsNotAlwaysLoadedForEditorGame;

        /// <summary>
        /// True if this export is an asset object.
        /// </summary>
        public bool IsAsset;

        /// <summary>
        /// If this object is a top level package (which must have been forced into the export table via OBJECTMARK_ForceTagExp) this is the GUID for the original package file
        /// </summary>
        public FGuid PackageGuid;

        /// <summary>
        /// If this object is a top level package (which must have been forced into the export table via OBJECTMARK_ForceTagExp) this is the package flags for the original package file.
        /// </summary>
        public EPackageFlags PackageFlags;

        /**
         * The export table must serialize as a fixed size, this is use to index into a long list, which is later loaded into the array. -1 means dependencies are not present
         * These are contiguous blocks, so CreateBeforeSerializationDependencies starts at FirstExportDependency + SerializationBeforeSerializationDependencies
         */
        public int FirstExportDependency;
        public int SerializationBeforeSerializationDependencies;
        public int CreateBeforeSerializationDependencies;
        public int SerializationBeforeCreateDependencies;
        public int CreateBeforeCreateDependencies;

        public FObjectExport(IAssetConverter converter)
        {

            FPackageFileSummary? summ = converter.GetSummary();
            using (BinaryReader br = new BinaryReader(converter.GetAssetStream(), Encoding.UTF8, true))
            {
                this.ClassIndex = br.ReadInt32();
                this.SuperIndex = br.ReadInt32();

                if (summ.GetUE4Version() >= 508)
                {
                    this.TemplateIndex = br.ReadInt32();
                }

                FObjectResource resource = new FObjectResource(converter);
                this.Index = resource.Index;
                this.ObjectName = resource.ObjectName;

                this.ObjectFlags = (EObjectFlags)br.ReadInt32();

                if (summ.GetUE4Version() >= 511)
                {
                    this.SerialSize = br.ReadInt64();
                    this.SerialOffset = br.ReadInt64();
                }
                else
                {
                    this.SerialSize = br.ReadInt32();
                    this.SerialOffset = br.ReadInt32();
                }

                this.ExportFileOffset = SerialOffset - summ.TotalSize;

                this.IsForcedExport = br.ReadInt32() != 0;
                this.IsNotForClient = br.ReadInt32() != 0;
                this.IsNotForServer = br.ReadInt32() != 0;

                this.PackageGuid = new FGuid(converter.GetAssetStream());
                this.PackageFlags = (EPackageFlags)br.ReadInt32();

                if (summ.GetUE4Version() >= 365)
                {
                    this.IsNotAlwaysLoadedForEditorGame = br.ReadInt32() != 0;
                }

                if (summ.GetUE4Version() >= 485)
                {
                    this.IsAsset = br.ReadInt32() != 0;
                }

                if (summ.GetUE4Version() >= 507)
                {
                    this.FirstExportDependency = br.ReadInt32();
                    this.SerializationBeforeSerializationDependencies = br.ReadInt32();
                    this.CreateBeforeSerializationDependencies = br.ReadInt32();
                    this.SerializationBeforeCreateDependencies = br.ReadInt32();
                    this.CreateBeforeCreateDependencies = br.ReadInt32();
                }
            }
        }
    }
}
