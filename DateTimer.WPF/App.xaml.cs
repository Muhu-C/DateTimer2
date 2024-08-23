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

        public Mutex _mutex;
        public readonly static string AppSettingPath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Settings.json");
        public readonly static string DefTimetablePath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Timetable_Default.json");
        public readonly static string CopiedTimetablePath = System.IO.Path.
            Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Timetable_Copied.json");
        public App() => Startup += AppStartUp;
        // public static TimerWindow _timerWindow;

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
            // _timerWindow = new TimerWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
