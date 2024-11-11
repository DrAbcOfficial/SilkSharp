namespace SilkSharp.Exception;

public class SilkMemFreeException(string msg, BaseCodec.SilkFreeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Memfree result
    /// </summary>
    public BaseCodec.SilkFreeResult Result { get => _result; }
    private readonly BaseCodec.SilkFreeResult _result = r;
}
