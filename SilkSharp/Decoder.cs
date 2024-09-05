using System.Runtime.InteropServices;
using static SilkSharp.Encoder;
using static System.Reflection.Metadata.BlobBuilder;

namespace SilkSharp;

public class Decoder : IDisposable
{
    public enum SilkDecodeResult
    {
        OK = 0,
        INPUT_NOT_FOUND,
        WRONG_HEADER,
        OUTPUT_NOT_FOUND,
        DECODE_ERROR,
        NULL_INPUT_STREAM,
        NULL_OUTPUT_STREAM
    }

    public int FS_API { get => _Fs_API; set { _Fs_API = value; } }

    private int _Fs_API = 24000;
    public async Task<SilkDecodeResult> DecodeAsync(Stream silkStream, Stream pcmStream)
    {
        return await new TaskFactory().StartNew(() =>
        {
            using MemoryStream ms = new();
            silkStream.CopyTo(ms);
            IntPtr outdata = 0;
            ulong size = 0;
            var ret = (SilkDecodeResult)NativeCodec.silk_decode(ms.ToArray(), (nuint)ms.Length, ref outdata, ref size, _Fs_API);
            byte[] data = new byte[size];
            Marshal.Copy(outdata, data, 0, data.Length);
            Marshal.FreeHGlobal(outdata);
            pcmStream.Write(data, 0, data.Length);
            return ret;
        });
    }

    public async Task<SilkDecodeResult> EncodeAsync(string pcmpath, string slkpath)
    {
        return await new TaskFactory().StartNew(() =>
        {
            return (SilkDecodeResult)NativeCodec.silk_decode_file(pcmpath, slkpath, _Fs_API);
        });
    }

    public void Dispose()
    {

    }
}
