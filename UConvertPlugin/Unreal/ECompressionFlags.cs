using System;

namespace UConvertPlugin.Unreal
{
    /// <summary>
    /// Flags controlling [de]compression
    /// </summary>
    [Flags]
    public enum ECompressionFlags : uint
    {
        /** No compression																*/
        COMPRESS_None = 0x00,
        /** Compress with ZLIB															*/
        COMPRESS_ZLIB = 0x01,
        /** Compress with GZIP															*/
        COMPRESS_GZIP = 0x02,
        /** Compress with user defined callbacks                                        */
        COMPRESS_Custom = 0x04,
        /** Prefer compression that compresses smaller (ONLY VALID FOR COMPRESSION)		*/
        COMPRESS_BiasMemory = 0x10,
        /** Prefer compression that compresses faster (ONLY VALID FOR COMPRESSION)		*/
        COMPRESS_BiasSpeed = 0x20,
        /* Override Platform Compression (use library Compression_Method even on platforms with platform specific compression */
        COMPRESS_OverridePlatform = 0x40
    }

    /// <summary>
    /// Compression Flag Masks
    /// </summary>
    public enum CompressionFlagMasks
    {
        /** mask out compression type flags */
        COMPRESSION_FLAGS_TYPE_MASK = 0x0F,

        /** mask out compression type */
        COMPRESSION_FLAGS_OPTIONS_MASK = 0xF0
    }
}
