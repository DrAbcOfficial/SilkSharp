namespace SilkSharp.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        Encoder encoder = new();
        Console.WriteLine("File Encode");
        encoder.Encode("./rasputin.pcm", "./rasputin.silk");
        Console.WriteLine("Stream Encode");
        using FileStream fse = File.OpenRead("./rasputin.pcm");
        var foe = encoder.EncodeAsync(fse);
        foe.Wait();
        using MemoryStream mse = new(foe.Result);

        Decoder decoder = new();
        Console.WriteLine("File Decode");
        decoder.Decode("./badmoonrising.silk", "./badmoonrising.pcm");
        Console.WriteLine("Stream Decode");
        using FileStream fsd = File.OpenRead("./badmoonrising.silk");
        var fod = decoder.DecodeAsync(fsd);
        fod.Wait();
        using MemoryStream msd = new(fod.Result);
    }
}
