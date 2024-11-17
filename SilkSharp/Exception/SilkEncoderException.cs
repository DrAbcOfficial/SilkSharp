namespace SilkSharp.Exception;
/// <summary>
/// Silk Encoder Exception
/// </summary>
/// <param name="msg">Message</param>
/// <param name="r">Encode result</param>
public class SilkEncoderException(string msg, SilkEncodeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Encode result
    /// </summary>
    public SilkEncodeResult Result { get => _result; }
    private readonly SilkEncodeResult _result = r;
}