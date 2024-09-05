namespace SilkSharp.Exception;
public class SilkEncoderException(string msg,  Encoder.SilkEncodeResult r) : System.Exception(msg)
{
    public Encoder.SilkEncodeResult Result { get=>_result;}
    private readonly Encoder.SilkEncodeResult _result = r;
}