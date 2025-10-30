using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Media.TextFormatting;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.ReactiveUI;

namespace HHSAdvAvalonia
{
    internal class ZRoll
    {
        public MainWindow Owner { get; }
        public string Credits { get; set; } = string.Empty;
        public TimeSpan TotalDuration { get; set; } = TimeSpan.FromSeconds(20);

        public event EventHandler? Completed;

        public ZRoll(MainWindow owner)
        {
            Owner = owner;
        }
        public async void ShowCredits()
        {
            Owner.RollOverlay.IsVisible = true;

            // 1. テキストをビットマップ化し、Sourceに設定
            var bmp = CreateCreditsBitmap(Credits, 16);
            Owner.RollCreditsImage.Source = bmp;

            // ★★★ 2. 画面がビットマップを描画するまで強制的に待機 ★★★
            
            // TaskCompletionSource を使った描画完了待機ロジック
            var tcs = new TaskCompletionSource<bool>();
            EventHandler handler = null!;
            
            handler = (s, e) =>
            {
                Owner.RollCreditsImage.LayoutUpdated -= handler; 
                tcs.TrySetResult(true); 
            };

            // LayoutUpdated イベントを購読して、描画を促す
            Owner.RollCreditsImage.LayoutUpdated += handler;
            // ビジュアルの無効化を要求
            Owner.RollCreditsImage.InvalidateVisual(); 
            
            // LayoutUpdated が発火するのを待つ
            await tcs.Task; 
            
            // ★★★ 3. ここに到達した時点でビットマップは画面に描画されていることが保証される ★★★

            // 初期位置を画面下に
            Owner.RollCreditsTransform!.Y = Owner.Bounds.Height;

            // レイアウト確定後にアニメーション開始 (Dispatcher.UIThread.InvokeAsync は不要)
            // すでにUIスレッド上なので、そのまま実行します。
            var anim = new Avalonia.Animation.Animation
            {
                Duration = TotalDuration,
                FillMode = FillMode.Forward,
                Children =
                {
                    // ... KeyFrames の定義は省略 ...
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(TranslateTransform.YProperty, Owner.Bounds.Height) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(TranslateTransform.YProperty, -bmp.PixelSize.Height -Owner.Bounds.Height + 480) }
                    }
                }
            };

            // アニメーションの実行
            await anim.RunAsync(Owner.RollCreditsImage).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Completed?.Invoke(this, EventArgs.Empty);
                    Owner.RollOverlay.IsVisible = false;
                    Owner.RollCreditsImage.Source = null;
                });
            });
        }

        private Bitmap CreateCreditsBitmap(string creditsText, double fontSize = 24)
        {          
            var layout = new TextLayout(
                creditsText.Replace("\r\n", "\n"),
                new Typeface("Yu Gothic UI"),
                fontSize,
                Brushes.White,
                TextAlignment.Center,             // 第5引数
                TextWrapping.Wrap,                // 第6引数
                TextTrimming.None,                // 第7引数
                null,                             // 装飾なし
                FlowDirection.LeftToRight,
                Owner.Bounds.Width, // Owner.Bounds.Width - fontSize,    // maxWidth
                double.PositiveInfinity           // maxHeight
            );
            var rtb = new RenderTargetBitmap(
                new PixelSize((int)Math.Ceiling(Owner.Bounds.Width), //(int)Math.Ceiling(layout.Width) + 4,
                              (int)Math.Ceiling(layout.Height) + 4));

            using (var ctx = rtb.CreateDrawingContext())
            {
                // ★★★ 背景を透明で塗りつぶす ★★★
                // RenderTargetBitmap全体をクリア。これにより黒いオーバーレイとの区別がつく。
                ctx.DrawRectangle(Brushes.Transparent, null, new Rect(rtb.PixelSize.ToSize(1.0)));
                
                // テキストを描画
                layout.Draw(ctx, new Point(2, 2)); // 4ピクセルのパディングを考慮 
            }

            return rtb;
        }
    }
}