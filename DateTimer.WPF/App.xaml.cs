using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using DateTimer.WPF.View;
using iNKORE.UI.WPF.Modern;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Input;
using System.Linq;
using DateTimer.WPF.View.CustomComponents;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Common;

namespace DateTimer.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 定义变量和常量
        public static CustomNotice _noticeWindow;                                                       // 时间表提示
        public static TaskbarIcon _taskbaricon;                                                         // 托盘图标
        public static Mutex _mutex;                                                                     // 程序启动监测
        public static TimerWindow _timerWindow;                                                         // 时间表窗口
        public readonly static string AppSettingPath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Settings.json");          // 设置文件
        public readonly static string DefTimetablePath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Timetable_Default.json"); // 默认时间表文件
        public readonly static string CopiedTimetablePath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Timetable_Copied.json");  // 外部复制时间表文件

        #endregion

        // 软件初始化函数
        public App()
        {
            Startup += AppStartUp;
            DispatcherUnhandledException += (_, e) =>
            {
                e.Handled = true;
                Clipboard.SetText(e.Exception.ToString());
                MsgBox.Show($"错误文本已复制到剪贴板。\n\n{e.Exception}", "发生了未知错误", MessageBoxButton.OK);
            };
        }

        // 当软件启动时
        protected override void OnStartup(StartupEventArgs e)
        {
            // Mutex 初始化
            _mutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out var createNew);
            if (!createNew)
            {
                MsgBox.Show("已有此程序在运行！", "提示", MessageBoxButton.OK);
                Current.Shutdown();
            }
            base.OnStartup(e);
        }

        // OnStartUp 引发事件
        private void AppStartUp(object s, EventArgs e)
        {
            // 初始化设置
            SettingsPage.LoadSettings();
            if (SettingsPage._appSetting.Theme == "Dark")
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; // 设置主题
            else if (SettingsPage._appSetting.Theme == "Light")
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            // 窗口初始化
            _taskbaricon = (TaskbarIcon)FindResource("Taskbar");
            _noticeWindow = new CustomNotice();
            _timerWindow = new TimerWindow();
            MainWindow mw = new();
            Current.MainWindow = mw;

            // 窗口设置应用
            if (SettingsPage._appSetting.EnableMainWindowShow)
                MainWindow.Show();
            else
            {
                _timerWindow.Show();
                _taskbaricon.ShowBalloonTip("控制台已隐藏", "可通过系统托盘图标显示", BalloonIcon.Info);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }

    /// <summary> 托盘图标命令 </summary>
    public class NotifyIconViewModel
    {
        public ICommand ShowMWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null && Application.Current.MainWindow.Visibility != Visibility.Visible,
                    CommandAction = () => Application.Current.MainWindow.Show()
                };
            }
        }
        public ICommand HideMWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Hide(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null && Application.Current.MainWindow.Visibility == Visibility.Visible,
                };
            }
        }
        public ICommand ShowTWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => App._timerWindow != null && App._timerWindow.Visibility != Visibility.Visible,
                    CommandAction = () =>
                    {
                        App._timerWindow.Show();
                        try
                        {
                            (Application.Current.Windows.Cast<Window>().
                            FirstOrDefault(window => window is MainWindow) as MainWindow).
                            _homePage.ShowTimer.Content = "隐藏时间表";
                        }
                        catch { }
                    }
                };
            }
        }
        public ICommand HideTWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => 
                    {
                        App._timerWindow.Hide();
                        try
                        {
                            (Application.Current.Windows.Cast<Window>().
                            FirstOrDefault(window => window is MainWindow) as MainWindow).
                            _homePage.ShowTimer.Content = "显示时间表";
                        }
                        catch { }
                    },
                    CanExecuteFunc = () => App._timerWindow != null && App._timerWindow.Visibility == Visibility.Visible,
                };
            }
        }
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
