using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Graphics.Display;
using Windows.Graphics;

namespace LofterGet.Extensions;

public static class WindowExtensions
{
    public static void Resize(this Window window, int width, int height)
    {
        var displayInfo = DisplayInformationInterop.GetForWindow((nint)window.AppWindow.Id.Value);
        window.AppWindow.Resize(new SizeInt32((int)(width * displayInfo.RawPixelsPerViewPixel),
            (int)(height * displayInfo.RawPixelsPerViewPixel)));
    }

    public static void Centre(this Window window)
    {
        var displayInfo = DisplayInformationInterop.GetForWindow((nint)window.AppWindow.Id.Value);
        window.AppWindow.Move(new PointInt32(
            (int)((displayInfo.ScreenWidthInRawPixels - window.AppWindow.Size.Width) / 2),
            (int)((displayInfo.ScreenHeightInRawPixels - window.AppWindow.Size.Height) / 2)));
    }
}
