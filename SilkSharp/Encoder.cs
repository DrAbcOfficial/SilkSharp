using System.Runtime.InteropServices;

namespace SilkSharp;

public class Encoder : IDisposable
{
    public enum SilkEncodeResult
    {
        OK = 0,
        INPUT_NOT_FOUND,
        OUTPUT_NOT_FOUND,
        CREATE_ECODER_ERROR,
        RESET_ECODER_ERROR,
        SAMPLE_RATE_OUT_OF_RANGE,
        NULL_INPUT_STREAM,
        NULL_OUTPUT_STREAM
    }

    public int FS_API { get => _Fs_API; set { _Fs_API = value; } }
    public int Rate { get => _rate; set { _rate = value; } }
    public int PacketLength { get => _packetlength; set { _packetlength = value; } }
    public bool Complecity { get => _complecity > 0; set { _complecity = value ? 2 : 0; } }
    public bool Tencent { get => _intencent > 0; set { _intencent = value ? 1 : 0; } }
    public int Loss { get => _loss; set { _loss = value; } }
    public bool DTX { get => _dtx > 0; set { _dtx = value ? 1 : 0; } }
    public bool BandFEC { get => _inbandfec > 0; set { _inbandfec = value ? 1 : 0; } }
    public int FS_MaxInternal { get => _Fs_maxInternal; set { _Fs_maxInternal = value; } }

    private int _Fs_API = 24000;
    private int _rate = 25000;
    private int _packetlength = 20;
    private int _complecity = 0;
    private int _intencent = 0;
    private int _loss = 0;
    private int _dtx = 0;
    private int _inbandfec = 0;
    private int _Fs_maxInternal = 0;

    public async Task<SilkEncodeResult> EncodeAsync(string pcmpath, string slkpath)
    {
        return await new TaskFactory().StartNew(() =>
        {
            return (SilkEncodeResult)NativeCodec.silk_encode_file(pcmpath, slkpath, _Fs_API,
            _rate, _packetlength, _complecity, _intencent, _loss, _dtx, _inbandfec, _Fs_maxInternal);
        });
    }
    public async Task<SilkEncodeResult> EncodeAsync(Stream pcm, Stream slk)
    {
        return await new TaskFactory().StartNew(() =>
        {
            using MemoryStream ms = new();
            pcm.CopyTo(ms);
            IntPtr outdata = 0;
            ulong size = 0;
            var ret = (SilkEncodeResult)NativeCodec.silk_encode(ms.ToArray(), (nuint)ms.Length, ref outdata, ref size, 
            _Fs_API, _rate, _packetlength, _complecity, _intencent, _loss, _dtx, _inbandfec, _Fs_maxInternal);
            byte[] data = new byte[size];
            Marshal.Copy(outdata, data, 0, data.Length);
            Marshal.FreeHGlobal(outdata);

            slk.Write(data, 0, data.Length);
            ms.Close();
            ms.Dispose();
            return ret;
        });
    }

    public void Dispose()
    {

    }
}
