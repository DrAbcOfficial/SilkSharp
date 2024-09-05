# SilkSharp

SilkSharp is a simple binding for silk-codec https://github.com/foyoux/silk-codec

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