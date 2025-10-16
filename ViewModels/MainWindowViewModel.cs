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

        // コンストラクタでサービスを受け取る
        public MainWindowViewModel(IAppSettingsService settingsService)
        {
            this.settingsService = settingsService;
            ZSystem.Instance.LoadPreferences();
            ApplicationFontSize = ZSystem.Instance.Properties.Attrs.FontSize;
            ApplicationThemeMode = ZSystem.Instance.Properties.Attrs.ThemeMode;

            // ★★★ サービスの変更を監視し、Viewに通知（プロキシ） ★★★
            // これにより、PreferencesでFontSizeが変更されると、
            // MainWindowのUIにバインドされた ApplicationFontSize も更新される。
            //SettingsService.WhenAnyValue(x => x.FontSize).Subscribe(size =>
            //    this.RaisePropertyChanged(nameof(ApplicationFontSize)));
            //SettingsService.WhenAnyValue(x => x.ThemeMode).Subscribe(mode =>
            //    this.RaisePropertyChanged(nameof(ApplicationThemeMode)));
            ZSystem.Instance.Properties.Attrs.WhenAnyValue(x => x.FontSize).Subscribe(size => ApplicationFontSize = size);
            ZSystem.Instance.Properties.Attrs.WhenAnyValue(x => x.ThemeMode).Subscribe(mode => ApplicationThemeMode = mode);
            SettingsService.WhenAnyValue(x => x.ThemeMode).Subscribe(mode =>
                ApplicationThemeMode = mode);
            SettingsService.WhenAnyValue(x => x.FontSize).Subscribe(size =>
                ApplicationFontSize = size);
        }
    }
}