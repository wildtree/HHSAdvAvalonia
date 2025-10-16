using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia;
using System;

namespace HHSAdvAvalonia
{
    public partial class ZDialog : Window
    {
        private string TitleText { get; set; } = "選択";
        private string Message { get; set; } = "選択してください";
        private string[] Labels =
        {
            "#1", "#2", "#3",
        };
        public int Selected { get; private set; } = 0;
        public ZDialog()
        {
            InitializeComponent();
            this.Opened += ZDialog_Loaded;
        }
        public ZDialog(string title, string message, string[] labels)
        {
            InitializeComponent();

            TitleText = title;
            TitleTextBlock.Text = TitleText;
            Message = message;
            Labels = labels;

            // Avalonia では Loaded イベントは AttachedToVisualTree を使うのが一般的
            this.Opened += ZDialog_Loaded;
        }

        private void ZDialog_Loaded(object? sender, EventArgs e)
        {
            this.Title = TitleText;
            textBox.Text = Message;

            bool isChecked = true;

            if (string.IsNullOrEmpty(Labels[0]))
            {
                Option1.IsVisible = false;
            }
            else
            {
                Option1.IsVisible = true;
                Option1.Content = Labels[0];
                Option1.IsChecked = isChecked;
                isChecked = false;
            }

            if (string.IsNullOrEmpty(Labels[1]))
            {
                Option2.IsVisible = false;
            }
            else
            {
                Option2.IsVisible = true;
                Option2.Content = Labels[1];
                Option2.IsChecked = isChecked;
                isChecked = false;
            }

            if (string.IsNullOrEmpty(Labels[2]))
            {
                Option3.IsVisible = false;
            }
            else
            {
                Option3.IsVisible = true;
                Option3.Content = Labels[2];
                Option3.IsChecked = isChecked;
                isChecked = false;
            }
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
        private void Okay_Clicked(object? sender, RoutedEventArgs e)
        {
            Selected = 0;
            if (Option1.IsChecked == true) Selected = 1;
            if (Option2.IsChecked == true) Selected = 2;
            if (Option3.IsChecked == true) Selected = 3;

            Close(true); // Avalonia では DialogResult の代わりに Close(result)
        }

        private void Minimize_Click(object? sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Maximize_Click(object? sender, RoutedEventArgs e)
            => WindowState = (WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;

        private void Close_Click(object? sender, RoutedEventArgs e)
            => Close(false);

        // タイトルバーのドラッグ移動
        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }
    }
}