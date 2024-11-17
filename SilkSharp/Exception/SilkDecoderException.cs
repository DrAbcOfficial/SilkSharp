namespace SilkSharp.Exception;
/// <summary>
/// Silk Decoder Exception
/// </summary>
/// <param name="msg">Except message</param>
/// <param name="r">Decode result</param>
public class SilkDecoderException(string msg, SilkDecodeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Decode result
    /// </summary>
    public SilkDecodeResult Result { get => _result; }
    private readonly SilkDecodeResult _result = r;
}