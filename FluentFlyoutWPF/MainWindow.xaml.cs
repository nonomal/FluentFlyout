﻿using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static WindowsMediaController.MediaManager;
using WindowsMediaController;
using Windows.Media.Control;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;
using MicaWPF.Controls;
using System.IO;
using System.Windows.Media.Animation;
using FluentFlyout;
using FluentFlyout.Properties;
using Microsoft.Win32;
using System.Reflection;
using System.Drawing;
using System.Windows.Controls;
using MicaWPF.Core.Models;


namespace FluentFlyoutWPF
{
    public partial class MainWindow : MicaWindow
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_APPCOMMAND = 0x0319;

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _hookProc;

        private CancellationTokenSource cts;

        private static readonly MediaManager mediaManager = new MediaManager();
        private static MediaSession? currentSession = null;

        private int _position = Settings.Default.Position;
        private bool _layout = Settings.Default.CompactLayout;
        private bool _repeatEnabled = Settings.Default.RepeatEnabled;
        private bool _shuffleEnabled = Settings.Default.ShuffleEnabled;

        static Mutex singleton = new Mutex(true, "FluentFlyout");

        public MainWindow()
        {
            InitializeComponent();

            if (!singleton.WaitOne(TimeSpan.Zero, true))
            {
                Application.Current.Shutdown();
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "FluentFlyout2.ico");

            if (Settings.Default.Startup)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                string executablePath = Assembly.GetExecutingAssembly().Location;
                key.SetValue("FluentFlyout", executablePath);
            }

            cts = new CancellationTokenSource();

            mediaManager.Start();

