using CommunityToolkit.Mvvm.Input;
using LofterGet.Extensions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Windows.System;

namespace LofterGet;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MainWindowViewModel viewModel = new();

    private readonly HttpClient client = new();
    private readonly Channel<string> channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = true,
    });

    string outFolder = string.Empty;
    int totals = 0;

    public MainWindow()
    {
        this.InitializeComponent();
        viewModel.DQueue = DispatcherQueue;
        viewModel.WindowId = AppWindow.Id;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(spTitle);
        txtTitle.Text += $" {typeof(MainWindow).Assembly.GetName().Version} - {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}";

        try
        {
            this.Resize(1200, 800);
            this.Centre();
        }
        catch (Exception e)
        {
            ibError.Content = e.StackTrace;
            ibError.Visibility = Visibility.Visible;
            ibError.IsOpen = true;
        }

        try
        {
            tvMain.SelectedIndex = 1;
            InitData();
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        viewModel.OneDriveUpdate();
                        await viewModel.DisplayGpuDriverBugAsync();
                        viewModel.SelectedOsPlatform = "win";
                        viewModel.UpdateGpuBugs();
                        viewModel.UpdateUsb4Caps();
                    }
                    catch (Exception e)
                    {
                        //File.WriteAllText("D:/crash2.txt", e.StackTrace);
                    }
                });
            });
            Task.Run(LofterHandler);
        }
        catch (Exception e)
        {
            //File.WriteAllText("D:/crash.txt", e.StackTrace);
        }
    }

    [RelayCommand]
    public void SetDarkTheme()
    {
        (Content as FrameworkElement)!.RequestedTheme = ElementTheme.Dark;
        AppWindow.TitleBar.ButtonForegroundColor = Colors.White;
    }

    [RelayCommand]
    public void SetLightTheme()
    {
        (Content as FrameworkElement)!.RequestedTheme = ElementTheme.Light;
        AppWindow.TitleBar.ButtonForegroundColor = Colors.Black;
    }

    private void InitData()
    {
        txtReferer.Text = "https://queyangzheng82678.lofter.com/";
        txtUserAgent.Text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0";
        txtOutputPath.Text = "D:/tmp/M1876";
        outFolder = txtOutputPath.Text;
        client.DefaultRequestHeaders.Add("User-Agent", txtUserAgent.Text);
        client.DefaultRequestHeaders.Add("Referer", txtReferer.Text);
    }

    private async Task LofterHandler()
    {
        while (true)
        {
            var uri = await channel.Reader.ReadAsync();
            var resp = client.Send(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
            });
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                var file_name = uri[25..].Replace("/", "_");
                using var f = new FileStream($"{outFolder}/{file_name}",
                    FileMode.Create, FileAccess.Write);
                var bin = await resp.Content.ReadAsByteArrayAsync();
                f.Write(bin);
                DispatcherQueue.TryEnqueue(() =>
                {
                    pBar.Value += 100.0 / totals;
                    txtConsole.Text += $"{resp.StatusCode}: {file_name}\r\n";
                });
                await Task.Delay(500);
            }
        }
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        var uris = txtSources.Text.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        if (uris.Length > 0)
        {
            pBar.Value = 0;
            totals = uris.Length;
            foreach (var uri in uris)
            {
                channel.Writer.TryWrite(uri);
            }
            txtSources.Text = string.Empty;
        }
    }

    private void txtOutputPath_TextChanged(object sender, TextChangedEventArgs e)
    {
        outFolder = txtOutputPath.Text;
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        viewModel.UpdateGpuBugs();
    }
}
