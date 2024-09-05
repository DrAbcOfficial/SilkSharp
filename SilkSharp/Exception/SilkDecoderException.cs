namespace SilkSharp.SilkException;
public class SilkDecoderException(string msg, Decoder.SilkDecodeResult r) : Exception(msg)
{
    public Decoder.SilkDecodeResult Result { get => _result; }
    private readonly Decoder.SilkDecodeResult _result = r;
}