namespace SilkSharp.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        using FileStream fs = File.OpenRead("./rasputin.pcm");
        using MemoryStream ms = new();
        var result = Codec.SilkEncoderAsync(fs, ms);
        result.Wait();
        Console.WriteLine("Hello, World!");
    }
}
