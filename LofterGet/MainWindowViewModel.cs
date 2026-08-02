using Aranyaka.Toolbox.Environment;
using Aranyaka.Toolbox.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LofterGet.Model;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.Storage.Pickers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml;
using Windows.Networking.Connectivity;
using static System.Net.Mime.MediaTypeNames;

namespace LofterGet;

partial class MainWindowViewModel : ObservableObject
{
    public DispatcherQueue DQueue { get; set; } = DispatcherQueue.GetForCurrentThread();
    public WindowId WindowId { get; set; } = new();

    [ObservableProperty]
    public partial string SysInfo { get; set; } = "Loading...Please Wait...";

    public void UpdateSystemInfo()
    {
        var info = string.Empty;
        try
        {
            info = SystemInfo.GetSystemInfo(WinVerLevel.LCUVer);
        }
        catch (Exception e)
        {
            info = $"ERROR: {e.CascadedMessages()}";
        }

        DQueue.TryEnqueue(() =>
        {
            SysInfo = info;
        });
    }

    [ObservableProperty]
    public partial GpuDriverBugEntry[] GpuBugs { get; set; } = [];

    public GpuDriverBugEntry[] AllBugs { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> OsPlatforms { get; set; } = [];

    [ObservableProperty]
    public partial string OneUpdate { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EffectiveTransferRate { get; set; } = "6400";

    [ObservableProperty]
    public partial string Channels { get; set; } = "2";

    [ObservableProperty]
    public partial string ChannelWidth { get; set; } = "64";

    [ObservableProperty]
    public partial string CalculatedBandwidth { get; set; } = "64";

    [ObservableProperty]
    public partial string SelectedOsPlatform { get; set; } = "N/A";

    [RelayCommand]
    public void CalcBandwidth()
    {
        if (double.TryParse(EffectiveTransferRate, out double transferRate) &&
           int.TryParse(Channels, out int channels) &&
           int.TryParse(ChannelWidth, out int channelWidth))
        {
            // Calculate bandwidth in Mbps
            double bandwidth = (transferRate * channels * channelWidth) / 8.0 / 1000;
            CalculatedBandwidth = $"{bandwidth:F2}";
        }
    }

    public void UpdateGpuBugs()
    {
        if (SelectedOsPlatform == "N/A")
        {
            GpuBugs = AllBugs;
        }
        else
        {
            GpuBugs = [.. AllBugs.Where(x => x.os?.type == SelectedOsPlatform)];
        }
    }

    [RelayCommand]
    public async Task DisplayGpuDriverBugAsync()
    {
        try
        {
            GpuDriverBugList? json = null;
            await Task.Run(() =>
            {
                using var fs = File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}Resources/gpu_driver_bug_list.json");
                json = JsonSerializer.Deserialize(fs, SrcGenContext.Default.GpuDriverBugList);
                json?.entries = [.. json.entries.Reverse()];
            });
            AllBugs = json?.entries ?? [];
            ObservableCollection<string> tmp = [.. AllBugs.Select(x => x.os?.type ?? "N/A").Distinct()];
            OsPlatforms = tmp;
            SelectedOsPlatform = "N/A";
            GpuBugs = json?.entries ?? [];
        }
        catch (Exception e)
        {
            File.WriteAllText("D:/crash3.txt", $"{AppDomain.CurrentDomain.BaseDirectory}{Environment.NewLine}{e.StackTrace}");
        }
    }

    [RelayCommand]
    public void OneDriveUpdate()
    {
        var path = @"C:\Users\a1240\AppData\Local\Microsoft\OneDrive\Update\update.xml";
        if (File.Exists(path))
        {
            using var xr = XmlReader.Create(path);
            var ret = true;
            while (ret == true)
            {
                ret = xr.ReadToFollowing("amd64binary");
                if (ret)
                {
                    OneUpdate = $"{xr["url"]}{Environment.NewLine}";
                    ret = xr.ReadToFollowing("arm64binary");
                    if (ret)
                    {
                        OneUpdate += xr["url"];
                    }
                }
            }
        }
    }

    [ObservableProperty]
    public partial string WwanDataClass { get; set; } = "N/A";

    [RelayCommand]
    public void RefreshWwanDataClass()
    {
        var cp = NetworkInformation.GetConnectionProfiles().Where(x => x.IsWwanConnectionProfile);
        if (cp.Any())
        {
            var wwan = cp.First();
            WwanDataClass = wwan.WwanConnectionProfileDetails.GetCurrentDataClass().ToString();
        }
    }

    public MainWindowViewModel()
    {
        RefreshWwanDataClass();
        CalcBandwidth();
    }

    [ObservableProperty]
    public partial string ChromiumVersion { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ChromiumDetectorEnabled { get; set; } = true;

    [RelayCommand]
    private async Task ChromeDetect()
    {
        var openPicker = new FileOpenPicker(WindowId)
        {
            FileTypeFilter = { ".exe", ".dll" },
        };

        var result = await openPicker.PickSingleFileAsync();
        if (result is not null)
        {
            ChromiumDetectorEnabled = false;
            _ = Task.Run(() =>
            {
                try
                {
                    var ver = ChromiumVersionDetector.DetectVersion(result.Path);
                    DQueue.TryEnqueue(() =>
                    {
                        ChromiumVersion = ver;
                    });
                }
                finally
                {
                    DQueue.TryEnqueue(() =>
                    {
                        ChromiumDetectorEnabled = true;
                    });
                }
            });
        }
    }

    [ObservableProperty]
    public partial Usb4DpCapabilities Usb4DpInLocalCaps { get; set; } = new();

    [ObservableProperty]
    public partial int DpInLocalCapabilities { get; set; } = 364946228;

    [RelayCommand]
    public void UpdateUsb4Caps()
    {
        Usb4DpInLocalCaps = new Usb4DpCapabilities
        {
            RawValue = DpInLocalCapabilities,
        };
    }

    [ObservableProperty]
    public partial ObservableCollection<string> VTFiles { get; set; } = [];

    public StringBuilder SbVTConsole { get; } = new(4096);

    [ObservableProperty]
    public partial string FfmpegPath { get; set; } = @"D:\tmp\@VideoDown\ffmpeg.exe";

    public string[] GpuDecodeVendor => ["Intel", "NVIDIA", "Media Foundation"];

    public string[] GpuEncodeVendor => ["Intel", "NVIDIA", "Media Foundation", "D3D12"];

    public string[] VideoDecodeFormat => ["AVC(H.264)", "HEVC(H.265)", "AV1", "VP9", "VC-1"];

    public string[] VideoEncodeFormat => ["AVC(H.264)", "HEVC(H.265)", "AV1", "VP9"];

    [ObservableProperty]
    public partial int DecodeVendorIndex { get; set; } = 1;

    [ObservableProperty]
    public partial int DecodeFormatIndex { get; set; }

    [ObservableProperty]
    public partial int EncodeVendorIndex { get; set; } = 1;

    [ObservableProperty]
    public partial int EncodeFormatIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool UseHardwareDecode { get; set; } = true;

    [ObservableProperty]
    public partial bool UseHardwareEncode { get; set; } = true;

    [ObservableProperty]
    public partial int TargetBitrate { get; set; } = 8;

    [ObservableProperty]
    public partial int MaxBitrate { get; set; } = 16;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStopTranscoding))]
    public partial bool IsTranscoding { get; set; }

    public bool CanStopTranscoding => !IsTranscoding;

    private List<string> VideosToTranscode = [];

    [RelayCommand]
    public async Task SelectVideosToTranscodeAsync()
    {
        var picker = new FileOpenPicker(WindowId);
        picker.FileTypeFilter.Add(".mkv");

        var files = await picker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            VideosToTranscode.Clear();
            VideosToTranscode.AddRange(files.Select(f => f.Path));
            VTFiles = new(files.Select(f => Path.GetFileName(f.Path)));
        }
    }

    [ObservableProperty]
    public partial string VTOutputPath { get; set; } = @"F:\tmp";

    [RelayCommand]
    public void StartVideoTranscode()
    {
        FfmpegPath = FfmpegPath.Trim('\"');
        IsTranscoding = true;

        Task.Run(() =>
        {
            foreach (var video in VideosToTranscode)
            {
                ClearVTConsole();
                var args = BuildFfmpegParams(video);
                AppendVTConsoleLine($"\"{FfmpegPath}\" {args}");

                VTProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = FfmpegPath,
                        Arguments = args,
                        UseShellExecute = false,
                        //RedirectStandardOutput = true,
                        //RedirectStandardError = true,
                    }
                };
                //VTProcess.OutputDataReceived += VT_OutputDataReceived;
                //VTProcess.ErrorDataReceived += VT_OutputDataReceived;

                try
                {
                    VTProcess.Start();
                    //VTProcess.BeginOutputReadLine();
                    VTProcess.WaitForExit();
                }
                catch (Exception e)
                {
                    AppendVTConsoleLine(e.CascadedMessages());
                    VTProcess.Kill();
                    DisposeVTProcess();
                    TranscodeStopped();
                    return;
                }

                if (CtsVideoTranscode?.IsCancellationRequested == true)
                {
                    VTProcess.Kill();
                    DisposeVTProcess();
                    TranscodeStopped();
                    return;
                }

                DisposeVTProcess();
            }

            TranscodeStopped();
        });
    }

    private void TranscodeStopped()
    {
        DQueue.TryEnqueue(() =>
        {
            IsTranscoding = false;
        });
    }

    private void DisposeVTProcess()
    {
        if (VTProcess is not null)
        {
            VTProcess.OutputDataReceived -= VT_OutputDataReceived;
            VTProcess.ErrorDataReceived -= VT_OutputDataReceived;
            VTProcess.Dispose();
            VTProcess = null;
        }
    }

    [RelayCommand]
    public void StopTranscode()
    {
        CtsVideoTranscode?.Cancel();
        VTProcess?.Kill();
    }

    private Process? VTProcess;

    private void VT_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            AppendVTConsole(e.Data);
    }

    private CancellationTokenSource? CtsVideoTranscode;

    private void AppendVTConsole(string text)
    {
        SbVTConsole.Append(text);
        DQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(SbVTConsole));
        });
    }

    private void ClearVTConsole()
    {
        SbVTConsole.Clear();
        DQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(SbVTConsole));
        });
    }

    private void AppendVTConsoleLine(string text)
    {
        SbVTConsole.AppendLine(text);
        DQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(SbVTConsole));
        });
    }

    private string BuildFfmpegParams(string inputPath)
    {
        var sb = new StringBuilder(512);

        if (UseHardwareDecode)
        {
            sb.Append("-c:v ");
            switch (DecodeFormatIndex)
            {
                case 0:
                    sb.Append("h264");
                    break;
                case 1:
                    sb.Append("h265");
                    break;
                case 2:
                    sb.Append("av1");
                    break;
                case 3:
                    sb.Append("vp9");
                    break;
                case 4:
                    sb.Append("vc1");
                    break;
                default:
                    break;
            }
            sb.Append('_');
            switch (DecodeVendorIndex)
            {
                case 0:
                    sb.Append("qsv");
                    break;
                case 1:
                    sb.Append("cuvid");
                    break;
                case 2:
                    sb.Append("mf");
                    break;
                default:
                    break;
            }
        }

        sb.Append($" -i \"{inputPath}\"");

        if (UseHardwareEncode)
        {
            sb.Append(" -c:v ");
            switch (EncodeFormatIndex)
            {
                case 0:
                    sb.Append("h264");
                    break;
                case 1:
                    sb.Append("h265");
                    break;
                case 2:
                    sb.Append("av1");
                    break;
                case 3:
                    sb.Append("vp9");
                    break;
                default:
                    break;
            }
            sb.Append('_');
            switch (EncodeVendorIndex)
            {
                case 0:
                    sb.Append("qsv");
                    break;
                case 1:
                    sb.Append("nvenc");
                    break;
                case 2:
                    sb.Append("mf");
                    break;
                case 3:
                    sb.Append("d3d12va");
                    break;
                default:
                    break;
            }
        }

        sb.Append($" -b:v {TargetBitrate}M -maxrate {MaxBitrate}M");
        sb.Append(" -bufsize 32M -rc-lookahead 24");
        sb.Append(" -c:a copy -c:s copy");

        sb.Append($" \"{VTOutputPath}\\{Path.GetFileName(inputPath)}\"");

        return sb.ToString();
    }
}
