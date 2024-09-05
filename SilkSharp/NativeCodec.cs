using System.Runtime.InteropServices;

namespace SilkSharp;

internal static partial class NativeCodec
{
    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int silk_decode_file(string silkFilePath, string outputFile, int Fs_API, float loss);

    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int silk_encode_file(string inputfile, string outputfile, int Fs_API,
        int rate, int packetlength, int complecity,
        int intencent, int loss, int dtx, int inbandfec, int Fs_maxInternal);

    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int silk_decode([In] byte[] slk, ulong length, ref nint pcm, ref ulong outlen, int Fs_API, float loss);

    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int silk_encode([In] byte[] pcm, ulong length, ref nint slk, ref ulong outlen, int Fs_API,
        int rate, int packetlength, int complecity,
        int intencent, int loss, int dtx, int inbandfec, int Fs_maxInternal);
}
