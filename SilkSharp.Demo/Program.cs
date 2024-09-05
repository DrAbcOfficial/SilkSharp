namespace SilkSharp.Demo;

internal class Program
{
    static async void Test()
    {
        Encoder encoder = new();
        Console.WriteLine("File Encode");
        encoder.Encode("./rasputin.pcm", "./rasputin.silk");
        Console.WriteLine("Stream Encode");
        using FileStream fse = File.OpenRead("./rasputin.pcm");
        using MemoryStream mse = new(await encoder.EncodeAsync(fse));
        
        Decoder decoder = new();
        Console.WriteLine("File Decode");
        decoder.Decode("./badmoonrising.silk", "./badmoonrising.pcm");
        Console.WriteLine("Stream Decode");
        using FileStream fsd = File.OpenRead("./badmoonrising.silk");
        using MemoryStream msd = new(await decoder.DecodeAsync(fsd));
    }
    static void Main(string[] args)
    {
        Test();
    }
}
