using LofterGet.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Channels;
using System.Xml;

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
    string outFolder;
    int totals = 0;

    public MainWindow()
    {
        this.InitializeComponent();

        InitData();
        viewModel.OneDriveUpdate();
        viewModel.DisplayGpuDriverBug();
        Task.Run(LofterHandler);
    }

    private void InitData()
    {
        txtReferer.Text = "https://queyangzheng82678.lofter.com/";
        txtUserAgent.Text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36 Edg/136.0.0.0";
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
                    svConsole.ScrollTo(0, svConsole.ExtentHeight);
                });
                Thread.Sleep(500);
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
                channel.Writer.WriteAsync(uri);
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
        if (viewModel.SelectedOsPlatform == "N/A")
        {
            viewModel.GpuBugs = viewModel.AllBugs;
        }
        else
        {
            viewModel.GpuBugs = [.. viewModel.AllBugs
                .Where(x => x.os?.type == viewModel.SelectedOsPlatform)];
        }
    }
}
