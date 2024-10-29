# SilkSharp

[![](https://img.shields.io/nuget/v/DrAbc.SilkSharp.svg)](https://www.nuget.org/packages/DrAbc.SilkSharp)
![NuGet Downloads](https://img.shields.io/nuget/dt/DrAbc.SilkSharp)


SilkSharp is a simple bundle of silk-codec https://github.com/foyoux/silk-codec

It converts audio in s16le format to and from silk v3 format.

Compatible with WeChat and QQ formats

Currently only `win-x86` `win-x64` `linux-x86` `linux-x64` `linux-arm` `linux-arm64` binary lib are available.

---

SilkSharp 是一个 silk-codec https://github.com/foyoux/silk-codec 的简单绑定

它实现s16le格式的音频与silk v3格式的相互转换

兼容微信和QQ格式

目前仅提供`win-x86` `win-x64` `linux-x86` `linux-x64` `linux-arm` `linux-arm64` `osx-arm64`版本的二进制文件

---

# 😇Usage:

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
decoder.DecodeAsync("./badmoonrising.silk", "./badmoonrising.pcm");
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

See the SilkSharp.Demo project for more information

更多请参阅SilkSharp.Demo项目


# 🏗Build

 
If you are using win-arm or other system, please build this project from source
> Not guaranteed it can be compiled on all architectural systems.

## Preparation

1. CMake 3.16 or above
2. Dotnet 8.0 SDK
3. git

> C# code may compile on lower versions of dotnet, but I do not guarantee that it will behave like dotnet 8.0.

## Steps
1. Compile the silkcodec with CMake
```
git clone https://github.com/DrAbcOfficial/SilkSharp
cd SilkSharp/silkcodec
mkdir build && cd build
cmake ..
make
```
2. Compile SilkSharp using dotnet
(following the above command)
```
cd ../SilkSharp
dotnet publish -c Release -r osx-arm64
```
3. Copy libsilkcodec.so (or silkcodec.dll/libsilkcodec.dylib) and SilkSharp.dll to your project.

---

如果你正在使用win-arm或其他架构系统，请自行从源代码构建本项目
>不保证可在所有架构系统上编译

## 准备

1. CMake 3.16或以上版本
2. Dotnet 8.0 SDK
3. git

> C#代码可能可在dotnet更低版本上编译，但我不保证其行为与dotnet 8.0一致

## 步骤
1. 使用CMake编译silkcodec
```
git clone https://github.com/DrAbcOfficial/SilkSharp
cd SilkSharp/silkcodec
mkdir build && cd build
cmake ..
make
```
2. 使用dotnet编译SilkSharp
(接上述命令)
```
cd ../SilkSharp
dotnet publish -c Release -r osx-arm64
```

3. 将`libsilkcodec.so`（或`silkcodec.dll`/`libsilkcodec.dylib`）及SilkSharp.dll拷贝至你的项目使用