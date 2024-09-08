# SilkSharp

[![](https://img.shields.io/nuget/v/DrAbc.SilkSharp.svg)](https://www.nuget.org/packages/DrAbc.SilkSharp)
![NuGet Downloads](https://img.shields.io/nuget/dt/DrAbc.SilkSharp)


SilkSharp is a simple bundle of silk-codec https://github.com/foyoux/silk-codec

It converts audio in s16le format to and from silk v3 format.

Compatible with WeChat and QQ formats

---

SilkSharp 是一个 silk-codec https://github.com/foyoux/silk-codec 的简单绑定

它实现s16le格式的音频与silk v3格式的相互转换

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