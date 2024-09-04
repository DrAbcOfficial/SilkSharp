# SilkSharp

SilkSharp is a simple binding for silk-codec https://github.com/kn007/silk-v3-decoder

usage:

```CSharp
//Stream
using FileStream fs = File.OpenRead("./rasputin.pcm");
using MemoryStream ms = new();
var result = await Codec.SilkEncoderAsync(fs, ms);

//File
var result = await Codec.SilkEncoderAsync("./rasputin.pcm", "./rasputin.slk");
```