using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace HHSAdvAvalonia
{
    public partial class ZMessageBox : Window
    {
        public ZMessageBox()
        {
            InitializeComponent();
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

        private void Minimize_Click(object? sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Maximize_Click(object? sender, RoutedEventArgs e)
            => WindowState = (WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;

        private void Close_Click(object? sender, RoutedEventArgs e)
            => Close();

        // タイトルバーのドラッグ移動
        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void Okay_Clicked(object? sender, RoutedEventArgs e)
            => Close(true); // WPFのDialogResult代わりにClose(result)を返せる
    }
}