using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Net;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LofterGet
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        HttpClient client = new();
        public MainWindow()
        {
            this.InitializeComponent();
            txtReferer.Text = "https://queyangzheng82678.lofter.com/";
            txtUserAgent.Text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0";
            txtOutputPath.Text = "D:/tmp/M1876";
            client.DefaultRequestHeaders.Add("User-Agent", txtUserAgent.Text);
            client.DefaultRequestHeaders.Add("Referer", txtReferer.Text);

        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            var uris = txtSources.Text.Split('\r');
            foreach (var uri in uris)
            {
                var resp = client.Send(new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                });
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var file_name = uri[25..].Replace("/", "_");
                    using var f = new FileStream($"D:/tmp/{file_name}", FileMode.Create, FileAccess.Write);
                    var bin = await resp.Content.ReadAsByteArrayAsync();
                    f.Write(bin);
                    txtConsole.Text += $"{resp.StatusCode}: {file_name}\r\n";
                }
            }
        }
    }
}
