namespace SilkSharp.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        using Encoder encoder = new();
        Console.WriteLine("File Encode");
        var fe = encoder.EncodeAsync("./rasputin.pcm", "./rasputin.silk");
        fe.Wait();
        Console.WriteLine("Stream Encode");
        using FileStream fse = File.OpenRead("./rasputin.pcm");
        using MemoryStream mse = new();
        var se = encoder.EncodeAsync(fse, mse);
        se.Wait();
        using Decoder decoder = new();
        Console.WriteLine("File Decode");
        var fd = encoder.EncodeAsync("./badmoonarising.silk", "./badmoonarising.pcm");
        fd.Wait();
        Console.WriteLine("Stream Decode");
        using FileStream fsd = File.OpenRead("./badmoonarising.silk");
        using MemoryStream msd = new();
        var sd = encoder.EncodeAsync(fsd, msd);
        sd.Wait();
    }
}
