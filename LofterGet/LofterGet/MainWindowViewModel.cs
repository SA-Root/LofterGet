using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LofterGet.Model;

namespace LofterGet;

partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial GpuDriverBugEntry[] GpuBugs { get; set; }

    public GpuDriverBugEntry[] AllBugs { get; set; }

    [ObservableProperty]
    public partial string[] OsPlatforms { get; set; }

    [ObservableProperty]
    public partial string SelectedOsPlatform { get; set; } = "N/A";

    [RelayCommand]
    public void DisplayGpuDriverBug()
    {
        try
        {
            using var fs = File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}Resources/gpu_driver_bug_list.json");
            var json = JsonSerializer.Deserialize(fs, SrcGenContext.Default.GpuDriverBugList);
            json.entries.Reverse();
            AllBugs = json.entries;
            OsPlatforms = [.. AllBugs.Select(x => x.os?.type ?? "N/A").Distinct()];
            SelectedOsPlatform = "N/A";
            GpuBugs = json.entries;
        }
        catch (Exception e)
        {

        }
    }
}
