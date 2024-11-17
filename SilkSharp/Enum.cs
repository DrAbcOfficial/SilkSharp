namespace SilkSharp;

/// <summary>
/// Silk file complecity
/// </summary>
public enum SilkComplecity
{
    /// <summary>
    /// Low quality, min size
    /// </summary>
    Low = 0,
    /// <summary>
    /// Medium quality, ave size
    /// </summary>
    Medium = 1,
    /// <summary>
    /// High quality, large size
    /// </summary>
    High = 2
}
/// <summary>
/// Encode result, from native lib
/// </summary>
public enum SilkEncodeResult
{
    /// <summary>
    /// OK
    /// </summary>
    OK = 0,
    /// <summary>
    /// Can not found input file
    /// </summary>
    INPUT_NOT_FOUND,
    /// <summary>
    /// Can not found output file
    /// </summary>
    OUTPUT_NOT_FOUND,
    /// <summary>
    /// Can not create encoder
    /// </summary>
    CREATE_ECODER_ERROR,
    /// <summary>
    /// Can not init encoder
    /// </summary>
    RESET_ECODER_ERROR,
    /// <summary>
    /// FS_API sampling out of range, valid range 8000 - 48000
    /// </summary>
    SAMPLE_RATE_OUT_OF_RANGE,
    /// <summary>
    /// Input stream is null
    /// </summary>
    NULL_INPUT_STREAM,
    /// <summary>
    /// Output stream is null
    /// </summary>
    NULL_OUTPUT_STREAM
}
/// <summary>
/// Decode result, from native lib
/// </summary>
public enum SilkDecodeResult
{
    /// <summary>
    /// OK
    /// </summary>
    OK = 0,
    /// <summary>
    /// Can not found input file
    /// </summary>
    INPUT_NOT_FOUND,
    /// <summary>
    /// Header is not valid silk v3 file
    /// </summary>
    WRONG_HEADER,
    /// <summary>
    /// Cant not found output file
    /// </summary>
    OUTPUT_NOT_FOUND,
    /// <summary>
    /// Decoded with error
    /// </summary>
    DECODE_ERROR,
    /// <summary>
    /// Input stream is null
    /// </summary>
    NULL_INPUT_STREAM,
    /// <summary>
    /// Output stream is null
    /// </summary>
    NULL_OUTPUT_STREAM
}
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