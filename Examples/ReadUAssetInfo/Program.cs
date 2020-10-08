using System;
using System.IO;
using UnrealUAssetConverter;
using UnrealUAssetConverter.Unreal;

namespace ReadUAssetInfo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    using (UAssetConverter uaConverter = new UAssetConverter(args[0]))
                    {
                        FPackageFileSummary summ = uaConverter.GetSummary();
                        Console.WriteLine($"Package Size: {summ.TotalSize} bytes");
                        Console.WriteLine($"Package Flags: {summ.PackageFlags}");
                        Console.WriteLine($"{summ.ImportCount} imported objects");
                        Console.WriteLine($"{summ.ExportCount} exported objects");
                    }
                }
            }
        }
    }
}
