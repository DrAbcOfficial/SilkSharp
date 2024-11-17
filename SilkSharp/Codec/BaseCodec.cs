using SilkSharp.Exception;

namespace SilkSharp.Codec;

/// <summary>
/// Basic class, do not use directly
/// </summary>
public abstract class BaseCodec
{
    /// <summary>
    /// Free unmanaged memory
    /// </summary>
    /// <param name="buffer">unmanaged memory pointer</param>
    /// <exception cref="SilkMemFreeException">Free failed</exception>
    protected static void Free(nint buffer)
    {
        SilkFreeResult ret = (SilkFreeResult)NativeCodec.silk_free(buffer);
        switch (ret)
        {
            case SilkFreeResult.ERROR: throw new SilkMemFreeException("Free memory failed!", ret);
        }
    }
}
