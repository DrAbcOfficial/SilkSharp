namespace SilkSharp.SilkException;
public class SilkEncoderException(string msg, Encoder.SilkEncodeResult r) : Exception(msg)
{
    public Encoder.SilkEncodeResult Result { get => _result; }
    private readonly Encoder.SilkEncodeResult _result = r;
}