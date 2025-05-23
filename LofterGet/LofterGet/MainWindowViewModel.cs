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
    public partial ObservableCollection<GpuDriverBugEntry> GpuBugs { get; set; }

    [RelayCommand]
    public void DisplayGpuDriverBug()
    {
        using var fs = File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}Resources/gpu_driver_bug_list.json");
        var json = JsonSerializer.Deserialize(fs, SrcGenContext.Default.GpuDriverBugList);
        GpuBugs = new ObservableCollection<GpuDriverBugEntry>(json.entries);
    }
}
