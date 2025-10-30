using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq; // Throttleを使うため

namespace HHSAdvAvalonia.Services
{
    // 実際の設定保持と変更通知を行うクラス
    public class AppSettingsService : ReactiveObject, IAppSettingsService
    {
        // 初期値
        private int fontSize;
        private ThemeType themeMode;
        private int width;
        private int height;
        public int FontSize
        {
            get => fontSize;
            // set で値が変更されたときに通知
            set
            {
                ZSystem.Instance.Properties.Attrs.FontSize = value;
                this.RaiseAndSetIfChanged(ref fontSize, value);
            }
        }

        public ThemeType ThemeMode
        {
            get => themeMode;
            set
            {
                ZSystem.Instance.Properties.Attrs.ThemeMode = value;
                this.RaiseAndSetIfChanged(ref themeMode, value);
            }
        }

        public int Width
        {
            get => width;
            set
            {
                ZSystem.Instance.Properties.Attrs.Width = value;
                this.RaiseAndSetIfChanged(ref width, value);
            }
        }
        public int Height
        {
            get => height;
            set
            {
                ZSystem.Instance.Properties.Attrs.Height = value;
                this.RaiseAndSetIfChanged(ref height, value);
            }
        }

        // コンストラクタで変更通知を監視し、設定を自動保存するロジックを実装
        public AppSettingsService()
        {
            ZSystem.Instance.LoadPreferences();
            fontSize = ZSystem.Instance.Properties.Attrs.FontSize;
            themeMode = ZSystem.Instance.Properties.Attrs.ThemeMode;
            width = ZSystem.Instance.Properties.Attrs.Width;
            height = ZSystem.Instance.Properties.Attrs.Height;
            //ZSystem.Instance.Properties.Attrs.WhenAnyValue(x => x.FontSize)
            //    .Subscribe(_ =>
            //        this.RaisePropertyChanged(nameof(FontSize)));
            //ZSystem.Instance.Properties.Attrs.WhenAnyValue(x => x.ThemeMode)
            //    .Subscribe(_ =>
            //        this.RaisePropertyChanged(nameof(ThemeMode)));
        }
        
        // 永続化ロジック（ファイル書き込みなど）
        public void SaveSettings() => ZSystem.Instance.SavePreferences();       
    }
}