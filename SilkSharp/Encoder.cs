using SilkSharp.SilkException;
using System.IO;
using System.Runtime.InteropServices;

namespace SilkSharp;

public class Encoder
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
    public enum SilkComplecity
    {
        Low = 0,
        Medium = 1,
        Hight = 2
    }

    /// <summary>
    /// API sampling rate in Hz, default: 24000
    /// </summary>
    public int FS_API { get => _Fs_API; set => _Fs_API = value; }
    /// <summary>
    /// Target bitrate; default: 25000
    /// </summary>
    public int Rate { get => _rate; set => _rate = value; }
    /// <summary>
    /// Packet interval in ms, default: 20
    /// </summary>
    public int PacketLength { get => _packetlength; set => _packetlength = value; }
    /// <summary>
    /// Set complexity, 0: low, 1: medium, 2: high; default: 2
    /// </summary>
    public SilkComplecity Complecity { get => (SilkComplecity)_complecity; set => _complecity = (int)value; }
    /// <summary>
    /// Compatible with QQ/Wechat
    /// </summary>
    public bool Tencent { get => _intencent > 0; set => _intencent = value ? 1 : 0; }
    /// <summary>
    /// Uplink loss estimate, in percent (0-100); default: 0
    /// </summary>
    public int Loss { get => _loss; set => _loss = Math.Clamp(value, 0, 100); }
    /// <summary>
    /// Enable DTX (0/1); default: 0
    /// </summary>
    public bool DTX { get => _dtx > 0; set => _dtx = value ? 1 : 0; }
    /// <summary>
    /// Enable inband FEC usage (0/1); default: 0
    /// </summary>
    public bool BandFEC { get => _inbandfec > 0; set => _inbandfec = value ? 1 : 0; }
    /// <summary>
    /// Maximum internal sampling rate in Hz, default: 24000
    /// </summary>
    public int FS_MaxInternal { get => _Fs_maxInternal; set => _Fs_maxInternal = value; }

    private int _Fs_API = 24000;
    private int _rate = 25000;
    private int _packetlength = 20;
    private int _complecity = 2;
    private int _intencent = 0;
    private int _loss = 0;
    private int _dtx = 0;
    private int _inbandfec = 0;
    private int _Fs_maxInternal = 24000;

    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcmpath">Input PCM sound file path</param>
    /// <param name="slkpath">Output Silk v3 sound file path</param>
    /// <exception cref="SilkEncoderException"></exception>
    public void Encode(string pcmpath, string slkpath)
    {
        Encode(new FileInfo(pcmpath), new FileInfo(slkpath));
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcminfo">Input PCM sound file path</param>
    /// <param name="slkinfo">Output Silk v3 sound file path</param>
    /// <exception cref="SilkEncoderException"></exception>
    public void Encode(FileInfo pcminfo, FileInfo slkinfo)
    {
        var ret = (SilkEncodeResult)NativeCodec.silk_encode_file(pcminfo.FullName, slkinfo.FullName, _Fs_API,
                 _rate, _packetlength, _complecity, _intencent, _loss, _dtx, _inbandfec, _Fs_maxInternal);
        switch (ret)
        {
            case SilkEncodeResult.NULL_INPUT_STREAM:
            case SilkEncodeResult.INPUT_NOT_FOUND: throw new SilkEncoderException("Input file is null", ret);
            case SilkEncodeResult.NULL_OUTPUT_STREAM:
            case SilkEncodeResult.OUTPUT_NOT_FOUND: throw new SilkEncoderException("Output file is null", ret);
            case SilkEncodeResult.CREATE_ECODER_ERROR: throw new SilkEncoderException("Can not create Silk Encoder", ret);
            case SilkEncodeResult.RESET_ECODER_ERROR: throw new SilkEncoderException("Can not reset Silk Encoder", ret);
            case SilkEncodeResult.SAMPLE_RATE_OUT_OF_RANGE: throw new SilkEncoderException($"API Sampling rate = {_rate} out of range, valid range 8000 - 48000", ret);
        }
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcm">Input PCM sound stream</param>
    /// <returns>Silk v3 file data</returns>
    /// <exception cref="SilkEncoderException"></exception>
    public byte[] Encode(Stream pcm)
    {
        byte[] bytes = new byte[pcm.Length];
        pcm.Read(bytes);
        nint outdata = 0;
        ulong size = 0;
        var ret = (SilkEncodeResult)NativeCodec.silk_encode(bytes, (ulong)bytes.Length, ref outdata, ref size,
            _Fs_API, _rate, _packetlength, _complecity, _intencent, _loss, _dtx, _inbandfec, _Fs_maxInternal);
        if (ret != SilkEncodeResult.OK && outdata != 0)
            Marshal.FreeHGlobal(outdata);
        switch (ret)
        {
            case SilkEncodeResult.NULL_INPUT_STREAM:
            case SilkEncodeResult.INPUT_NOT_FOUND: throw new SilkEncoderException("Input stream is null", ret);
            case SilkEncodeResult.NULL_OUTPUT_STREAM:
            case SilkEncodeResult.OUTPUT_NOT_FOUND: throw new SilkEncoderException("Output stream is null", ret);
            case SilkEncodeResult.CREATE_ECODER_ERROR: throw new SilkEncoderException("Can not create Silk Encoder", ret);
            case SilkEncodeResult.RESET_ECODER_ERROR: throw new SilkEncoderException("Can not reset Silk Encoder", ret);
            case SilkEncodeResult.SAMPLE_RATE_OUT_OF_RANGE: throw new SilkEncoderException($"API Sampling rate = {_rate} out of range, valid range 8000 - 48000", ret);
        }
        byte[] data = new byte[size];
        Marshal.Copy(outdata, data, 0, data.Length);
        Marshal.FreeHGlobal(outdata);
        return data;
    }

    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcmpath">Input PCM sound file path</param>
    /// <param name="slkpath">Output Silk v3 sound file path</param>
    /// <exception cref="SilkEncoderException"></exception>
    public async Task EncodeAsync(string slkpath, string pcmpath)
    {
        await Task.Run(() => Encode(slkpath, pcmpath));
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcminfo">Input PCM sound file path</param>
    /// <param name="slkinfo">Output Silk v3 sound file path</param>
    /// <exception cref="SilkEncoderException"></exception>
    public async Task EncodeAsync(FileInfo slkpath, FileInfo pcmpath)
    {
        await Task.Run(() => Encode(slkpath, pcmpath));
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcm">Input PCM sound stream</param>
    /// <returns>Silk v3 file data</returns>
    /// <exception cref="SilkEncoderException"></exception>
    public async Task<byte[]> EncodeAsync(Stream pcm)
    {
        byte[] bytes = new byte[pcm.Length];
        await pcm.ReadAsync(bytes);
        nint outdata = 0;
        ulong size = 0;
        var ret = (SilkEncodeResult)NativeCodec.silk_encode(bytes, (ulong)bytes.Length, ref outdata, ref size,
            _Fs_API, _rate, _packetlength, _complecity, _intencent, _loss, _dtx, _inbandfec, _Fs_maxInternal);
        if (ret != SilkEncodeResult.OK && outdata != 0)
            Marshal.FreeHGlobal(outdata);
        switch (ret)
        {
            case SilkEncodeResult.NULL_INPUT_STREAM:
            case SilkEncodeResult.INPUT_NOT_FOUND: throw new SilkEncoderException("Input stream is null", ret);
            case SilkEncodeResult.NULL_OUTPUT_STREAM:
            case SilkEncodeResult.OUTPUT_NOT_FOUND: throw new SilkEncoderException("Output stream is null", ret);
            case SilkEncodeResult.CREATE_ECODER_ERROR: throw new SilkEncoderException("Can not create Silk Encoder", ret);
            case SilkEncodeResult.RESET_ECODER_ERROR: throw new SilkEncoderException("Can not reset Silk Encoder", ret);
            case SilkEncodeResult.SAMPLE_RATE_OUT_OF_RANGE: throw new SilkEncoderException($"API Sampling rate = {_rate} out of range, valid range 8000 - 48000", ret);
        }
        byte[] data = new byte[size];
        Marshal.Copy(outdata, data, 0, data.Length);
        Marshal.FreeHGlobal(outdata);
        return data;
    }
}
