namespace SilkSharp.SilkException;
/// <summary>
/// Silk Encoder Exception
/// </summary>
/// <param name="msg">Message</param>
/// <param name="r">Encode result</param>
public class SilkEncoderException(string msg, Encoder.SilkEncodeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Encode result
    /// </summary>
    public Encoder.SilkEncodeResult Result { get => _result; }
    private readonly Encoder.SilkEncodeResult _result = r;
}