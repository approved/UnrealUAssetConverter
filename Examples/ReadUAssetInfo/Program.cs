using Newtonsoft.Json;
using System;
using System.IO;
using UConvertPlugin.Unreal;
using UnrealUAssetConverter;

namespace ReadUAssetInfo
{
    public class Program
    {
        private static string AssetFile;
        private static string ExportFile = string.Empty;

        public static void Main(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
            { 
                AssetFile = args[0];
                if (args.Length > 1 && File.Exists(args[1]))
                {
                    ExportFile = args[1];
                }
            }
            else
            {
                Console.WriteLine("Enter the path to the uasset file you wish to read:");
                AssetFile = Console.ReadLine();

                if (!File.Exists(AssetFile))
                {
                    Console.WriteLine($"Could not locate file {AssetFile}. Please try again");
                    return;
                }
            }

            using (UAssetConverter uaConverter = new UAssetConverter(AssetFile, ExportFile))
            {
                FPackageFileSummary summ = uaConverter.GetSummary();
                Console.WriteLine($"Package Size: {summ.TotalSize} bytes");
                Console.WriteLine($"Package Flags: {summ.PackageFlags}");
                Console.WriteLine($"{summ.ImportCount} imported objects");
                Console.WriteLine($"{summ.ExportCount} exported objects");

                Console.WriteLine("Plugins Loaded:");
                foreach (var plugin in uaConverter.Plugins)
                {
                    Console.WriteLine($"\t{plugin.Key}");
                }
                Console.WriteLine();

                foreach(FObjectExport export in uaConverter.GetExportMap())
                {
                    Console.WriteLine($"{export.ObjectName.Name}:");
                    foreach(object prop in uaConverter.GetExportProperties(export))
                    {
                        Console.WriteLine($"\t{JsonConvert.SerializeObject(prop)}");
                    }
                }
            }
        }
    }
}
