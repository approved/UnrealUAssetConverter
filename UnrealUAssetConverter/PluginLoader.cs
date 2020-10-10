using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace UnrealUAssetConverter
{
    public class PluginLoader : AssemblyLoadContext
    {
        private const string DefaultPluginDirectory = "Plugins";
        public static string PluginDirectory { get; private set; } = DefaultPluginDirectory;

        public static void SetPluginDirectory(string dir)
        {
            PluginDirectory = dir;
        }

        public static void SetDefaultPluginDirectory()
        {
            PluginDirectory = DefaultPluginDirectory;
        }

        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoader(string pluginPath = DefaultPluginDirectory) : base(isCollectible: true)
        {
            this._resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = this._resolver.ResolveAssemblyToPath(assemblyName);
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                return this.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        public Assembly? Load(string assemblyPath)
        {
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                return this.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = this._resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                return this.LoadUnmanagedDllFromPath(libraryPath);
            }

            return (nint)0;
        }

        public Assembly? LoadPlugin(string pluginName)
        {
            if (!string.IsNullOrEmpty(pluginName))
            {
                return this.Load(pluginName);
            }

            return null;
        }

        public Assembly? LoadPlugin(FileInfo plugin)
        {
            return this.LoadPlugin(plugin.Name);
        }

        public static List<Type> LoadAllPlugins<T>()
        {
            List<Type> pluginTypeList = new List<Type>();
            string? assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(assemblyFolder))
            {
                string pluginDirectory = Path.Combine(assemblyFolder, PluginDirectory);
                foreach (string? file in Directory.GetFiles(pluginDirectory))
                {
                    string filePath = Path.GetFullPath(file);
                    PluginLoader alc = new PluginLoader(filePath);
                    Assembly? asm = alc.Load(filePath);
                    if (asm is not null)
                    {
                        IEnumerable<Type>? validPluginTypes = asm.GetTypes().Where(x => typeof(T).IsAssignableFrom(x));
                        foreach (Type? pluginType in validPluginTypes)
                        {
                            pluginTypeList.Add(pluginType);
                        }
                    }
                }
                return pluginTypeList;
            }

            return pluginTypeList;
        }
    }
}
