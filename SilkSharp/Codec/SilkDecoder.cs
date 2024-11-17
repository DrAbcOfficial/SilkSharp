using SilkSharp.Audio;
using SilkSharp.Exception;
using System.Runtime.InteropServices;

namespace SilkSharp.Codec;

/// <summary>
/// Silk v3 speech to PCM decoder
/// </summary>
public class SilkDecoder : BaseCodec
{
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
    /// <param name="input">Silk v3 data</param>
    /// <returns>PCM</returns>
    /// <exception cref="SilkDecoderException"></exception>
    public S16LEAudio Decode(byte[] input)
    {
        nint outdata = 0;
        ulong size = 0;
        var ret = (SilkDecodeResult)NativeCodec.silk_decode(input, (ulong)input.Length, ref outdata, ref size, _Fs_API, _loss);
        if (ret != SilkDecodeResult.OK && outdata != 0)
            Free(outdata);
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
        Free(outdata);
        var s16le = new S16LEAudio(data)
        {
            Rate = FS_API,
            Loss = (int)Loss,
        };
        return s16le;
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="silkStream">Silk v3 stream</param>
    /// <returns>PCM</returns>
    /// <exception cref="SilkDecoderException"></exception>
    public S16LEAudio Decode(Stream silkStream)
    {
        using MemoryStream ms = new();
        silkStream.CopyTo(ms);
        return Decode(ms.ToArray());
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
    public async Task<S16LEAudio> DecodeAsync(Stream silkStream)
    {
        return await Task.Run(() => Decode(silkStream));
    }
    /// <summary>
    /// Decode Silk v3 speech into PCM
    /// </summary>
    /// <param name="silk">Silk v3 data</param>
    /// <returns>PCM data</returns>
    public async Task<S16LEAudio> DecodeAsync(byte[] silk)
    {
        return await Task.Run(() => Decode(silk));
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
