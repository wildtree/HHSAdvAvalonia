using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using HHSAdvAvalonia.Services;

namespace HHSAdvAvalonia;

public partial class App : Application
{
    public static IAppSettingsService SettingService { get; } = new AppSettingsService();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                ViewModel = new ViewModels.MainWindowViewModel(SettingService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}