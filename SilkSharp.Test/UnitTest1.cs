using SilkSharp.Codec;

namespace SilkSharp.Test;

public class Tests
{
    private SilkDecoder decoder;
    private SilkEncoder encoder;

    private readonly byte[] _free_decode_pattern = [
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x02, 0x00,
            0x11, 0x00, 0x3B, 0x00,
            0x7A, 0x00, 0xB4, 0x00
        ];
    private readonly int _free_decode_size = 314880;

    private readonly byte[] _push_encode_pattern = [
            0x02, 0x23, 0x21, 0x53,
            0x49, 0x4C, 0x4B, 0x5F,
            0x56, 0x33, 0x0B, 0x00,
            0xA7, 0x2B, 0x74, 0xF7
        ];
    private readonly int _push_encode_size = 41533;

    private static bool PatternTest(byte[] pattern, byte[] data)
    {
        if (pattern.Length > data.Length)
            return false;
        bool t = true;
        for (int i = 0; i < pattern.Length; i++)
        {
            if (!t)
                return false;
            t = t && pattern[i] == data[i];
        }
        return t;
    }
    private static string BuildString(string de, string method, int size, int max, byte[] data)
    {
        string s = $"{de}({method}) not get properly result. size: {size}({max})\n";
        s += "Data(16):\n";
        for (int i = 0; i < Math.Min(16, data.Length); i++)
        {
            s += string.Format("0x{0:X2} ", data[i]);
            if ((i + 1) % 4 == 0)
                s += "\n";
        }
        return s;
    }
    [SetUp]
    public void Setup()
    {
        bool a = File.Exists("assets/free.silk");
        bool b = File.Exists("assets/push.pcm");
        Assert.That(a && b, Is.True, "Test assets are not exist!");

        decoder = new SilkDecoder()
        {
            FS_API = 24000,
            Loss = 0
        };
        encoder = new SilkEncoder()
        {
            FS_API = 24000,
            FS_MaxInternal = 24000,
            Rate = 24000,
            Loss = 0,
            Tencent = true
        };
    }

    [Test]
    public void DecodeTest_File_Sync()
    {
        decoder.Decode("assets/free.silk", "assets/free_file_sync.pcm");
        using var test = File.OpenRead("assets/free_file_sync.pcm");
        byte[] result = new byte[test.Length];
        test.Read(result);
        int size = result.Length;
        Assert.That(PatternTest(_free_decode_pattern, result) && size == _free_decode_size, Is.True,
            BuildString("Decode", "FileSync", size, _free_decode_size, result));
    }
    [Test]
    public void DecodeTest_Stream_Sync()
    {
        using var fs = File.OpenRead("assets/free.silk");
        var result = decoder.Decode(fs);
        int size = result.Data.Length;
        Assert.That(PatternTest(_free_decode_pattern, result.Data) && size == _free_decode_size, Is.True,
            BuildString("Decode", "StreamSync", size, _free_decode_size, result.Data));
    }
    [Test]
    public void DecodeTest_File_Async()
    {
        var task = decoder.DecodeAsync("assets/free.silk", "assets/free_file_async.pcm");
        task.Wait();
        using var test = File.OpenRead("assets/free_file_async.pcm");
        byte[] result = new byte[test.Length];
        test.Read(result);
        int size = result.Length;
        Assert.That(PatternTest(_free_decode_pattern, result) && size == _free_decode_size, Is.True,
            BuildString("Decode", "FileAsync", size, _free_decode_size, result));
    }
    [Test]
    public void DecodeTest_Stream_Async()
    {
        using var fs = File.OpenRead("assets/free.silk");
        var task = decoder.DecodeAsync(fs);
        task.Wait();
        var result = task.Result;
        int size = result.Data.Length;
        Assert.That(PatternTest(_free_decode_pattern, result.Data) && size == _free_decode_size, Is.True,
             BuildString("Decode", "StreamAsync", size, _free_decode_size, result.Data));
    }
    [Test]
    public void EncodeTest_File_Sync()
    {
        encoder.Encode("assets/push.pcm", "assets/push_file_sync.silk");
        using var test = File.OpenRead("assets/push_file_sync.silk");
        byte[] result = new byte[test.Length];
        test.Read(result);
        int size = result.Length;
        Assert.That(PatternTest(_push_encode_pattern, result) && size == _push_encode_size, Is.True,
             BuildString("Encode", "FileSync", size, _push_encode_size, result));
    }
    [Test]
    public void EncodeTest_Stream_Sync()
    {
        using var fs = File.OpenRead("assets/push.pcm");
        var result = encoder.Encode(fs);
        int size = result.Data.Length;
        Assert.That(PatternTest(_push_encode_pattern, result.Data) && size == _push_encode_size, Is.True,
            BuildString("Encode", "StreamSync", size, _push_encode_size, result.Data));
    }
    [Test]
    public void EncodeTest_File_Async()
    {
        var task = encoder.EncodeAsync("assets/push.pcm", "assets/push_file_async.silk");
        task.Wait();
        using var test = File.OpenRead("assets/push_file_async.silk");
        byte[] result = new byte[test.Length];
        test.Read(result);
        int size = result.Length;
        Assert.That(PatternTest(_push_encode_pattern, result) && size == _push_encode_size, Is.True,
            BuildString("Encode", "FileAsync", size, _push_encode_size, result));
    }
    [Test]
    public void EncodeTest_Stream_Async()
    {
        using var fs = File.OpenRead("assets/push.pcm");
        var task = encoder.EncodeAsync(fs);
        task.Wait();
        var result = task.Result;
        int size = result.Data.Length;
        Assert.That(PatternTest(_push_encode_pattern, result.Data) && size == _push_encode_size, Is.True,
            BuildString("Encode", "StreamAsync", size, _push_encode_size, result.Data));
    }

    const long _encode_time = 15140;
    [Test]
    public void SilkAudio_Format()
    {
        using var fs = File.OpenRead("assets/push.pcm");
        var result = encoder.Encode(fs);
        long time = result.GetDuration();
        Assert.That(time, Is.EqualTo(_encode_time), $"Silk audio format GetDuration() with error! {time}({_encode_time})");

        var pcm = result.GetS16LE();
        long ptime = pcm.GetDuration();
        Assert.That(time, Is.EqualTo(_encode_time), $"Silk audio format GetS16LE() with error! {time}({_encode_time})");
        Assert.That(time, Is.EqualTo(ptime), $"Silk S16LE audio format duration not equal! {time}-{ptime}");
    }

    const long _decode_time = 6560;
    [Test]
    public void S16LEAudio_Format()
    {
        using var fs = File.OpenRead("assets/free.silk");
        var result = decoder.Decode(fs);
        long time = result.GetDuration();
        Assert.That(time, Is.EqualTo(_decode_time), $"S16LE audio format GetDuration() with error! {time}({_decode_time})");

        var silk = result.GetSilk(true);
        long stime = silk.GetDuration();
        Assert.That(time, Is.EqualTo(_decode_time), $"S16LE audio format GetSilk() with error! {time}({_decode_time})");
        Assert.That(time, Is.EqualTo(stime), $"S16LE Silk audio format duration not equal! {time}-{stime}");
    }
}