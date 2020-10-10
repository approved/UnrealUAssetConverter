using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using UConvertPlugin;
using UConvertPlugin.Unreal;

namespace UnrealUAssetConverter
{
    /* Alias for Unreal's Linker */
    public class UAssetConverter : IDisposable, IAssetConverter
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

        private readonly Dictionary<string, Type> _plugins = new Dictionary<string, Type>();
        public Dictionary<string, Type> GetPlugins() => _plugins;

        public UAssetConverter(FileInfo assetFile)
        {
            Contract.Requires(assetFile.Exists);

            this._openAssetStream = assetFile.OpenRead();
            GetConverterPlugins();
        }

        public UAssetConverter(FileInfo assetFile, FileInfo exportFile)
        {
            Contract.Requires(assetFile.Exists);

            this._openAssetStream = assetFile.OpenRead();
            if (exportFile.Exists)
            {
                this._openExportStream = exportFile.OpenRead();
            }
            GetConverterPlugins();
        }

        public UAssetConverter(string assetFile, string exportFile = "")
        {
            Contract.Requires(!string.IsNullOrEmpty(assetFile));

            this._openAssetStream = new FileStream(assetFile, FileMode.Open, FileAccess.Read);
            if(!string.IsNullOrEmpty(exportFile))
            {
                this._openExportStream = new FileStream(exportFile, FileMode.Open, FileAccess.Read);
            }
            GetConverterPlugins();
        }

        public UAssetConverter(byte[] assetData)
        {
            Contract.Requires(assetData.Length > 0);

            this._openAssetStream = new MemoryStream(assetData);
            GetConverterPlugins();
        }

        public UAssetConverter(byte[] assetData, byte[] exportData)
        {
            Contract.Requires(assetData.Length > 0);

            this._openAssetStream = new MemoryStream(assetData);
            this._openExportStream = new MemoryStream(exportData);
            GetConverterPlugins();
        }

        private void GetConverterPlugins()
        {
            if (!Directory.Exists(PluginLoader.PluginDirectory))
            {
                Directory.CreateDirectory(PluginLoader.PluginDirectory);
            }

            List<Type> plugins = PluginLoader.LoadAllPlugins<IConverterPlugin>();
            if (plugins.Count > 0)
            {
                foreach (Type pluginType in plugins)
                {
                    IConverterPlugin? plugin = (IConverterPlugin?)Activator.CreateInstance(pluginType);
                    if (plugin is not null)
                    {
                        string propertyName = plugin.GetPropertyName();
                        this._plugins.Add(propertyName, pluginType);
                    }
                }
            }
        }

        public FPackageFileSummary GetSummary()
        {
            if (this._packageFileSummary is null)
            {
                long lastPos = this._openAssetStream.Position;
                this._openAssetStream.Seek(0, SeekOrigin.Begin);
                this._packageFileSummary = FPackageFileSummary.Deserialize(this._openAssetStream);
                this._openAssetStream.Seek(lastPos, SeekOrigin.Begin);
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
                    long lastPos = this._openAssetStream.Position;
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
                    this._openAssetStream.Seek(lastPos, SeekOrigin.Begin);
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
                    long lastPos = this._openAssetStream.Position;
                    if (this._openAssetStream.Position != summ.ImportOffset)
                    {
                        this._openAssetStream.Seek(summ.ImportOffset, SeekOrigin.Begin);
                    }

                    for (int i = 0; i < summ.ImportCount; i++)
                    {
                        this._imports.Add(new FObjectImport(this));
                    }
                    this._openAssetStream.Seek(lastPos, SeekOrigin.Begin);
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
                    long lastPos = this._openAssetStream.Position;
                    if (this._openAssetStream.Position != summ.ExportOffset)
                    {
                        this._openAssetStream.Seek(summ.ExportOffset, SeekOrigin.Begin);
                    }

                    for (int i = 0; i < summ.ExportCount; i++)
                    {
                        this._exports.Add(new FObjectExport(this));
                    }
                    this._openAssetStream.Seek(lastPos, SeekOrigin.Begin);
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
                    long lastPos = this._openAssetStream.Position;
                    if (this._openAssetStream.Position != summ.DependsOffset)
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
                    this._openAssetStream.Seek(lastPos, SeekOrigin.Begin);
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
                    long lastPos = this._openAssetStream.Position;
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
                    this._openAssetStream.Seek(lastPos, SeekOrigin.Begin);
                }
            }

            return this._preloadDependencies;
        }

        private (bool, object?) GetProperty()
        {
            using (BinaryReader br = new BinaryReader(this.GetExportStream(), Encoding.UTF8, true))
            {
                FName propertyName = new FName(this.GetNameMap(), br.BaseStream);

                if (propertyName.Name.Equals("None"))
                {
                    //TODO: Maybe figure out how to grab other properties if there is still data left.
                    return (false, null);
                }

                FName propertyType = new FName(this.GetNameMap(), br.BaseStream);
                int propertySize = br.ReadInt32();
                int arrayIndex = br.ReadInt32();

                long propPos = br.BaseStream.Position;
                if (_plugins.ContainsKey(propertyType.Name))
                {
                    IConverterPlugin? plugin = (IConverterPlugin?)Activator.CreateInstance(_plugins[propertyType.Name]);
                    if (plugin is not null)
                    {
                        if (plugin.HasTagData())
                        {
                            plugin.DeserializePropertyTagData(this);
                        }

                        if (br.ReadByte() != 0)
                        {
                            new FGuid(this.GetAssetStream());
                        }

                        propPos = br.BaseStream.Position;
                        if (plugin.HasTagValue())
                        {
                            plugin.DeserializePropertyTagValue(this);
                        }

                        br.BaseStream.Seek(propPos, SeekOrigin.Begin);
                        br.BaseStream.Seek(propertySize, SeekOrigin.Current);
                        return (true, plugin);
                    }
                }
                else
                {
                    if (br.ReadByte() != 0)
                    {
                        new FGuid(this.GetAssetStream());
                    }
                    propPos = br.BaseStream.Position;
                }

                br.BaseStream.Seek(propPos, SeekOrigin.Begin);
                br.BaseStream.Seek(propertySize, SeekOrigin.Current);
                return (false, null);
            }
        }

        public List<object> GetExportProperties(FObjectExport export)
        {
            List<object> properties = new List<object>();
            long lastPos = this._openExportStream.Position;
            if (this._openExportStream.Position != export.ExportFileOffset)
            {
                this._openExportStream.Seek(export.ExportFileOffset, SeekOrigin.Begin);
            }

            while (true)
            {
                (bool, object?) property = GetProperty();

                if (property.Item1)
                {
                    properties.Add(property.Item2);
                }
                else
                {
                    break;
                }
            }
            this._openExportStream.Seek(lastPos, SeekOrigin.Begin);

            return properties;
        }

        public List<object> GetValueProperties(string parentType)
        {
            List<object> properties = new List<object>();
            if (_plugins.ContainsKey(parentType))
            {
                IConverterPlugin? plugin = (IConverterPlugin?)Activator.CreateInstance(_plugins[parentType]);
                if (plugin is not null)
                {
                    if (plugin.HasTagValue())
                    {
                        plugin.DeserializePropertyTagValue(this);
                        properties.Add(plugin);
                    }
                }
            }
            else
            {
                while (true)
                {
                    (bool, object?) property = GetProperty();

                    if (property.Item1)
                    {
                        properties.Add(property.Item2);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return properties;
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
