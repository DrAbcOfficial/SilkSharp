namespace SilkSharp.Exception;
public class SilkDecoderException(string msg, Decoder.SilkDecodeResult r) : System.Exception(msg)
{
    public Decoder.SilkDecodeResult Result { get=>_result;}
    private readonly Decoder.SilkDecodeResult _result = r;
}