namespace SilkSharp.Demo;

internal class Program
{
    static async void Test()
    {
        Encoder encoder = new();
        Console.WriteLine("File Encode");
        encoder.EncodeAsync("./rasputin.pcm", "./rasputin.silk");
        Console.WriteLine("Stream Encode");
        using FileStream fse = File.OpenRead("./rasputin.pcm");
        using MemoryStream mse = new(await encoder.EncodeAsync(fse));
        
        Decoder decoder = new();
        Console.WriteLine("File Decode");
        encoder.EncodeAsync("./badmoonrising.silk", "./badmoonrising.pcm");
        Console.WriteLine("Stream Decode");
        using FileStream fsd = File.OpenRead("./badmoonrising.silk");
        using MemoryStream msd = new(await encoder.EncodeAsync(fsd));
    }
    static void Main(string[] args)
    {
        Test();
    }
}
