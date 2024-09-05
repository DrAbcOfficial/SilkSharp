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
    internal static partial int silk_decode([In] byte[] slk, UIntPtr length, ref IntPtr pcm, ref ulong outlen, int Fs_API, float loss);

    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int silk_encode([In] byte[] pcm, UIntPtr length, ref IntPtr slk, ref ulong outlen, int Fs_API,
        int rate, int packetlength, int complecity,
        int intencent, int loss, int dtx, int inbandfec, int Fs_maxInternal);
}
