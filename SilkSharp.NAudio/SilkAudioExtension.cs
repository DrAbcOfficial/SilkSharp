using NAudio.Wave;
using SilkSharp.Audio;

namespace SilkSharp.NAudio;

public static class SilkAudioExtension
{
    /// <summary>
    /// Get raw stream
    /// </summary>
    /// <param name="silk"></param>
    /// <returns></returns>
    public static RawSourceWaveStream GetRawStream(this SilkAudio silk)
    {
        using var ms = new MemoryStream(silk.GetS16LE().Data);
        WaveFormat wave = new(silk.Rate, 16, 1);
        var rawStream = new RawSourceWaveStream(ms, wave);
        return rawStream;
    }
    /// <summary>
    /// Get Wave steram
    /// </summary>
    /// <param name="silk">pcm</param>
    /// <returns>wave</returns>
    public static Stream GetWave(this SilkAudio silk)
    {
        return GetRawStream(silk);
    }
    /// <summary>
    /// Get Mp3 stream
    /// </summary>
    /// <param name="silk">pcm</param>
    /// <returns>mp3</returns>
    public static Stream GetMp3(this SilkAudio silk)
    {
        using var rawStream = GetRawStream(silk);
        var stream = new MemoryStream();
        MediaFoundationEncoder.EncodeToMp3(rawStream, stream);
        return stream;
    }
    /// <summary>
    /// Get Aac stream
    /// </summary>
    /// <param name="silk">pcm</param>
    /// <returns>aac</returns>
    public static Stream GetAac(this SilkAudio silk)
    {
        using var rawStream = GetRawStream(silk);
        var stream = new MemoryStream();
        MediaFoundationEncoder.EncodeToAac(rawStream, stream);
        return stream;
    }
    /// <summary>
    /// Get WaveOutEvent
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>waveout</returns>
    public static WaveOutEvent GetWaveOutEvent(this SilkAudio silk)
    {
        using var rawStream = GetRawStream(silk);
        var waveout = new WaveOutEvent();
        waveout.Init(rawStream);
        return waveout;
    }
}
