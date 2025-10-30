using System.Dynamic;
using ReactiveUI; // ReactiveObjectを使用するため

namespace HHSAdvAvalonia.Services
{
    // アプリケーション全体の設定インターフェース
    public interface IAppSettingsService
    {
        // 外部から設定可能なフォントサイズプロパティ
        int FontSize { get; set; }       
        // アプリケーションテーマモード（もしあれば）
        ThemeType ThemeMode { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        // 設定を永続化（保存）するためのメソッド
        void SaveSettings();
    }
}