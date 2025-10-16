using HHSAdvAvalonia.Services;
using ReactiveUI; // ReactiveUIをインポート
using System;
using System.Reactive;
using System.Reactive.Linq;
using Tmds.DBus.Protocol; // ReactiveCommand<Unit, Unit> に必要

namespace HHSAdvAvalonia.ViewModels // プロジェクトの名前空間に合わせて修正
{
    // ★ ReactiveObjectを継承
    // (INotifyPropertyChangedの実装はFodyが自動で行います)
    public class ZPreferencesViewModel : ReactiveObject
    {
        // プロジェクト内の列挙型に合わせて修正してください
        //public enum ThemeType { Light, Dark, System }
        private readonly IAppSettingsService _settingsService;
        public int ZIndexValue { get; } = 1;
        private ZSystem zsystem = ZSystem.Instance;
        
        // ----------------------------------------------------
        // 1. データプロパティ (UIでバインドされるもの)
        // ----------------------------------------------------
        
        // Fodyを使用しているため、プロパティはバッキングフィールドを持ち、
        // this.RaiseAndSetIfChanged を使用して変更を通知します。   

        public ThemeType ThemeMode
        {
            get => _settingsService.ThemeMode;
            set => _settingsService.ThemeMode = value;
        }
        public int FontSize
        {
            get => _settingsService.FontSize;
            set => _settingsService.FontSize = value;
        }

        public bool OpeningRoll
        {
            get => zsystem.Properties.Attrs.OpeningRoll;
            set => zsystem.Properties.Attrs.OpeningRoll = value;
        }

        public bool PlaySound
        {
            get => zsystem.Properties.Attrs.PlaySound;
            set => zsystem.Properties.Attrs.PlaySound = value;
        }
        private bool _dialogResult;
        public bool DialogResult
        {
            get => _dialogResult;
            set => this.RaiseAndSetIfChanged(ref _dialogResult, value);
        }

        // ----------------------------------------------------
        // 2. コマンド (UIのボタンクリックなどの処理)
        // ----------------------------------------------------
        
        // 設定ボタンクリック時に実行されるコマンド
        // <Unit, Unit> は、「パラメーターなし」「戻り値なし」を意味します。
        public ReactiveCommand<Unit, Unit> OkayCommand { get; }
        public ReactiveCommand<int, Unit> ChangeFontSizeCommand { get; }
        public ReactiveCommand<ThemeType, Unit> ChangeThemeModeCommand { get; }
        public ReactiveCommand<bool, Unit> ChangeOpeningRollCommand { get; }
        public ReactiveCommand<bool, Unit> ChangePlaySoundCommand { get; }

        // ダイアログを閉じるためのインタラクション
        public Interaction<bool, Unit> CloseInteractionAsync { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ZPreferencesViewModel(IAppSettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();

            // コマンドの定義: 常に実行可能で、OnOkayClicked メソッドを実行します。
            OkayCommand = ReactiveCommand.Create(OnOkayClicked);
            ChangeFontSizeCommand = ReactiveCommand.Create<int>(size =>
            {
                this.FontSize = size;
            });
            ChangeThemeModeCommand = ReactiveCommand.Create<ThemeType>(mode =>
            {
                this.ThemeMode = mode;
                this.RaisePropertyChanged(nameof(ThemeMode)); // notify theme change to ZPreferences dialog code behind
            });
            ChangeOpeningRollCommand = ReactiveCommand.Create<bool>(value =>
            {
                this.OpeningRoll = value;
            });
            ChangePlaySoundCommand = ReactiveCommand.Create<bool>(value =>
            {
                this.PlaySound = value;
            });
            CloseInteractionAsync = new Interaction<bool, Unit>();
            CancelCommand = ReactiveCommand.CreateFromTask(async () => { await CloseInteractionAsync.Handle(false); });
            ApplyCommand = ReactiveCommand.CreateFromTask(async () => { _settingsService.SaveSettings(); await CloseInteractionAsync.Handle(true); });
        }

        private void LoadSettings()
        {
            zsystem.LoadPreferences();
        }

        private void OnOkayClicked()
        {
            // 設定を保存する処理をここに実装
            zsystem.SavePreferences();
        }
    }
}