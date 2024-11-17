using SilkSharp.Codec;

namespace SilkSharp.Exception;
/// <summary>
/// Silk Encoder Exception
/// </summary>
/// <param name="msg">Message</param>
/// <param name="r">Encode result</param>
public class SilkEncoderException(string msg, SilkEncoder.SilkEncodeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Encode result
    /// </summary>
    public SilkEncoder.SilkEncodeResult Result { get => _result; }
    private readonly SilkEncoder.SilkEncodeResult _result = r;
}