using SilkSharp.Codec;

namespace SilkSharp.Audio;

/// <summary>
/// S16LE Audio
/// </summary>
public class S16LEAudio : BaseAudio
{
    internal S16LEAudio(byte[] data)
    {
        _type = AudioType.S16LE;
        _data = data;
    }
    /// <summary>
    /// Init a empty s16le audio
    /// </summary>
    /// <param name="data">pcm data</param>
    /// <param name="rate">Audio rate</param>
    /// <param name="loss">Audio loss</param>
    public S16LEAudio(byte[] data, int rate, int loss)
    {
        _type = AudioType.S16LE;
        _data = data;
        _rate = rate;
        _loss = loss;
    }
    /// <summary>
    /// Init a empty s16le audio
    /// </summary>
    /// <param name="path">pcm file path</param>
    /// <param name="rate">Audio rate</param>
    /// <param name="loss">Audio loss</param>
    public S16LEAudio(string path, int rate, int loss)
    {
        _type = AudioType.S16LE;
        _rate = rate;
        _loss = loss;
        using FileStream fs = File.OpenRead(path);
        _data = new byte[fs.Length];
        fs.Read(_data, 0, _data.Length);
    }
    /// <summary>
    /// Get audio duration
    /// </summary>
    /// <returns>audio duration, in (ms)</returns>
    public override long GetDuration()
    {
        return (_data.Length / 2) * 1000 / _rate;
    }
    /// <summary>
    /// Cast into Silk v3
    /// </summary>
    /// <returns></returns>
    public SilkAudio GetSilk(bool tencent)
    {
        SilkEncoder encoder = new()
        {
            FS_API = _rate,
            FS_MaxInternal = _rate,
            Tencent = tencent,
            Rate = _rate,
        };
        return encoder.Encode(_data);
    }
}
