﻿using SilkSharp.Audio;
using SilkSharp.Exception;
using System.Runtime.InteropServices;

namespace SilkSharp.Codec;

/// <summary>
///  s16le PCM to Silk v3 Encoder
/// </summary>
public class SilkEncoder : BaseCodec
{
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
    /// <param name="input">PCM data</param>
    /// <returns>Silk v3 audio</returns>
    /// <exception cref="SilkEncoderException"></exception>
    public SilkAudio Encode(byte[] input)
    {
        if (input.Length <= 0)
            throw new SilkEncoderException("Input data is null", SilkEncodeResult.NULL_INPUT_STREAM);
        nint outdata = 0;
        ulong size = 0;
        var ret = (SilkEncodeResult)NativeCodec.silk_encode(input, (ulong)input.Length, ref outdata, ref size,
            _Fs_API, _rate, _packetlength, _complecity, _intencent, _loss, _dtx, _inbandfec, _Fs_maxInternal);
        if (ret != SilkEncodeResult.OK && outdata != 0)
            Free(outdata);
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
        Free(outdata);
        var silk = new SilkAudio(data)
        {
            BandFEC = BandFEC,
            Complecity = Complecity,
            DTX = DTX,
            Loss = Loss,
            PacketLength = PacketLength,
            Rate = Rate,
            Tencent = Tencent,
        };
        return silk;
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcm">Input PCM sound stream</param>
    /// <returns>Silk v3 audio</returns>
    /// <exception cref="SilkEncoderException"></exception>
    public SilkAudio Encode(Stream pcm)
    {
        if (pcm == null || !pcm.CanRead)
            throw new SilkEncoderException("Input stream is null or not readable", SilkEncodeResult.NULL_INPUT_STREAM);
        pcm.Seek(0, SeekOrigin.Begin);
        byte[] bytes = new byte[pcm.Length];
        int bytesRead = pcm.Read(bytes);
        if (bytesRead != bytes.Length)
            throw new SilkEncoderException("Failed to read the entire stream", SilkEncodeResult.NULL_INPUT_STREAM);
        return Encode(bytes);
    }

    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcmpath">Input PCM sound file path</param>
    /// <param name="slkpath">Output Silk v3 sound file path</param>
    /// <exception cref="SilkEncoderException"></exception>
    public async Task EncodeAsync(string pcmpath, string slkpath)
    {
        await Task.Run(() => Encode(pcmpath, slkpath));
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcminfo">Input PCM sound file path</param>
    /// <param name="slkinfo">Output Silk v3 sound file path</param>
    /// <exception cref="SilkEncoderException"></exception>
    public async Task EncodeAsync(FileInfo pcminfo, FileInfo slkinfo)
    {
        await Task.Run(() => Encode(pcminfo, slkinfo));
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="pcm">Input PCM sound stream</param>
    /// <returns>Silk v3 file data</returns>
    /// <exception cref="SilkEncoderException"></exception>
    public Task<SilkAudio> EncodeAsync(Stream pcm)
    {
        return Task.Run(() => Encode(pcm));
    }
    /// <summary>
    /// Encode PCM sound into Silk v3
    /// </summary>
    /// <param name="data">Input PCM sound data</param>
    /// <returns>Silk v3 file data</returns>
    public Task<SilkAudio> EncodeAsync(byte[] data)
    {
        return Task.Run(() => Encode(data));
    }
}
