using ReactiveUI;
using HHSAdvAvalonia.Services;
using System;
using System.Reactive.Linq; // Subscribeを使うため

namespace HHSAdvAvalonia.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        // サービスを保持するためのプライベート変数
        private readonly IAppSettingsService settingsService;
        public IAppSettingsService SettingsService { get => settingsService; }

        // Viewで直接バインドするために、サービスのプロパティを公開する（オプション）
        // MainWindowのFontSizeプロパティをこの値にバインドできます。
        //public IObservable<int> WhenFontSizeChanged { get; }
        //public IObservable<ThemeType> WhenThemeModeChanged { get; }

        //public int ApplicationFontSize => settingsService.FontSize;
        //public ThemeType ApplicationThemeMode => settingsService.ThemeMode;

        private int fontSize;
        private ThemeType themeType;
        private int width;
        private int height;
        public int ApplicationFontSize
        {
            get => fontSize;
            set => this.RaiseAndSetIfChanged(ref fontSize, value);
        }
        public ThemeType ApplicationThemeMode
        {
            get => themeType;
            set => this.RaiseAndSetIfChanged(ref themeType, value);
        }        
        public int ApplicationWidth
        {
            get => settingsService.Width;
            set => settingsService.Width = value;
        }
        public int ApplicationHeight
        {
            get => settingsService.Height;
            set => settingsService.Height = value;
        }

        // コンストラクタでサービスを受け取る
        public MainWindowViewModel(IAppSettingsService settingsService)
        {
            this.settingsService = settingsService;
            ZSystem.Instance.LoadPreferences();
            ApplicationFontSize = ZSystem.Instance.Properties.Attrs.FontSize;
            ApplicationThemeMode = ZSystem.Instance.Properties.Attrs.ThemeMode;
            ApplicationWidth = ZSystem.Instance.Properties.Attrs.Width;
            ApplicationHeight = ZSystem.Instance.Properties.Attrs.Height;

            // ★★★ サービスの変更を監視し、Viewに通知（プロキシ） ★★★
            // これにより、PreferencesでFontSizeが変更されると、
            // MainWindowのUIにバインドされた ApplicationFontSize も更新される。
            SettingsService.WhenAnyValue(x => x.ThemeMode).Subscribe(mode =>
                ApplicationThemeMode = mode);
            SettingsService.WhenAnyValue(x => x.FontSize).Subscribe(size =>
                ApplicationFontSize = size);
            SettingsService.WhenAnyValue(x => x.Width).Subscribe(width =>
                ApplicationWidth = width);
            SettingsService.WhenAnyValue(x => x.Height).Subscribe(height =>
                ApplicationHeight = height);
        }
    }
}