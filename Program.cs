using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

namespace HHSAdvAvalonia
{
    internal static class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                // ★★★ ここが重要です ★★★
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}