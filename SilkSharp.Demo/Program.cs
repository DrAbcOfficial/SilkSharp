using SilkSharp.Codec;

/* Init a encoder 
初始化一个编码器 */
SilkEncoder encoder = new()
{
    FS_API = 16000,
    Tencent = true
};
Console.WriteLine("File Encode");
/* Encode file in sync method
   以同步的方式进行编码 */
encoder.Encode("./rasputin.pcm", "./rasputin.silk");
Console.WriteLine("Stream Encode");
/* Open file as a strem
   作为流打开一个文件 */
using FileStream fse = File.OpenRead("./rasputin.pcm");
/* Encode file in async method
   以异步的方式进行编码 */
var foe = encoder.EncodeAsync(fse);
/* Wait for async method done
   等待异步方法结束 */
foe.Wait();
/* Read async result as a stream
   将异步结果读取为流 */
using MemoryStream mse = new(foe.Result.Data);

/* Init a decoder 
   初始化一个解码器 */
SilkDecoder decoder = new()
{
    FS_API = 16000,
    Loss = 100
};
Console.WriteLine("File Decode");
/* Decode file in sync method
   以同步的方式进行解码 */
decoder.Decode("./badmoonrising.silk", "./badmoonrising.pcm");
Console.WriteLine("Stream Decode");
/* Open file as a strem
  作为流打开一个文件 */
using FileStream fsd = File.OpenRead("./badmoonrising.silk");
/* Decode file in async method
   以异步的方式进行解码 */
var fod = decoder.DecodeAsync(fsd);
/* Wait for async method done
   等待异步方法结束 */
fod.Wait();
/* Read async result as a stream
   将异步结果读取为流 */
using MemoryStream msd = new(fod.Result.Data);