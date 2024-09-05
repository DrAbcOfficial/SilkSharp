using SilkSharp.Exception;
using System.Runtime.InteropServices;

namespace SilkSharp;

public class Decoder
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
    /// <summary>
    /// Sampling rate of output signal in Hz; default: 24000
    /// </summary>
    public int FS_API { get => _Fs_API; set => _Fs_API = value; }
    /// <summary>
    /// Simulated packet loss percentage (0-100); default: 0
    /// </summary>
    public float Loss {get => _loss; set => _loss = Math.Clamp(value, 0.0f, 100.0f);}

    private int _Fs_API = 24000;
    private float _loss = 0.0f;
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="silkStream">Silk v3 stream</param>
    /// <returns>PCM data</returns>
    /// <exception cref="SilkDecoderException"></exception>
    public async Task<byte[]> DecodeAsync(Stream silkStream)
    {
        return await new TaskFactory().StartNew(() =>
        {
            using MemoryStream ms = new();
            silkStream.CopyTo(ms);
            IntPtr outdata = 0;
            ulong size = 0;
            var ret = (SilkDecodeResult)NativeCodec.silk_decode(ms.ToArray(), (nuint)ms.Length, ref outdata, ref size, _Fs_API, _loss);
            if(ret != SilkDecodeResult.OK && outdata != 0)
                Marshal.FreeHGlobal(outdata);
            switch(ret)
            {
                case SilkDecodeResult.NULL_INPUT_STREAM:
                case SilkDecodeResult.INPUT_NOT_FOUND: throw new SilkDecoderException("Input stream is null", ret);
                case SilkDecodeResult.WRONG_HEADER: throw new SilkDecoderException("Input file is not valid silk v3 speech", ret);
                case SilkDecodeResult.NULL_OUTPUT_STREAM:
                case SilkDecodeResult.OUTPUT_NOT_FOUND: throw new SilkDecoderException("Output stream is null", ret);
                case SilkDecodeResult.DECODE_ERROR: throw new SilkDecoderException("Internal decoder error", ret);
            }
            byte[] data = new byte[size];
            Marshal.Copy(outdata, data, 0, data.Length);
            Marshal.FreeHGlobal(outdata);
            return data;
        });
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="slkpath">Input Silk v3 file path</param>
    /// <param name="pcmpath">Output PCM file path</param>
    /// <exception cref="SilkDecoderException"></exception>
    public void Decode(string slkpath, string pcmpath)
    {
        DecodeAsync(new FileInfo(slkpath), new FileInfo(pcmpath));
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="slkpath">Input Silk v3 file path</param>
    /// <param name="pcmpath">Output PCM file path</param>
    /// <exception cref="SilkDecoderException"></exception>
    public async void DecodeAsync(FileInfo slkpath, FileInfo pcmpath)
    {
        await new TaskFactory().StartNew(() =>
        {
            var ret = (SilkDecodeResult)NativeCodec.silk_decode_file(slkpath.FullName, pcmpath.FullName, _Fs_API, _loss);
            switch(ret)
            {
                case SilkDecodeResult.NULL_INPUT_STREAM:
                case SilkDecodeResult.INPUT_NOT_FOUND: throw new SilkDecoderException("Input file is null", ret);
                case SilkDecodeResult.WRONG_HEADER: throw new SilkDecoderException("Input file is not valid silk v3 speech", ret);
                case SilkDecodeResult.NULL_OUTPUT_STREAM:
                case SilkDecodeResult.OUTPUT_NOT_FOUND: throw new SilkDecoderException("Output file is null", ret);
                case SilkDecodeResult.DECODE_ERROR: throw new SilkDecoderException("Internal decoder error", ret);
            };
        });
    }
}
