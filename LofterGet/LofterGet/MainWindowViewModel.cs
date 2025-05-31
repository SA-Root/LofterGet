using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LofterGet.Model;
using System.Text.Json;
using System.Xml;

namespace LofterGet;

partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial GpuDriverBugEntry[] GpuBugs { get; set; }

    public GpuDriverBugEntry[] AllBugs { get; set; }

    [ObservableProperty]
    public partial string[] OsPlatforms { get; set; }

    [ObservableProperty]
    public partial string OneUpdate { get; set; }

    [ObservableProperty]
    public partial string SelectedOsPlatform { get; set; } = "N/A";

    [RelayCommand]
    public async Task DisplayGpuDriverBug()
    {
        try
        {
            GpuDriverBugList json = null;
            await Task.Run(() =>
            {
                using var fs = File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}Resources/gpu_driver_bug_list.json");
                json = JsonSerializer.Deserialize(fs, SrcGenContext.Default.GpuDriverBugList);
                json.entries.Reverse();
            });
            AllBugs = json.entries;
            OsPlatforms = [.. AllBugs.Select(x => x.os?.type ?? "N/A").Distinct()];
            SelectedOsPlatform = "N/A";
            GpuBugs = json.entries;
        }
        catch (Exception e)
        {

        }
    }

    [RelayCommand]
    public void OneDriveUpdate()
    {
        var path = @"C:\Users\a1240\AppData\Local\Microsoft\OneDrive\Update\update.xml";
        if (File.Exists(path))
        {
            using var xr = XmlReader.Create(path);
            xr.ReadToFollowing("amd64binary");
            OneUpdate = $"{xr["url"]}{Environment.NewLine}";
            xr.ReadToFollowing("arm64binary");
            OneUpdate += xr["url"];
        }
    }
}
