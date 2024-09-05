# SilkSharp

[![](https://img.shields.io/nuget/v/DrAbc.SilkSharp.svg)](https://www.nuget.org/packages/DrAbc.SilkSharp)
![NuGet Downloads](https://img.shields.io/nuget/dt/DrAbc.SilkSharp)


SilkSharp is a simple binding for silk-codec https://github.com/foyoux/silk-codec

It can convert audio in s16le format to silk v3 format

Compatible wechat and QQ

---

SilkSharp 是一个 silk-codec https://github.com/foyoux/silk-codec 的简单绑定

它可以将s16le格式的音频转换为silk v3格式

兼容微信和QQ格式

---

usage:

```CSharp
//Encoding
Encoder encoder = new();
//File
encoder.EncodeAsync("./rasputin.pcm", "./rasputin.silk");
//Stream
using FileStream fse = File.OpenRead("./rasputin.pcm");
using MemoryStream mse = new(await encoder.EncodeAsync(fse));

//Decoding
Decoder decoder = new();
//File
encoder.EncodeAsync("./badmoonrising.silk", "./badmoonrising.pcm");
//Stream
using FileStream fsd = File.OpenRead("./badmoonrising.silk");
using MemoryStream msd = new(await encoder.EncodeAsync(fsd));
```

E.g: Convert other audio to pcm (NAudio)

```CSharp
using NAudio.Wave;

using Mp3FileReader reader = new("input.mp3");
WaveFormat pcmFormat = new(16000, 16, 1);
using WaveFormatConversionStream conversionStream = new(pcmFormat, reader);
Encoder encoder = new()
{
    //Same with your music
    Rate = 16000,
    FS_API = 16000,
    //QQ and Wechat Compatibility
    Tencent = true
};
using MemoryStream silk = new(await encoder.EncodeAsync(conversionStream));
```