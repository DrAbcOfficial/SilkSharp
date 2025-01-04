using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NAudio.Utils;
using NAudio.Wave;
using SilkSharp.Audio;
using SilkSharp.Codec;
using SilkSharp.NAudio;
using System.IO;
using System.Linq;
using System.Timers;

namespace SilkSharp.SilkPlayer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        _timer.Elapsed += (sender, args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                ProgressBar slider = this.FindControl<ProgressBar>("slider")!;
                Label time = this.FindControl<Label>("time")!;
                var timespan = _event.GetPositionTimeSpan();
                slider.Value = timespan.TotalMilliseconds;
                time.Content = timespan.ToString(@"mm\:ss");
            });
            if (_event.PlaybackState == PlaybackState.Stopped)
                _timer.Stop();
        };
        _event.PlaybackStopped += (sender, e) =>
        {
            _timer.Stop();
            Dispatcher.UIThread.Post(() =>
            {
                ProgressBar slider = this.FindControl<ProgressBar>("slider")!;
                Label time = this.FindControl<Label>("time")!;
                slider.Value = 0;
                time.Content = "00:00";
            });
        };
    }

    //#SILK_V3
    static readonly byte[] _silkheader = [0x23, 0x21, 0x53, 0x49, 0x4C, 0x4B, 0x5F, 0x56, 0x33];
    private readonly WaveOutEvent _event = new();
    private readonly Timer _timer = new(500);

    private async void OpenFileButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var filepicker = new FilePickerFileType("S16LE and SILK_V3")
        {
            Patterns = ["*.pcm", "*.silk"],
        };
        var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "🎵",
            AllowMultiple = false,
            FileTypeFilter = [filepicker],
        });
        if (file != null && file.Count > 0)
        {
            string? fp = file[0].TryGetLocalPath();
            if (fp != null)
            {
                using FileStream fs = File.OpenRead(fp);
                int firstbyte = fs.ReadByte();

                //not tencent, rewind
                if (firstbyte != 0x02)
                    fs.Seek(0, SeekOrigin.Begin);
                bool silkv3 = true;
                for (int i = 0; (i < _silkheader.Length) && silkv3; i++)
                {
                    int b = fs.ReadByte();
                    silkv3 = silkv3 && b == _silkheader[i];
                }

                NumericUpDown numericUpDown = this.FindControl<NumericUpDown>("rate")!;
                int rate = (int)numericUpDown.Value!;

                S16LEAudio s16LEAudio;
                //silkv3
                if (silkv3)
                {
                    SilkDecoder silkDecoder = new()
                    {
                        FS_API = rate
                    };
                    s16LEAudio = silkDecoder.Decode(fs);

                }
                else
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    s16LEAudio = new(data, rate, 0);
                }
                if (_event.PlaybackState != PlaybackState.Stopped)
                    _event.Stop();

                using StreamMediaFoundationReader smfr = new(s16LEAudio.GetMp3());
                _event.Init(smfr);

                long duration = s16LEAudio.GetDuration();
                ProgressBar slider = this.FindControl<ProgressBar>("slider")!;
                slider.Maximum = duration;

                FileInfo fi = new(fp);
                //Yes, I dont wanna write binding
                Label lb = this.FindControl<Label>("name")!;
                lb.Content = $"{fi.Name} ({(silkv3 ? "SILK_V3" : "S16LE")}) - {duration / 60000}:{duration % 60000 / 1000}   Rate: {s16LEAudio.Rate}  Loss: {s16LEAudio.Loss}";
            }
        }
    }
    private void PlayButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        switch (_event.PlaybackState)
        {
            case PlaybackState.Playing: _event.Pause(); break;
            default: _event.Play(); _timer.Start(); break;
        }
    }

    private void StopButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _event.Stop();
    }
}