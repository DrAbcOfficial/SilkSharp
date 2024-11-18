using NAudio.Wave;
using SilkSharp.Audio;

namespace SilkSharp.NAudio;

/// <summary>
/// S16LEAudio class extension
/// </summary>
public static class S16LEAudioExtension
{
    /// <summary>
    /// Get raw stream
    /// </summary>
    /// <param name="s16le"></param>
    /// <returns></returns>
    public static RawSourceWaveStream GetRawStream(this S16LEAudio s16le)
    {
        using var ms = new MemoryStream(s16le.Data);
        WaveFormat wave = new(s16le.Rate, 16, 1);
        var rawStream = new RawSourceWaveStream(ms, wave);
        return rawStream;
    }
    /// <summary>
    /// Get Wave steram
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>wave</returns>
    public static Stream GetWave(this S16LEAudio s16le)
    {
        return GetRawStream(s16le);
    }
    /// <summary>
    /// Get Mp3 stream
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>mp3</returns>
    public static Stream GetMp3(this S16LEAudio s16le)
    {
        using var rawStream = GetRawStream(s16le);
        var stream = new MemoryStream();
        MediaFoundationEncoder.EncodeToMp3(rawStream, stream);
        return stream;
    }
    /// <summary>
    /// Get Aac stream
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>aac</returns>
    public static Stream GetAac(this S16LEAudio s16le)
    {
        using var rawStream = GetRawStream(s16le);
        var stream = new MemoryStream();
        MediaFoundationEncoder.EncodeToAac(rawStream, stream);
        return stream;
    }
    /// <summary>
    /// Get WaveOutEvent
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>waveout</returns>
    public static WaveOutEvent GetWaveOutEvent(this S16LEAudio s16le)
    {
        using var rawStream = GetRawStream(s16le);
        var waveout = new WaveOutEvent();
        waveout.Init(rawStream);
        return waveout;
    }
}
