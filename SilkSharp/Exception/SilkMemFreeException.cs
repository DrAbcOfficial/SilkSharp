namespace SilkSharp.Exception;
/// <summary>
/// Silk Mem Free Exception
/// </summary>
/// <param name="msg">Message</param>
/// <param name="r">Mem free result</param>
public class SilkMemFreeException(string msg, SilkFreeResult r) : System.Exception(msg)
{
    /// <summary>
    /// Memfree result
    /// </summary>
    public SilkFreeResult Result { get => _result; }
    private readonly SilkFreeResult _result = r;
}
