namespace SilkSharp.SilkException;
/// <summary>
/// Silk Decoder Exception
/// </summary>
/// <param name="msg">Except message</param>
/// <param name="r">Decode result</param>
public class SilkDecoderException(string msg, Decoder.SilkDecodeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Decode result
    /// </summary>
    public Decoder.SilkDecodeResult Result { get => _result; }
    private readonly Decoder.SilkDecodeResult _result = r;
}