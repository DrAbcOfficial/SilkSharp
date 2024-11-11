using SilkSharp.Exception;

namespace SilkSharp;

/// <summary>
/// Basic class, do not use directly
/// </summary>
public abstract class BaseCodec
{
    /// <summary>
    /// Memfree code
    /// </summary>
    public enum SilkFreeResult
    {
        /// <summary>
        /// OK
        /// </summary>
        OK = 0,
        /// <summary>
        /// Error
        /// </summary>
        ERROR = 1,
    }
    /// <summary>
    /// Free unmanaged memory
    /// </summary>
    /// <param name="buffer">unmanaged memory pointer</param>
    /// <exception cref="SilkMemFreeException">Free failed</exception>
    protected void Free(nint buffer)
    {
        SilkFreeResult ret = (SilkFreeResult)NativeCodec.silk_free(buffer);
        switch (ret)
        {
            case SilkFreeResult.ERROR: throw new SilkMemFreeException("Free memory failed!", ret);
        }
    }
}
