using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using System.Reactive.Linq;
using HHSAdvAvalonia.Services;
using HHSAdvAvalonia.ViewModels;
using Avalonia.ReactiveUI;
using System.Reactive.Disposables;

namespace HHSAdvAvalonia
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private ZSystem zsystem;
        private Canvas canvas;
        private const string CRLF = "\r\n";
        private static string[] title =
        [
            @"ハイハイスクールアドベンチャー",
             @"Copyright(c)1995-2025",
              @"ZOBplus",
              @"hiro",

        ];
        private string credits =
@"High High School Adventure

PalmOS version: hiro © 2002-2004
Android version: hiro © 2011-2025
Web version: hiro © 2012-2024
M5 version: hiro © 2023-2025
Qt version: hiro © 2024-2025
PicoCalc version: hiro © 2025
SDL version: hiro © 2025
Windows version: hiro © 2025
AvaloniaUI version: hiro © 2025
.NET MAUI version: hiro © 2025

- Project ZOBPlus -
Hayami <hayami@zob.jp>
Exit <exit@zob.jp>
ezumi <ezumi@zob.jp>
Ogu <ogu@zob.jp>
neopara <neopara@zob.jp>
hiro <hiro@zob.jp>

--- Original Staff ---

- Director -
HIRONOBU NAKAGUCHI

- Graphic Designers -

NOBUKO YANAGITA
YUMIKO HOSONO
HIRONOBU NAKAGUCHI
TOSHIHIKO YANAGITA
TOHRU OHYAMA

MASANORI ISHII
YASUSHI SHIGEHARA
HIDETOSHI SUZUKI
TATSUYA UCHIBORI
MASAKI NOZAWA

TOMOKO OHKAWA
FUMIKAZU SHIRATSUCHI
YASUNORI YAMADA
MUNENORI TAKIMOTO

- Message Converters -
TATSUYA UCHIBORI
HIDETOSHI SUZUKI
YASUSHI SHIGEHARA
YASUNORI YAMADA

- Floppy Disk Converters -
HIRONOBU NAKAGUCHI

- Music -
MASAO MIZOBE

- Special Thanks To -
HIROSHI YAMAMOTO
TAKAYOSHI KASHIWAGI

- Cooperate with -
Furniture KASHIWAGI

ZAMA HIGH SCHOOL MICRO COMPUTER CIRCLE";

        private string opening =
@"ストーリー

2019年神奈山県立ハイ高等学校は地盤が弱く校舎の老朽化も進んだため、とうとう廃校にする以外方法がなくなってしまった。
ところで大変な情報を手に入れた。

それは、

「ハイ高校にＡＴＯＭＩＣ ＢＯＭＢが仕掛けられている。」

と、いうものだ。

どうやらハイ高が廃校になった時、気が狂った理科の先生がＡＴＯＭＩＣ ＢＯＭＢを、学校のどこかに仕掛けてしまったらしい。
お願いだ。我が母校のコナゴナになった姿を見たくはない。
早くＡＴＯＭＩＣ ＢＯＭＢを取り除いてくれ……！！

行動は英語で、
<動詞>
或いは、
<動詞>+<目的語>
のように入れていただきたい。
例えば、
look room
と入れれば部屋の様子を見ることが出来るという訳だ。

