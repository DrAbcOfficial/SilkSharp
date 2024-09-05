using SilkSharp.SilkException;
using System.Runtime.InteropServices;

namespace SilkSharp;

/// <summary>
/// Silk v3 speech to PCM decoder
/// </summary>
public class Decoder
{
    /// <summary>
    /// Decode result, from native lib
    /// </summary>
    public enum SilkDecodeResult
    {
        /// <summary>
        /// OK
        /// </summary>
        OK = 0,
        /// <summary>
        /// Can not found input file
        /// </summary>
        INPUT_NOT_FOUND,
        /// <summary>
        /// Header is not valid silk v3 file
        /// </summary>
        WRONG_HEADER,
        /// <summary>
        /// Cant not found output file
        /// </summary>
        OUTPUT_NOT_FOUND,
        /// <summary>
        /// Decoded with error
        /// </summary>
        DECODE_ERROR,
        /// <summary>
        /// Input stream is null
        /// </summary>
        NULL_INPUT_STREAM,
        /// <summary>
        /// Output stream is null
        /// </summary>
        NULL_OUTPUT_STREAM
    }
    /// <summary>
    /// Sampling rate of output signal in Hz; default: 24000
    /// </summary>
    public int FS_API { get => _Fs_API; set => _Fs_API = value; }
    /// <summary>
    /// Simulated packet loss percentage (0-100); default: 0
    /// </summary>
    public float Loss { get => _loss; set => _loss = Math.Clamp(value, 0.0f, 100.0f); }

    private int _Fs_API = 24000;
    private float _loss = 0.0f;
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="silkStream">Silk v3 stream</param>
    /// <returns>PCM data</returns>
    /// <exception cref="SilkDecoderException"></exception>
    public byte[] Decode(Stream silkStream)
    {
        using MemoryStream ms = new();
        silkStream.CopyTo(ms);
        nint outdata = 0;
        ulong size = 0;
        var ret = (SilkDecodeResult)NativeCodec.silk_decode(ms.ToArray(), (ulong)ms.Length, ref outdata, ref size, _Fs_API, _loss);
        if (ret != SilkDecodeResult.OK && outdata != 0)
            Marshal.FreeHGlobal(outdata);
        switch (ret)
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
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="slkpath">Input Silk v3 file path</param>
    /// <param name="pcmpath">Output PCM file path</param>
    /// <exception cref="SilkDecoderException"></exception>
    public void Decode(string slkpath, string pcmpath)
    {
        Decode(new FileInfo(slkpath), new FileInfo(pcmpath));
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="slkpath">Input Silk v3 file path</param>
    /// <param name="pcmpath">Output PCM file path</param>
    /// <exception cref="SilkDecoderException"></exception>
    public void Decode(FileInfo slkpath, FileInfo pcmpath)
    {
        var ret = (SilkDecodeResult)NativeCodec.silk_decode_file(slkpath.FullName, pcmpath.FullName, _Fs_API, _loss);
        switch (ret)
        {
            case SilkDecodeResult.NULL_INPUT_STREAM:
            case SilkDecodeResult.INPUT_NOT_FOUND: throw new SilkDecoderException("Input file is null", ret);
            case SilkDecodeResult.WRONG_HEADER: throw new SilkDecoderException("Input file is not valid silk v3 speech", ret);
            case SilkDecodeResult.NULL_OUTPUT_STREAM:
            case SilkDecodeResult.OUTPUT_NOT_FOUND: throw new SilkDecoderException("Output file is null", ret);
            case SilkDecodeResult.DECODE_ERROR: throw new SilkDecoderException("Internal decoder error", ret);
        };
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="silkStream">Silk v3 stream</param>
    /// <returns>PCM data</returns>
    /// <exception cref="SilkDecoderException"></exception>
    public async Task<byte[]> DecodeAsync(Stream silkStream)
    {
        return await Task.Run(() => Decode(silkStream));
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="slkpath">Input Silk v3 file path</param>
    /// <param name="pcmpath">Output PCM file path</param>
    /// <exception cref="SilkDecoderException"></exception>
    public async Task DecodeAsync(string slkpath, string pcmpath)
    {
        await Task.Run(() => Decode(slkpath, pcmpath));
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="slkpath">Input Silk v3 file path</param>
    /// <param name="pcmpath">Output PCM file path</param>
    /// <exception cref="SilkDecoderException"></exception>
    public async Task DecodeAsync(FileInfo slkpath, FileInfo pcmpath)
    {
        await Task.Run(() => Decode(slkpath, pcmpath));
    }
}
