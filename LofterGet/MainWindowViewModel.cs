using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LofterGet.Model;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml;
using Windows.Networking.Connectivity;

namespace LofterGet;

partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial GpuDriverBugEntry[] GpuBugs { get; set; }

    public GpuDriverBugEntry[] AllBugs { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> OsPlatforms { get; set; }

    [ObservableProperty]
    public partial string OneUpdate { get; set; }

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
            GpuDriverBugList json = null;
            await Task.Run(() =>
            {
                using var fs = File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}Resources/gpu_driver_bug_list.json");
                json = JsonSerializer.Deserialize(fs, SrcGenContext.Default.GpuDriverBugList);
                json?.entries = [.. json.entries.Reverse()];
            });
            AllBugs = json.entries;
            ObservableCollection<string> tmp = [.. AllBugs.Select(x => x.os?.type ?? "N/A").Distinct()];
            OsPlatforms = tmp;
            SelectedOsPlatform = "N/A";
            GpuBugs = json.entries;
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
}
