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
        var ms = new MemoryStream(s16le.Data);
        WaveFormat wave = new(s16le.Rate, 16, 1);
        var rawStream = new RawSourceWaveStream(ms, wave);
        return rawStream;
    }

    /// <summary>
    /// Get raw stream async
    /// </summary>
    /// <param name="s16le"></param>
    /// <returns></returns>
    public static async Task<RawSourceWaveStream> GetRawStreamAsync(this S16LEAudio s16le)
    {
        return await Task.Run(() => GetRawStream(s16le));
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
        MediaFoundationEncoder.EncodeToMp3(rawStream, stream, s16le.Rate);
        return stream;
    }

    /// <summary>
    /// Get Mp3 stream
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>mp3</returns>
    public static async Task<Stream> GetMp3Async(this S16LEAudio s16le)
    {
        return await Task.Run(() => GetMp3(s16le));
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
        MediaFoundationEncoder.EncodeToAac(rawStream, stream, s16le.Rate);
        return stream;
    }

    /// <summary>
    /// Get Aac stream
    /// </summary>
    /// <param name="s16le">pcm</param>
    /// <returns>aac</returns>
    public static async Task<Stream> GetAacAsync(this S16LEAudio s16le)
    {
        return await Task.Run(() => GetAac(s16le));
    }
}