それでは Ｇｏｏｄ Ｌｕｃｋ！！！............";

        public TranslateTransform? RollCreditsTransform => this.RollCreditsImage.RenderTransform as TranslateTransform;
        //public Image RollCreditsImage => this.RollCreditsImage;
        //public Border RollOverlay => this.RollOverlay;
        private readonly IAppSettingsService settingsService = App.SettingService;
        public MainWindow()
        {
            InitializeComponent();
            canvas = new Canvas(256, 152);
            zsystem = ZSystem.Instance;
            this.WhenActivated(d =>
            {
                var vm = ViewModel!;
                vm.WhenAnyValue(x => x.ApplicationThemeMode)
                    //.ObserveOn(RxApp.MainThreadScheduler) // UIスレッドで実行
                    .Subscribe(themeMode =>
                    {
                        this.RequestedThemeVariant = themeMode switch
                        {
                            ThemeType.Light => Avalonia.Styling.ThemeVariant.Light,
                            ThemeType.Dark => Avalonia.Styling.ThemeVariant.Dark,
                            ThemeType.System => Avalonia.Styling.ThemeVariant.Default,
                            _ => Avalonia.Styling.ThemeVariant.Default,
                        };
                    });              
            });
        }
        private async void OnOpened(object? sender, EventArgs e)
        {
            zsystem.Init();
            Screen.Source = canvas.Bitmap;
            this.RequestedThemeVariant = zsystem.DarkMode switch
            {
                ThemeType.Light => Avalonia.Styling.ThemeVariant.Light,
                ThemeType.Dark => Avalonia.Styling.ThemeVariant.Dark,
                ThemeType.System => Avalonia.Styling.ThemeVariant.Default,
                _ => Avalonia.Styling.ThemeVariant.Default,
            };
            // Set FontSize if LogArea is initialized
            LogArea.FontSize = zsystem.Properties.Attrs.FontSize;
            InputArea.KeyDown += InputArea_KeyDown;
            await TitleScreen();
        }
        public async void OnClosed(object? sender, EventArgs e)
        {
            zsystem.SavePreferences();
        }
        public async Task UpdateScreenAndAwaitRenderAsync()
        {
            // 1. TaskCompletionSource を作成し、イベントの完了を待てるようにする
            var tcs = new TaskCompletionSource<bool>();
            
            // 2. イベントハンドラを定義し、発火したら Task を完了させる
            //    C#のイベントハンドラは、イベントが発生した後に自身を解除します。
            EventHandler handler = null!;
            
            handler = (s, e) =>
            {
                // UIスレッドで実行されているため、直接操作して問題ありません。
                // イベントハンドラを解除
                Screen.LayoutUpdated -= handler; 
                // 待機中の Task を完了させる
                tcs.TrySetResult(true); 
            };

            // 3. イベントハンドラを登録し、ソースを更新
            Screen.LayoutUpdated += handler;
            Screen.InvalidateVisual(); // 画面更新を要求

            // 4. Taskが完了するまで待機 (描画完了まで待機)
            await tcs.Task;
        }
        private bool IsDark()
        {
            bool dim = false;
            ZCore core = ZCore.Instance;
            ZUserData user = ZUserData.Instance;
            switch (core.MapId)
            {
                case 47:
                case 48:
                case 49:
                case 61:
                case 64:
                case 65:
                case 67:
                case 68:
                case 69:
                case 71:
                case 74:
                case 75:
                case 77:
                    if (user.getFact(7) != 0)
                    {
                        if (user.getFact(6) != 0)
                        {
                            // dark mode (blue)
                            dim = true;
                        }
                    }
                    else
                    {
                        // blackout
                        core.MapViewId = core.MapId;
                        zsystem.Map.Cursor = 84;
                    }
                    break;
                default:
                    if (user.getFact(6) != 0)
                    {
                        dim = false;
                    }
                    break;
            }
            return dim;
        }
        public void TimeElapsed()
        {
            ZUserData user = ZUserData.Instance;
            ZMessage messages = zsystem.Messages;

            if (user.getFact(3) > 0 && user.getFact(7) == 1)
            {
                // Light is ON
                user.setFact(3, (byte)(user.getFact(3) - 1));
                if (user.getFact(3) < 8 && user.getFact(3) > 0)
                {
                    // battery LOW
                    user.setFact(6, 1); // dim mode
                    LogArea.Text += messages.GetMessage(0xd9);
                    LogArea.Text += CRLF;
                }
                else if (user.getFact(3) == 0)
                {
                    // battery ware out
                    user.setFact(7, 0); // light off
                    LogArea.Text += messages.GetMessage(0xc0);
                    LogArea.Text += CRLF;
                }
            }
            if (user.getFact(11) > 0)
            {
                user.setFact(11, (byte)(user.getFact(11) - 1));
                if (user.getFact(11) == 0)
                {
                    LogArea.Text += messages.GetMessage(0xd8);
                    if (user.getPlace(7) == 48)
                    {
                        user.getLink(75 - 1).N = 77;
                        user.getLink(68 - 1).W = 77;
                        LogArea.Text += messages.GetMessage(0xda);
                        LogArea.Text += CRLF;
                    }
                    else if (user.getPlace(7) == 255 || user.getPlace(7) == zsystem.Map.Cursor)
                    {
                        // suicide explosion
                        // set screen color to red
                        canvas!.ColorFilterType = Canvas.FilterType.Red;
                        LogArea.Text += messages.GetMessage(0xcf);
                        LogArea.Text += CRLF;
                        LogArea.Text += messages.GetMessage(0xcb);
                        LogArea.Text += CRLF;
                        GameOver();
                    }
                    else
                    {
                        user.setPlace(7, 0);
                    }
                }
            }
        }
        private void CheckTeacher()
        {
            ZUserData user = ZUserData.Instance;
            ZCore core = ZCore.Instance;
            if (zsystem.Status == ZSystem.GameStatus.GameOver || user.getFact(1) == core.MapId)
                return;
            int rd = 100 + core.MapId + ((user.getFact(1) > 0) ? 1000 : 0);
            int rz = new Random().Next(3000);
            user.setFact(1, (byte)((rd < rz) ? 0 : core.MapId));
            switch (core.MapId)
            {
                case 1:
                case 48:
                case 50:
                case 51:
                case 52:
                case 53:
                case 61:
                case 64:
                case 65:
                case 66:
                case 67:
                case 68:
                case 69:
                case 70:
                case 71:
                case 72:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 83:
                case 86:
                    user.setFact(1, 0);
                    break;
            }
        }

        private async Task DrawScreen(bool drawMessage = true)
        {
            if (canvas != null)
            {
                ZMap map = zsystem.Map;
                if (canvas.ColorFilterType == Canvas.FilterType.Blue)
                {
                    canvas.ColorFilterType = Canvas.FilterType.None;
                }
                if (IsDark())
                {
                    canvas.ColorFilterType = Canvas.FilterType.Blue;
                }
                map.Draw(canvas);
                if (drawMessage && zsystem.Status != ZSystem.GameStatus.GameOver)
                {
                    string s = map.Message;
                    if (map.IsBlank)
                    {
                        LogArea.Text += zsystem.Messages.GetMessage(0xcc);
                        LogArea.Text += CRLF;
                    }
                    if (!string.IsNullOrEmpty(s))
                    {
                        LogArea.Text += s;
                        LogArea.Text += CRLF;
                    }
                }
                ZUserData user = ZUserData.Instance;
                ZObjects obj = zsystem.Objects;
                for (int i = 0; i < ZUserData.Items; i++)
                {
                    if (user.getPlace(i) == map.Cursor)
                    {
                        bool shift = (i == 1 && user.getFact(0) != 1);

                        obj.Id = i;
                        obj.Draw(canvas, shift);
                        if (drawMessage && zsystem.Status != ZSystem.GameStatus.GameOver)
                        {
                            LogArea.Text += zsystem.Messages.GetMessage(0x96 + i);
                            LogArea.Text += CRLF;
                        }
                    }
                }
                if (user.getFact(1) == map.Cursor)
                {
                    obj.Id = 14;
                    obj.Draw(canvas);
                    if (drawMessage && zsystem.Status != ZSystem.GameStatus.GameOver)
                    {
                        LogArea.Text += zsystem.Messages.GetMessage(0xb4);
                        LogArea.Text += CRLF;
                    }

                }
                canvas.colorFilter();
                //canvas.Invalidate();
                //Screen.Source = canvas.Bitmap;
                //await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
                await UpdateScreenAndAwaitRenderAsync();
                ScrollArea.ScrollToEnd();
            }
        }

        private void SaveGame(int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(index).Append(".dat");
            string fileName = System.IO.Path.Combine(zsystem.dataFolder, sb.ToString());
            ZCore core = ZCore.Instance;
            ZUserData user = ZUserData.Instance;
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (var br = new BinaryWriter(fs))
                {
                    br.Write(core.pack());
                    br.Write(user.pack());
                }
            }
        }
        private void LoadGame(int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(index).Append(".dat");
            string fileName = System.IO.Path.Combine(zsystem.dataFolder, sb.ToString());
            ZCore core = ZCore.Instance;
            ZUserData user = ZUserData.Instance;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    core.unpack(br.ReadBytes(core.packedSize));
                    user.unpack(br.ReadBytes(user.packedSize));
                }
            }
            zsystem.Map.Cursor = core.MapId;
        }
        private void GameOver()
        {
            zsystem.Status = ZSystem.GameStatus.GameOver;
            InputArea.IsVisible = false;
            HitAnyKey.IsVisible = true;
            this.KeyDown += Application_PreviewKeyDown;
        }

        private async Task ExecuteRules()
        {
            bool okay = false;
            ZCore core = ZCore.Instance;
            ZUserData user = ZUserData.Instance;
            ZMessage messages = zsystem.Messages;
            ZMap map = zsystem.Map;
            ZProperties properties = zsystem.Properties;
            foreach (var rule in zsystem.Rules.Rules)
            {
                if (rule.Evaluate())
                {
                    map.Cursor = core.MapId;
                    ZCore.ZCommand c = new ZCore.ZCommand();
                    ZAudio audio = zsystem.Audio;
                    while ((c = core.pop()).Cmd != ZCore.ZCommand.Command.Nop)
                    {
                        byte o = c.Operand;
                        switch (c.Cmd)
                        {
                            case ZCore.ZCommand.Command.Nop:
                                break;
                            case ZCore.ZCommand.Command.Message:
                                string s = messages.GetMessage(o);
                                if ((o & 0x80) == 0)
                                {
                                    s = map.Find(core.CmdId, core.ObjId);
                                }
                                LogArea.Text += s;
                                LogArea.Text += CRLF;
                                break;
                            case ZCore.ZCommand.Command.Sound:
                                if (properties.Attrs.PlaySound)
                                    audio!.Play(o);
                                break;
                            case ZCore.ZCommand.Command.Dialog:
                                ZDialog dialog;
                                switch (o)
                                {
                                    case 0: // boy or girl
                                        user.setFact(0, 1); // boy
                                        dialog = new ZDialog("選択", zsystem.Messages.GetMessage(0xe7), new string[] { "男子", "女子", string.Empty });
                                        if (await dialog.ShowDialog<bool>(this))
                                        {
                                            user.setFact(0, (byte)dialog.Selected);
                                            map!.Cursor = 3;
                                        }
                                        break;
                                    case 1:
                                        string[] labels = new string[] { "1", "2", "3" };
                                        string title = "セーブ";
                                        if (core.CmdId != 0x0f)
                                        {
                                            title = "ロード";
                                            int n = labels.Length;
                                            if (!File.Exists(System.IO.Path.Combine(zsystem.dataFolder, "1.dat")))
                                            {
                                                --n;
                                                labels[0] = string.Empty;
                                            }
                                            if (!File.Exists(System.IO.Path.Combine(zsystem.dataFolder, "2.dat")))
                                            {
                                                --n;
                                                labels[1] = string.Empty;
                                            }
                                            if (!File.Exists(System.IO.Path.Combine(zsystem.dataFolder, "3.dat")))
                                            {
                                                --n;
                                                labels[2] = string.Empty;
                                            }
                                            if (n == 0)
                                            {
                                                ZMessageBox messageBox = new ZMessageBox();
                                                messageBox.TitleTextBlock.Text = "情報";
                                                messageBox.textBox.Text = "セーブデータが存在していません。";
                                                await messageBox.ShowDialog(this);
                                                break;
                                            }
                                        }
                                        dialog = new ZDialog(title, zsystem.Messages.GetMessage(0xe8), labels);
                                        if (await dialog.ShowDialog<bool>(this) == true)
                                        {
                                            if (core.CmdId != 0x0f)
                                            {
                                                LoadGame(dialog.Selected);
                                            }
                                            else
                                            {
                                                SaveGame(dialog.Selected);
                                            }
                                        }
                                        break;
                                    case 2:
                                        //MessageBoxHelper.ShowCentered(this, user.getItemList(), "持物", MessageBoxButton.OK, MessageBoxImage.None);
                                        ZMessageBox invBox = new ZMessageBox();
                                        string items = user.getItemList();
                                        invBox.TitleTextBlock.Text = "持物";
                                        invBox.textBox.Text = string.IsNullOrEmpty(items) ? "持物はありません。" : items;
                                        await invBox.ShowDialog(this);
                                        break;
                                    case 3:
                                        dialog = new ZDialog("選択", zsystem.Messages.GetMessage(0xe9), new string[] { "黄", "赤", string.Empty });
                                        if (await dialog.ShowDialog<bool>(this))
                                        {
                                            if (user.getPlace(11) != 0xff)
                                            {
                                                LogArea.Text += zsystem.Messages.GetMessage(0xe0);
                                                LogArea.Text += CRLF;
                                            }
                                            if (dialog.Selected == 1 || user.getPlace(11) != 0xff)
                                            {
                                                canvas.ColorFilterType = Canvas.FilterType.Red;
                                                LogArea.Text += zsystem.Messages.GetMessage(0xc7);
                                                LogArea.Text += CRLF;
                                                LogArea.Text += zsystem.Messages.GetMessage(0xee);
                                                LogArea.Text += CRLF;
                                                GameOver();
                                                break;
                                            }
                                            user.setPlace(11, 0);
                                            map.Cursor = 74;
                                            break;
                                        }
                                        break;
                                }
                                break;
                            case ZCore.ZCommand.Command.GameOver:
                                switch (o)
                                {
                                    case 1: // teacher
                                        canvas!.ColorFilterType = Canvas.FilterType.Sepia;
                                        await DrawScreen(false);
                                        break;
                                    case 2: // explosion
                                        canvas!.ColorFilterType = Canvas.FilterType.Red;
                                        await DrawScreen(false);
                                        break;
                                    case 3: // clear
                                        if (properties.Attrs.PlaySound)
                                            audio!.Play(0);
                                        //TitleBarGrid.Visibility = Visibility.Collapsed;
                                        ZRoll endroll = new ZRoll(this);
                                        endroll.Credits = credits;
                                        endroll.TotalDuration = TimeSpan.FromSeconds(35);
                                        // { Owner = this, Credits = credits, TotalDuration = TimeSpan.FromSeconds(35) };
                                        endroll.Completed += async (s, e) =>
                                        {
                                            //TitleBarGrid.Visibility = Visibility.Visible;
                                            await DrawScreen(true);
                                        };
                                        endroll.ShowCredits();
                                        break;
                                }
                                GameOver();
                                break;
                        }
                    }
                    if (zsystem.Status == ZSystem.GameStatus.GameOver) return;
                    LogArea.Text += messages.GetMessage(0xed); // Ｏ．Ｋ．
                    LogArea.Text += CRLF;
                    okay = true;
                    break;
                }
            }
            map!.Cursor = core.MapId;
            if (!okay)
            {
                string s = map.Find(core.CmdId, core.ObjId);
                if (string.IsNullOrEmpty(s))
                {
                    s = messages.GetMessage(0xec); // ダメ
                }
                LogArea.Text += s;
                LogArea.Text += CRLF;
            }
            if (map.Cursor == 74)
            {
                int msg_id = 0;
                user.setFact(13, (byte)(user.getFact(13) + 1));
                switch (user.getFact(13))
                {
                    case 4: msg_id = 0xe2; break;
                    case 6: msg_id = 0xe3; break;
                    case 10: msg_id = 0xe4; break;
                }
                if (msg_id != 0)
                {
                    LogArea.Text += messages.GetMessage(msg_id);
                    LogArea.Text += CRLF;
                }
            }
        }
        private async Task TitleScreen()
        {
            this.KeyDown += Application_PreviewKeyDown;
            zsystem.Init();
            LogArea.Text = string.Empty;
            canvas.ColorFilterType = Canvas.FilterType.None;
            StringBuilder sb = new StringBuilder();
            foreach (var s in title)
            {
                sb.AppendLine(s);
            }
            LogArea.Text += sb.ToString();
            await DrawScreen(false);
            InputArea.IsVisible = false;
            HitAnyKey.IsVisible = true;
        }
        private async void Application_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            switch (zsystem.Status)
            {
                case ZSystem.GameStatus.Title:
                    EventHandler h = (s, e) =>
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            //TitleBarGrid.Visibility = Visibility.Visible;
                            zsystem.Map.Cursor = 1;
                            LogArea.Text = string.Empty;
                            zsystem.Status = ZSystem.GameStatus.Play;
                            // invoke opening roll?
                            await DrawScreen(true);
                            HitAnyKey.IsVisible = false;
                            InputArea.IsVisible = true;
                            InputArea.Focus();
                        });
                    };
                    this.KeyDown -= Application_PreviewKeyDown;
                    if (zsystem.Properties.Attrs.OpeningRoll)
                    {
                        //TitleBarGrid.Visibility = Visibility.Collapsed;
                        ZRoll openingRoll = new ZRoll(this);// { Owner = this, Credits = opening };
                        openingRoll.Credits = opening;
                        openingRoll.Completed += h;
                        //openingRoll.ShowCredits_DEBUG_TEST();
                        openingRoll.ShowCredits();
                    }
                    else
                    {
                        h.Invoke(this, EventArgs.Empty);
                    }
                    e.Handled = true;
                    break;
                case ZSystem.GameStatus.GameOver:
                    this.KeyDown -= Application_PreviewKeyDown;
                    await TitleScreen();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void Menu_Quit_Clicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => zsystem.Quit();

        private void File_Settings_Clicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Settings_Click(sender, e);
        private void Help_About_Clicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => About_Click(sender, e);

        private async void Settings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new ZPreferences();
            var viewModel = new ZPreferencesViewModel(settingsService);
            dialog.DataContext = viewModel;
            dialog.OnFontSizeChanged = (size) => LogArea.FontSize = size;
            if (await dialog.ShowDialog<bool>(this))
            {

            }
        }

        private async void About_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new ZAboutDialog();
            await dialog.ShowDialog(this);
        }

        private async void InputArea_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                string inputText = InputArea.Text ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(inputText))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($">> {inputText}");
                    LogArea.Text += sb.ToString();
                    InputArea.Text = string.Empty; // Clear the input area
                    e.Handled = true;
                    //
                    string[] args = inputText.Split(new char[] { ' ' });
                    var cmd = args[0].Trim();
                    var obj = (args.Length > 1) ? args[1].Trim() : string.Empty;
                    ZCore.Instance.CmdId = (byte)zsystem.Dict.findVerb(cmd);
                    ZCore.Instance.ObjId = (byte)zsystem.Dict.findObj(obj);
                    TimeElapsed();
                    if (zsystem.Status == ZSystem.GameStatus.GameOver) return;
                    await ExecuteRules();
                    if (zsystem.Status == ZSystem.GameStatus.GameOver) return;
                    CheckTeacher();
                    await DrawScreen(true);
                    //
                    return;
                }
            }
            e.Handled = false;
        }
    }
}