using SilkSharp.Codec;

namespace SilkSharp.Audio;

/// <summary>
/// Silk audio
/// </summary>
public class SilkAudio : BaseAudio
{
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
    /// Enable DTX (0/1); default: 0
    /// </summary>
    public bool DTX { get => _dtx > 0; set => _dtx = value ? 1 : 0; }
    /// <summary>
    /// Enable inband FEC usage (0/1); default: 0
    /// </summary>
    public bool BandFEC { get => _inbandfec > 0; set => _inbandfec = value ? 1 : 0; }

    private int _packetlength = 20;
    private int _complecity = 0;
    private int _intencent = 0;
    private int _dtx = 0;
    private int _inbandfec = 0;
    internal SilkAudio(byte[] data)
    {
        _type = AudioType.SILKV3;
        _data = data;
    }
    /// <summary>
    /// Init a silk v3 audio
    /// </summary>
    /// <param name="data">silk file data</param>
    /// <param name="rate">Audio rate</param>
    /// <param name="loss">Audio loss</param>
    /// <param name="tencent">Is tencent format?</param>
    /// <param name="packetlength">Packet length</param>
    /// <param name="complecity">Complecity</param>
    /// <param name="dtx">DTX</param>
    /// <param name="bandfec">InBand FEC</param>
    public SilkAudio(byte[] data, int rate, int loss,
        bool tencent = false, int packetlength = 20, SilkComplecity complecity = SilkComplecity.High,
        bool dtx = false, bool bandfec = false)
    {
        _type = AudioType.SILKV3;
        _data = data;
        _packetlength = packetlength;
        Complecity = complecity;
        DTX = dtx;
        BandFEC = bandfec;
        Tencent = tencent;
        _rate = rate;
        _loss = loss;
    }
    /// <summary>
    /// Init a silk v3 audio
    /// </summary>
    /// <param name="filepath">silk file path</param>
    /// <param name="rate">Audio rate</param>
    /// <param name="loss">Audio loss</param>
    /// <param name="tencent">Is tencent format?</param>
    /// <param name="packetlength">Packet length</param>
    /// <param name="complecity">Complecity</param>
    /// <param name="dtx">DTX</param>
    /// <param name="bandfec">InBand FEC</param>
    public SilkAudio(string filepath, int rate, int loss,
        bool tencent = false, int packetlength = 20, SilkComplecity complecity = SilkComplecity.High,
        bool dtx = false, bool bandfec = false)
    {
        _type = AudioType.SILKV3;
        _packetlength = packetlength;
        Complecity = complecity;
        DTX = dtx;
        BandFEC = bandfec;
        Tencent = tencent;
        _rate = rate;
        _loss = loss;
        using FileStream fs = File.OpenRead(filepath);
        _data = new byte[fs.Length];
        fs.Read(_data, 0, _data.Length);
    }
    /// <summary>
    /// Get audio duration
    /// </summary>
    /// <returns>audio duration, in (ms)</returns>
    public override long GetDuration()
    {
        int frame = 0;
        for (int i = Tencent ? 10 : 9; i < _data.Length;)
        {
            //little endian
            int size = _data[i] + _data[i + 1] * 16;
            i += size + 2;
            frame++;
        }
        return frame * _packetlength;
    }
    /// <summary>
    /// Cast into S16LE
    /// </summary>
    /// <returns></returns>
    public S16LEAudio GetS16LE()
    {
        SilkDecoder decoder = new()
        {
            FS_API = _rate,
            Loss = _loss
        };
        return decoder.Decode(_data);
    }
}
