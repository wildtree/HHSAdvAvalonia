// ZPreferences.axaml.cs

using System;
//using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI; // AvaloniaSchedulerなどのため
using ReactiveUI;
using HHSAdvAvalonia.ViewModels;
using static HHSAdvAvalonia.ZProperties;
using System.Reactive.Disposables;
using System.Reactive;
using Avalonia.Input; // ZProperties.Attributes, ZSystem のために必要

namespace HHSAdvAvalonia
{
    // ★★★ 1. IViewFor<T> と IActivatableView を実装 ★★★
    public partial class ZPreferences : Window, IViewFor<ZPreferencesViewModel>
    {
        // IViewFor<T> の明示的な実装 (必須)
        public ZPreferencesViewModel? ViewModel { get; set; }
        // ZPreferences 側で必要なプロパティ/イベント
        public Action<int>? OnFontSizeChanged { get; set; }

        public ZProperties.Attributes Settings
        {
            get => ZSystem.Instance.Properties.Attrs;
            private set => ZSystem.Instance.Properties.Attrs = value;
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ZPreferencesViewModel?)value; }

        public ZPreferences()
        {
            InitializeComponent();
            var disposables = new CompositeDisposable();
            this.WhenAnyValue(x => x.ViewModel)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    // ViewModelの DialogResult の変更を監視
                    vm.WhenAnyValue(x => x.DialogResult)
                        .WhereNotNull()
                        .Subscribe(result =>
                        {
                            this.Close(result);
                        })
                        .DisposeWith(disposables); // CompositeDisposable に購読を追加
                });
            this.Closed += (sender, e) =>
            {
                // ディアクティブ化イベントを発火
                disposables.Dispose();
            };
            this.WhenActivated(disposable =>
            {
                var vm = (ZPreferencesViewModel?)DataContext;
                disposable(vm!.CloseInteractionAsync.RegisterHandler(ctx =>
                {
                    this.Close(ctx.Input);
                    ctx.SetOutput(Unit.Default);
                }));
                // pickup change theme mode and apply it immediately
                vm!.WhenAnyValue(x => x.ThemeMode).Subscribe(mode =>
                    this.RequestedThemeVariant = mode switch
                    {
                        ThemeType.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeType.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        ThemeType.System => Avalonia.Styling.ThemeVariant.Default,
                        _ => Avalonia.Styling.ThemeVariant.Default,
                    }
                ).DisposeWith(disposables);
                // ViewModelの ThemeMode の変更を監視し、UIに反映
            });

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
        // ★★★ 4. XAMLでフックしているイベントハンドラ ★★★
        
        private void Minimize_Click(object? sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Maximize_Click(object? sender, RoutedEventArgs e)
            => WindowState = (WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;

        private void Close_Click(object? sender, RoutedEventArgs e)
            => Close(false);
        // タイトルバーのPointerPressedイベントハンドラー
        private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            // プライマリボタン（通常は左クリック）が押された場合のみ処理
            if (e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
            {
                // BeginMoveDrag() メソッドを呼び出す
                // これがウィンドウをドラッグ移動可能にするAvaloniaの組み込み機能です。
                // 第2引数は通常は null です。
                BeginMoveDrag(e);

                // イベントを処理済みに設定
                e.Handled = true;
            }
        }
    }
}