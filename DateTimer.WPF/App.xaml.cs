using System;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading;
using System.Windows;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using DateTimer.WPF.View;
using iNKORE.UI.WPF.Modern;

namespace DateTimer.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 定义变量和常量

        public static Mutex _mutex;
        public static TimerWindow _timerWindow;
        public readonly static string AppSettingPath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Settings.json");
        public readonly static string DefTimetablePath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Timetable_Default.json");
        public readonly static string CopiedTimetablePath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Timetable_Copied.json");
        public App()
        {
            Startup += AppStartUp;
            DispatcherUnhandledException += (_, e) =>
            {
                Clipboard.SetText(e.Exception.ToString());
                MessageBox.Show($"错误文本已复制到剪贴板。\n\n{e.Exception}", "发生了未知错误", MessageBoxButton.OK);
                e.Handled = true;
            };
        }

        #endregion

        // 当程序启动时
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
            _timerWindow = new TimerWindow();

            MainWindow mw = new MainWindow();
            Current.MainWindow = mw;
            if (SettingsPage._appSetting.EnableMainWindowShow)
                MainWindow.Show();
            else _timerWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
