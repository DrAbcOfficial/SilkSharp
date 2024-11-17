using SilkSharp.Codec;

namespace SilkSharp.Exception;
/// <summary>
/// Silk Decoder Exception
/// </summary>
/// <param name="msg">Except message</param>
/// <param name="r">Decode result</param>
public class SilkDecoderException(string msg, SilkDecoder.SilkDecodeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Decode result
    /// </summary>
    public SilkDecoder.SilkDecodeResult Result { get => _result; }
    private readonly SilkDecoder.SilkDecodeResult _result = r;
}