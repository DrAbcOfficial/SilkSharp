using System.Runtime.InteropServices;

namespace SilkSharp;

public partial class Codec
{
    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int silk_decode(string silkFilePath, string outputFile, int Fs_API);

    [LibraryImport("silkcodec", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int silk_encode(string inputfile, string outputfile, int Fs_API,
        int rate, int packetlength, int complecity,
        int intencent, int loss, int dtx, int inbandfec, int Fs_maxInternal);

    public enum SilkDecodeResult
    {
        OK = 0,
        INPUT_NOT_FOUND,
        WRONG_HEADER,
        OUTPUT_NOT_FOUND,
        DECODE_ERROR
    }
    public enum SilkEncodeResult
    {
        OK = 0,
        INPUT_NOT_FOUND,
        OUTPUT_NOT_FOUND,
        CREATE_ECODER_ERROR,
        RESET_ECODER_ERROR,
        SAMPLE_RATE_OUT_OF_RANGE
    }

    public static async Task<SilkDecodeResult> SilkDecoderAsync(Stream silkStream, Stream pcmStream, int Fs_API = 0)
    {
        string input = Path.GetTempFileName();
        var fws = File.OpenWrite(input);
        silkStream.CopyTo(fws);
        fws.Close();
        fws.Dispose();
        string output = Path.GetTempFileName();
        Task<SilkDecodeResult> t = new TaskFactory().StartNew(() =>
        {
            return (SilkDecodeResult)silk_decode(input, output, Fs_API);
        });
        await t.ContinueWith((t) =>
        {
            var frs = File.OpenRead(output);
            frs.CopyTo(pcmStream);
            frs.Close();
            frs.Dispose();
            File.Delete(input);
            File.Delete(output);
        });
        return await t;
    }
    public static async Task<SilkDecodeResult> SilkDecoderAsync(string silkFile, string PCMFile, int Fs_API = 0)
    {
        string input = Path.GetFullPath(silkFile);
        string output = Path.GetFullPath(PCMFile);
        return await new TaskFactory().StartNew(() =>
        {
            return (SilkDecodeResult)silk_decode(input, output, Fs_API);
        });
    }
    public static async Task<SilkEncodeResult> SilkEncoderAsync(Stream pcmStream, Stream silkStream, int Fs_API = 24000,
        int rate = 25000, int packetlength = 20, int complecity = 0,
        bool intencent = false, int loss = 0, bool dtx = false, bool inbandfec = false, int Fs_maxInternal = 0)
    {
        string input = Path.GetTempFileName();
        var fws = File.OpenWrite(input);
        pcmStream.CopyTo(fws);
        fws.Close();
        fws.Dispose();
        string output = Path.GetTempFileName();
        Task<SilkEncodeResult> t = new TaskFactory().StartNew(() =>
        {
            return (SilkEncodeResult)silk_encode(input, output, Fs_API,
                rate, packetlength, complecity, intencent ? 1 : 0, loss, dtx ? 1 : 0, inbandfec ? 1 : 0, Fs_maxInternal);
        });
        await t.ContinueWith((t) =>
        {
            var frs = File.OpenRead(output);
            frs.CopyTo(silkStream);
            frs.Close();
            frs.Dispose();
            File.Delete(input);
            File.Delete(output);
        });
        return await t;
    }
    public static async Task<SilkEncodeResult> SilkEncoderAsync(string PCMFile, string silkFile, int Fs_API = 24000,
        int rate = 25000, int packetlength = 20, int complecity = 0,
        bool intencent = false, int loss = 0, bool dtx = false, bool inbandfec = false, int Fs_maxInternal = 0)
    {
        string input = Path.GetFullPath(PCMFile);
        string output = Path.GetFullPath(silkFile);

        return await new TaskFactory().StartNew(() =>
        {
            return (SilkEncodeResult)silk_encode(input, output, Fs_API,
            rate, packetlength, complecity, intencent ? 1 : 0, loss, dtx ? 1 : 0, inbandfec ? 1 : 0, Fs_maxInternal);
        });
    }
}