            _hookProc = HookCallback;
            _hookId = SetHook(_hookProc);

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -Width - 20;

            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
            mediaManager.OnAnyPlaybackStateChanged += CurrentSession_OnPlaybackStateChanged;
        }

        private void openSettings(object? sender, EventArgs e)
        {
            SettingsWindow.ShowInstance();
        }

        private int getDuration()
        {
            int msDuration = Settings.Default.FlyoutAnimationSpeed switch
            {
                0 => 0, // off
                1 => 150, // 0.5x
                2 => 300, // 1x
                3 => 450, // 1.5x
                4 => 600, // 2x
                _ => 900 // 3x
            };
            return msDuration;
        }

        private void OpenAnimation()
        {

            var eventTriggers = Triggers[0] as EventTrigger;
            var beginStoryboard = eventTriggers.Actions[0] as BeginStoryboard;
            var storyboard = beginStoryboard.Storyboard;

            DoubleAnimation moveAnimation = (DoubleAnimation)storyboard.Children[0];
            _position = Settings.Default.Position;
            if (_position == 0)
            {
                Left = 16;
                moveAnimation.From = SystemParameters.WorkArea.Height - Height - 0;
                moveAnimation.To = SystemParameters.WorkArea.Height - Height - 16;
            }
            else if (_position == 1)
            {
                Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
                moveAnimation.From = SystemParameters.WorkArea.Height - Height - 60;
                moveAnimation.To = SystemParameters.WorkArea.Height - Height - 80;
            }
            else if (_position == 2)
            {
                Left = SystemParameters.WorkArea.Width - Width - 16;
                moveAnimation.From = SystemParameters.WorkArea.Height - Height - 0;
                moveAnimation.To = SystemParameters.WorkArea.Height - Height - 16;
            }
            else if (_position == 3)
            {
                Left = 16;
                moveAnimation.From = 0;
                moveAnimation.To = 16;
            }
            else if (_position == 4)
            {
                Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
                moveAnimation.From = 0;
                moveAnimation.To = 16;
            }
            else if (_position == 5)
            {
                Left = SystemParameters.WorkArea.Width - Width - 16;
                moveAnimation.From = 0;
                moveAnimation.To = 16;
            }
            int msDuration = getDuration();

            DoubleAnimation opacityAnimation = (DoubleAnimation)storyboard.Children[1];
            opacityAnimation.From = 0;
            opacityAnimation.To = 1;
            opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(msDuration));

            EasingFunctionBase easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            moveAnimation.EasingFunction = opacityAnimation.EasingFunction = easing;
            moveAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(msDuration));

            storyboard.Begin(this);
        }

        private void CloseAnimation()
        {
            var eventTriggers = Triggers[0] as EventTrigger;
            var beginStoryboard = eventTriggers.Actions[0] as BeginStoryboard;
            var storyboard = beginStoryboard.Storyboard;

            DoubleAnimation moveAnimation = (DoubleAnimation)storyboard.Children[0];
            _position = Settings.Default.Position;
            if (_position == 0 || _position == 2)
            {
                moveAnimation.From = SystemParameters.WorkArea.Height - Height - 16;
                moveAnimation.To = SystemParameters.WorkArea.Height - Height - 0;
            }
            else if (_position == 1)
            {
                moveAnimation.From = SystemParameters.WorkArea.Height - Height - 80;
                moveAnimation.To = SystemParameters.WorkArea.Height - Height - 60;
            }
            else if (_position == 3 || _position == 4 || _position == 5)
            {
                moveAnimation.From = 16;
                moveAnimation.To = 0;
            }
            int msDuration = getDuration();

            DoubleAnimation opacityAnimation = (DoubleAnimation)storyboard.Children[1];
            opacityAnimation.From = 1;
            opacityAnimation.To = 0;
            opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(msDuration));

            EasingFunctionBase easing = new CubicEase { EasingMode = EasingMode.EaseIn };
            moveAnimation.EasingFunction = opacityAnimation.EasingFunction = easing;
            moveAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(msDuration));

            storyboard.Begin(this);
        }

        private void reportBug(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/unchihugo/FluentFlyout/issues/new",
                UseShellExecute = true
            });
        }

        private void openRepository(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/unchihugo/FluentFlyout",
                UseShellExecute = true
            });
        }

        private void CurrentSession_OnPlaybackStateChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo = null)
        {
            UpdateUI(mediaManager.GetFocusedSession());
        }

        private void MediaManager_OnAnyMediaPropertyChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            UpdateUI(mediaManager.GetFocusedSession());
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == 0xB3 || vkCode == 0xB0 || vkCode == 0xB1 || vkCode == 0xB2 // Play/Pause, next, previous, stop
                    || vkCode == 0xAD || vkCode == 0xAE || vkCode == 0xAF) // Mute, Volume Down, Volume Up
                {
                    ShowMediaFlyout();
                }

            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private async void ShowMediaFlyout()
        {
            if (mediaManager.GetFocusedSession() == null) return;
            UpdateUI(mediaManager.GetFocusedSession());

            if (Visibility == Visibility.Hidden) OpenAnimation();
            cts.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;
            Visibility = Visibility.Visible;
            Topmost = true;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(100, token); // check if mouse is over every 100ms
                    if (!IsMouseOver)
                    {
                        await Task.Delay(Settings.Default.Duration, token);
                        if (!IsMouseOver)
                        {
                            CloseAnimation();
                            await Task.Delay(getDuration());
                            Hide();
                            break;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        private void UpdateUI(MediaSession mediaSession)
        {
            if (_layout != Settings.Default.CompactLayout || _shuffleEnabled != Settings.Default.ShuffleEnabled || _repeatEnabled != Settings.Default.ShuffleEnabled) UpdateUILayout();

            Dispatcher.Invoke(() =>
            {
                if (mediaSession == null)
                {
                    SongTitle.Text = "No media playing";
                    SongArtist.Text = "";
                    SongImage.Source = null;
                    SymbolPlayPause.Symbol = Wpf.Ui.Controls.SymbolRegular.Stop16;
                    ControlPlayPause.IsEnabled = false;
                    ControlPlayPause.Opacity = 0.35;
                    ControlBack.IsEnabled = ControlForward.IsEnabled = false;     
                    ControlBack.Opacity = ControlForward.Opacity = 0.35;
                    return;
                }

                var mediaProperties = mediaSession.ControlSession.GetPlaybackInfo();
                if (mediaProperties != null)
                {
                    if (mediaSession.ControlSession.GetPlaybackInfo().Controls.IsPauseEnabled)
                    {
                        ControlPlayPause.IsEnabled = true;
                        ControlPlayPause.Opacity = 1;
                        SymbolPlayPause.Symbol = Wpf.Ui.Controls.SymbolRegular.Pause16;
                    }
                    else
                    {
                        ControlPlayPause.IsEnabled = true;
                        ControlPlayPause.Opacity = 1;
                        SymbolPlayPause.Symbol = Wpf.Ui.Controls.SymbolRegular.Play16;
                    }
                    ControlBack.IsEnabled = ControlForward.IsEnabled = mediaProperties.Controls.IsNextEnabled;
                    ControlBack.Opacity = ControlForward.Opacity = mediaProperties.Controls.IsNextEnabled ? 1 : 0.35;

                    if (Settings.Default.RepeatEnabled && !Settings.Default.CompactLayout)
                    {
                        ControlRepeat.Visibility = Visibility.Visible;
                        ControlRepeat.IsEnabled = mediaProperties.Controls.IsRepeatEnabled;
                        ControlRepeat.Opacity = mediaProperties.Controls.IsRepeatEnabled ? 1 : 0.35;
                        if (mediaProperties.AutoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.List)
                        {
                            SymbolRepeat.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowRepeatAll24;
                            SymbolRepeat.Opacity = 1;
                        }
                        else if (mediaProperties.AutoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.Track)
                        {
                            SymbolRepeat.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowRepeat124;
                            SymbolRepeat.Opacity = 1;
                        }
                        else if (mediaProperties.AutoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.None)
                        {
                            SymbolRepeat.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowRepeatAllOff24;
                            SymbolRepeat.Opacity = 0.5;
                        }
                    }
                    else ControlRepeat.Visibility = Visibility.Collapsed;

                    if (Settings.Default.ShuffleEnabled && !Settings.Default.CompactLayout)
                    {
                        ControlShuffle.Visibility = Visibility.Visible;
                        ControlShuffle.IsEnabled = mediaProperties.Controls.IsShuffleEnabled;
                        ControlShuffle.Opacity = mediaProperties.Controls.IsShuffleEnabled ? 1 : 0.35;
                        if (mediaProperties.IsShuffleActive == true)
                        {
                            SymbolShuffle.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowShuffle24;
                            SymbolShuffle.Opacity = 1;
                        }
                        else
                        {
                            SymbolShuffle.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowShuffleOff24;
                            SymbolShuffle.Opacity = 0.5;
                        }
                    }
                    else ControlShuffle.Visibility = Visibility.Collapsed;

                    MediaId.Text = mediaSession.Id;
                }

                var songInfo = mediaSession.ControlSession.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
                if (songInfo != null)
                {
                    SongTitle.Text = songInfo.Title;
                    SongArtist.Text = songInfo.Artist;
                    SongImage.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                }
            });
        }

        private void UpdateUILayout() {
            Dispatcher.Invoke(() =>
            {
                int extraWidth = Settings.Default.RepeatEnabled ? 36 : 0;
                extraWidth += Settings.Default.ShuffleEnabled ? 36 : 0;
                if (Settings.Default.CompactLayout)
                {
                    Height = 60;
                    Width = 400;
                    BodyStackPanel.Orientation = Orientation.Horizontal;
                    BodyStackPanel.Width = 300;
                    ControlsStackPanel.Margin = new Thickness(0);
                    ControlsStackPanel.Width = 104;
                    MediaIdStackPanel.Visibility = Visibility.Hidden;
                    SongImageBorder.Margin = new Thickness(0);
                    SongImage.Height = 36;
                    SongImageRect.Rect = new Rect(0, 0, 78, 36);
                    SongInfoStackPanel.Margin = new Thickness(8, 0, 0, 0);
                    SongInfoStackPanel.Width = 182;
                }
                else
                {
                    Height = 116;
                    Width = 310 + extraWidth;
                    BodyStackPanel.Orientation = Orientation.Vertical;
                    BodyStackPanel.Width = 194 + extraWidth;
                    ControlsStackPanel.Margin = Margin = new Thickness(12, 8, 0, 0);
                    ControlsStackPanel.Width = 184 + extraWidth;
                    MediaIdStackPanel.Visibility = Visibility.Visible;
                    SongImageBorder.Margin = new Thickness(6);
                    SongImage.Height = 78;
                    SongImageRect.Rect = new Rect(0, 0, 78, 78);
                    SongInfoStackPanel.Margin = new Thickness(12, 0, 0, 0);
                    SongInfoStackPanel.Width = 182 + extraWidth;
                }
            });
            _layout = Settings.Default.CompactLayout;
            _repeatEnabled = Settings.Default.RepeatEnabled;
            _shuffleEnabled = Settings.Default.ShuffleEnabled;
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            if (mediaManager.GetFocusedSession() == null)
                return;

            await mediaManager.GetFocusedSession().ControlSession.TrySkipPreviousAsync();
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            keybd_event(0xB3, 0, 0, IntPtr.Zero);

            if (mediaManager.GetFocusedSession() == null)
                return;

            //var controlsInfo = mediaManager.GetFocusedSession().ControlSession.GetPlaybackInfo().Controls;

            //if (controlsInfo.IsPauseEnabled == true)
            //{
            //    await mediaManager.GetFocusedSession().ControlSession.TryPauseAsync();
            //}
            //else if (controlsInfo.IsPlayEnabled == true)
            //    await mediaManager.GetFocusedSession().ControlSession.TryPlayAsync();
            if (mediaManager.GetFocusedSession().ControlSession.GetPlaybackInfo().Controls.IsPauseEnabled)
                SymbolPlayPause.Dispatcher.Invoke(() => SymbolPlayPause.Symbol = Wpf.Ui.Controls.SymbolRegular.Pause16);
            else
                SymbolPlayPause.Dispatcher.Invoke(() => SymbolPlayPause.Symbol = Wpf.Ui.Controls.SymbolRegular.Play16);
        }

        private async void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (mediaManager.GetFocusedSession() == null)
                return;

            await mediaManager.GetFocusedSession().ControlSession.TrySkipNextAsync();
        }

        private async void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (mediaManager.GetFocusedSession() == null)
                return;

            if (mediaManager.GetFocusedSession().ControlSession.GetPlaybackInfo().AutoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.None)
            {
                SymbolRepeat.Dispatcher.Invoke(() => SymbolRepeat.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowRepeatAll24);
                await mediaManager.GetFocusedSession().ControlSession.TryChangeAutoRepeatModeAsync(Windows.Media.MediaPlaybackAutoRepeatMode.List);
            }
            else if (mediaManager.GetFocusedSession().ControlSession.GetPlaybackInfo().AutoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.List)
            {
                SymbolRepeat.Dispatcher.Invoke(() => SymbolRepeat.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowRepeat124);
                await mediaManager.GetFocusedSession().ControlSession.TryChangeAutoRepeatModeAsync(Windows.Media.MediaPlaybackAutoRepeatMode.Track);
            }
            else if (mediaManager.GetFocusedSession().ControlSession.GetPlaybackInfo().AutoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.Track)
            {
                SymbolRepeat.Dispatcher.Invoke(() => SymbolRepeat.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowRepeatAllOff24);
                await mediaManager.GetFocusedSession().ControlSession.TryChangeAutoRepeatModeAsync(Windows.Media.MediaPlaybackAutoRepeatMode.None);
            }
        }

        private async void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            if (mediaManager.GetFocusedSession() == null)
                return;

            if (mediaManager.GetFocusedSession().ControlSession.GetPlaybackInfo().IsShuffleActive == true)
            {
                SymbolShuffle.Dispatcher.Invoke(() => SymbolShuffle.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowShuffleOff24);
                await mediaManager.GetFocusedSession().ControlSession.TryChangeShuffleActiveAsync(false);
            }
            else
            {
                SymbolShuffle.Dispatcher.Invoke(() => SymbolShuffle.Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowShuffle24);
                await mediaManager.GetFocusedSession().ControlSession.TryChangeShuffleActiveAsync(true);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookId);
            base.OnClosed(e);
        }

        internal static class Helper
        {
            internal static BitmapImage? GetThumbnail(IRandomAccessStreamReference Thumbnail, bool convertToPng = true)
            {
                if (Thumbnail == null)
                    return null;

                var thumbnailStream = Thumbnail.OpenReadAsync().GetAwaiter().GetResult();
                byte[] thumbnailBytes = new byte[thumbnailStream.Size];
                using (DataReader reader = new DataReader(thumbnailStream))
                {
                    reader.LoadAsync((uint)thumbnailStream.Size).GetAwaiter().GetResult();
                    reader.ReadBytes(thumbnailBytes);
                }

                byte[] imageBytes = thumbnailBytes;

                if (convertToPng)
                {
                    using var fileMemoryStream = new System.IO.MemoryStream(thumbnailBytes);
                    Bitmap thumbnailBitmap = (Bitmap)Bitmap.FromStream(fileMemoryStream);

                    if (!thumbnailBitmap.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    {
                        using var pngMemoryStream = new System.IO.MemoryStream();
                        thumbnailBitmap.Save(pngMemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        imageBytes = pngMemoryStream.ToArray();
                    }
                }

                var image = new BitmapImage();
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                }

                return image;
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        private void MicaWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ShowMediaFlyout();
        }

        private void NotifyIconQuit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MicaWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
            Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();
            UpdateUILayout();
            
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(
                this,
                Wpf.Ui.Controls.WindowBackdropType.Mica,
                true
            );
        }
    }
}