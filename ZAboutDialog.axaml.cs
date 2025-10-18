using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Reflection;

namespace HHSAdvAvalonia
{
    public partial class ZAboutDialog : Window
    {
        public class AboutContents
        {
            public string IconImageFile =>
                System.IO.Path.Combine(ZSystem.Instance.dataFolder, "icon.png");

            public string AboutText = string.Empty;

            public AboutContents()
            {
                var appVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
                AboutText = $@"High High School Adventure
{appVersion}

PalmOS version: hiro © 2002-2004
Android version: hiro © 2011-2025
Web version: hiro © 2012-2024
M5 version: hiro © 2023-2025
Qt version: hiro © 2024-2025
PicoCalc version: hiro © 2025
SDL version: hiro © 2025
Windows version: hiro © 2025
AvaloniaUI version: hiro © 2025

- Project ZOBPlus -
Hayami <hayami@zob.jp>, Exit <exit@zob.jp>, ezumi <ezumi@zob.jp>
Ogu <ogu@zob.jp>, neopara <neopara@zob.jp>, hiro <hiro@zob.jp>

--- Original Staff ---
Directed By HIRONOBU NAKAGUCHI

- Graphic Designers -

NOBUKO YANAGITA, YUMIKO HOSONO, HIRONOBU NAKAGUCHI, TOSHIHIKO YANAGITA, TOHRU OHYAMA
MASANORI ISHII, YASUSHI SHIGEHARA, HIDETOSHI SUZUKI, TATSUYA UCHIBORI, MASAKI NOZAWA
TOMOKO OHKAWA, FUMIKAZU SHIRATSUCHI, YASUNORI YAMADA, MUNENORI TAKIMOTO

- Message Converters -
TATSUYA UCHIBORI, HIDETOSHI SUZUKI, YASUSHI SHIGEHARA, YASUNORI YAMADA

- Floppy Disk Converters -
HIRONOBU NAKAGUCHI

- Music -
MASAO MIZOBE

- Special Thanks To -
HIROSHI YAMAMOTO, TAKAYOSHI KASHIWAGI

- Cooperate with -
Furniture KASHIWAGI

ZAMA HIGH SCHOOL MICRO COMPUTER CIRCLE";
            }
        }

        public ZAboutDialog()
        {
            InitializeComponent();

            var data = new AboutContents();
            icon.Source = new Bitmap(data.IconImageFile);

            // Avalonia には FlowDocument がないので TextBox に文字列を流す
            textBox.Text = data.AboutText;
        }
        private void OnOpened(object? sender, EventArgs e)
        {
            this.RequestedThemeVariant = ZSystem.Instance.Properties.Attrs.ThemeMode switch
            {
                ThemeType.Light => Avalonia.Styling.ThemeVariant.Light,
                ThemeType.Dark => Avalonia.Styling.ThemeVariant.Dark,
                ThemeType.System => Avalonia.Styling.ThemeVariant.Default,
                _ => Avalonia.Styling.ThemeVariant.Default,
            };
        }

        private void Button_Okay_Click(object? sender, RoutedEventArgs e)
            => Close(true);

        private void Minimize_Click(object? sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Maximize_Click(object? sender, RoutedEventArgs e)
            => WindowState = (WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;

        private void Close_Click(object? sender, RoutedEventArgs e)
            => Close(false);

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }
    }
}