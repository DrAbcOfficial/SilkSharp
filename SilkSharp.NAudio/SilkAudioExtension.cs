using NAudio.Wave;
using SilkSharp.Audio;

namespace SilkSharp.NAudio;

/// <summary>
/// SilkAudio class extension
/// </summary>
public static class SilkAudioExtension
{
    /// <summary>
    /// Get raw stream
    /// </summary>
    /// <param name="silk"></param>
    /// <returns></returns>
    public static RawSourceWaveStream GetRawStream(this SilkAudio silk)
    {
        var ms = new MemoryStream(silk.GetS16LE().Data);
        WaveFormat wave = new(silk.Rate, 16, 1);
        var rawStream = new RawSourceWaveStream(ms, wave);
        return rawStream;
    }
    /// <summary>
    /// Get raw stream async
    /// </summary>
    /// <param name="silk"></param>
    /// <returns></returns>
    public static async Task<RawSourceWaveStream> GetRawStreamAsync(this SilkAudio silk)
    {
        return await Task.Run(() => GetRawStream(silk));
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
        MediaFoundationEncoder.EncodeToMp3(rawStream, stream, silk.Rate);
        return stream;
    }
    /// <summary>
    /// Get Mp3 stream
    /// </summary>
    /// <param name="silk">pcm</param>
    /// <returns>mp3</returns>
    public static async Task<Stream> GetMp3Async(this SilkAudio silk)
    {
        return await Task.Run(() => GetMp3(silk));
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
        MediaFoundationEncoder.EncodeToAac(rawStream, stream, silk.Rate);
        return stream;
    }
    /// <summary>
    /// Get Aac stream
    /// </summary>
    /// <param name="silk">pcm</param>
    /// <returns>aac</returns>
    public static async Task<Stream> GetAacAsync(this SilkAudio silk)
    {
        return await Task.Run(() => GetAac(silk));
    }
}
