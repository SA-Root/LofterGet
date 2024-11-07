using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Concurrent;
using System.Net;

namespace LofterGet;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly HttpClient client = new();
    private readonly ConcurrentQueue<string> queue = new();
    string outFolder;
    int totals = 0;

    public MainWindow()
    {
        this.InitializeComponent();
        txtReferer.Text = "https://queyangzheng82678.lofter.com/";
        txtUserAgent.Text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0";
        txtOutputPath.Text = "D:/tmp/M1876";
        outFolder = txtOutputPath.Text;
        client.DefaultRequestHeaders.Add("User-Agent", txtUserAgent.Text);
        client.DefaultRequestHeaders.Add("Referer", txtReferer.Text);

        Task.Run(async () =>
        {
            while (true)
            {
                if (queue.TryDequeue(out var uri))
                {
                    if (uri != string.Empty)
                    {
                        var resp = client.Send(new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(uri),
                        });
                        if (resp.StatusCode == HttpStatusCode.OK)
                        {
                            var file_name = uri[25..].Replace("/", "_");
                            using var f = new FileStream($"{outFolder}/{file_name}", FileMode.Create, FileAccess.Write);
                            var bin = await resp.Content.ReadAsByteArrayAsync();
                            f.Write(bin);
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                pBar.Value += 100.0 / totals;
                                txtConsole.Text += $"{resp.StatusCode}: {file_name}\r\n";
                            });
                            Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            pBar.Value += 100.0 / totals;
                        });
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        });
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        var uris = txtSources.Text.Split('\r');
        if (uris.Length > 0)
        {
            pBar.Value = 0;
            totals = uris.Length;
            foreach (var uri in uris)
            {
                queue.Enqueue(uri);
            }
            txtSources.Text = string.Empty;
        }
    }

    private void txtOutputPath_TextChanged(object sender, TextChangedEventArgs e)
    {
        outFolder = txtOutputPath.Text;
    }
}
