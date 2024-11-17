namespace SilkSharp.Audio;
/// <summary>
/// Audio base
/// </summary>
public abstract class BaseAudio
{
    /// <summary>
    /// Audio type
    /// </summary>
    public enum AudioType
    {
        /// <summary>
        /// Has no valid audio data
        /// </summary>
        INVALID,
        /// <summary>
        /// packed, 16 bit, little endian, raw data
        /// </summary>
        S16LE,
        /// <summary>
        /// SILKV3
        /// </summary>
        SILKV3
    }
    /// <summary>
    /// Audio rate
    /// </summary>
    public int Rate
    {
        get { return _rate; }
        set { _rate = value; }
    }
    /// <summary>
    /// Stored audio data
    /// </summary>
    public byte[] Data
    {
        get { return _data; }
        private set { _data = value; }
    }
    /// <summary>
    /// Stored audio type
    /// </summary>
    public AudioType Type
    {
        get { return _type; }
    }
    /// <summary>
    /// Uplink loss estimate, in percent (0-100); default: 0
    /// </summary>
    public int Loss { get => _loss; set => _loss = Math.Clamp(value, 0, 100); }

    /// <summary>
    /// protected rate
    /// </summary>
    protected int _rate = 0;
    /// <summary>
    /// protected data
    /// </summary>
    protected byte[] _data = [];
    /// <summary>
    /// protected type
    /// </summary>
    protected AudioType _type = AudioType.INVALID;
    /// <summary>
    /// protected loss
    /// </summary>
    protected int _loss = 0;

    /// <summary>
    /// Get audio duration
    /// </summary>
    /// <returns>audio duration, in (ms)</returns>
    public abstract long GetDuration();

    /// <summary>
    /// implicit cast into byte[]
    /// </summary>
    /// <param name="d">base audio</param>
    public static implicit operator byte[](BaseAudio d) => d._data;
}
