using iNKORE.UI.WPF.Modern;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using System.Threading.Tasks;
using System.Reflection;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public static AppSetting _appSetting;
        private static bool isInit = false;    // 是否正在加载（执行 ReloadPage()）

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadPage();
            TargetExpander.IsExpanded = true;
            NoticeExpander.IsExpanded = true;
            VersionText.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString()[..5];
            CopyrightYearText.Text = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright.Substring(9) + " All rights reserved.";
            if (App.isBeta) BetaText.Text = App.BetaVersion;
            else BetaText.Visibility = Visibility.Collapsed;
        }

        #region 基础操作
        // 加载 _appSetting 中的内容到 UI, 读取设置
        /*
         ETToggle: 倒计时显示
         EMToggle: 控制台显示
         TWToggle: (ETToggle is ON)显示 2 周内目标星期日
         ANToggle: 提前提醒
         */
        private void ReloadPage()
        {
            isInit = true;
            if (_appSetting.TimeTablePath != null)
                TablePosTb.Text = Path.GetFileName(_appSetting.TimeTablePath);
            else TablePosTb.Text = "默认时间表";
            if (!_appSetting.EnableTarget)
            {
                TargetExpanderGrid.IsEnabled = false;
                TargetExpander.IsExpanded = false;
            }
            if (!_appSetting.EnableAdvancedNotice)
            {
                NoticeExpanderGrid.IsEnabled = false;
                NoticeExpander.IsExpanded = false;
            }

            ETToggle.IsOn = _appSetting.EnableTarget;
            TWToggle.IsOn = _appSetting.EnableTargetWeekday;
            EMToggle.IsOn = _appSetting.EnableMainWindowShow;
            TargetPick.SelectedDate = _appSetting.TargetDate == null ? null : DateTime.Parse(_appSetting.TargetDate);

            TargetNameTb.Text = _appSetting.TargetName;
            ANToggle.IsOn = _appSetting.EnableAdvancedNotice;
            AdvanceNb.Value = _appSetting.AdvancedMinutes;
            switch (_appSetting.Theme)
            {
                case "Light": ThemeSelector.SelectedIndex = 2; break;
                case "Dark": ThemeSelector.SelectedIndex = 1; break;
                case "Auto": ThemeSelector.SelectedIndex = 0; break;
            }
            switch (_appSetting.BackDrop)
            {
                case "None": BackdropSelector.SelectedIndex = 0; break;
                case "Mica": BackdropSelector.SelectedIndex = 1; break;
                case "Acrylic": BackdropSelector.SelectedIndex = 2; break;
                case "MicaAlt": BackdropSelector.SelectedIndex = 3; break;
            }
            isInit = false;
        }

        // 写入当前设置
        public static void WriteCurSetting()
        {
            Utils.FileProcess.WriteFile(JsonConvert.SerializeObject(_appSetting, Formatting.Indented), App.AppSettingPath);
        }

        // 加载设置
        public static void LoadSettings()
            =>  _appSetting = JsonConvert.DeserializeObject<AppSetting>(Utils.FileProcess.ReadFile(App.AppSettingPath));
        #endregion

        // 启动时显示控制台
        private void EMToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            _appSetting.EnableMainWindowShow = EMToggle.IsOn;
            WriteCurSetting();
        }

        // 刷新时间表
        private void RefreshTableButton_Click(object sender, RoutedEventArgs e)
        {
            DisableRefresh();
            App._timerWindow.ReloadTable(true);
        }

        private async void DisableRefresh()
        {
            RefreshTableButton.IsEnabled = false;
            await Task.Delay(5000);
            RefreshTableButton.IsEnabled = true;
        }

        // 更改时间表位置
        private void ChangeTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "时间表文件|*.json",
                Title = "更改默认时间表",
            };
            if ((bool)openFileDialog.ShowDialog())
            {
                if (openFileDialog.SafeFileName == "Settings.json")
                {
                    MsgBox.Show("请勿选择设置文件! ", "提示");
                    return;
                }
                string FileName;
                if (openFileDialog.FileName == App.DefTimetablePath)
                {
                    _appSetting.TimeTablePath = null;
                    WriteCurSetting();
                    ReloadPage();
                    return;
                }
                if (openFileDialog.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))
                {
                    FileName = openFileDialog.FileName.Substring
                        (AppDomain.CurrentDomain.BaseDirectory.Length - 1,
                        openFileDialog.FileName.Length - AppDomain.CurrentDomain.BaseDirectory.Length + 1);
                    _appSetting.TimeTablePath = openFileDialog.FileName;
                }
                else
                {
                    File.Copy(openFileDialog.FileName, App.CopiedTimetablePath, true);
                    FileName = App.CopiedTimetablePath;
                    _appSetting.TimeTablePath = FileName;
                }
                WriteCurSetting();
                ReloadPage();
                App._timerWindow.ReloadTable();
            }
        }

        // 亮暗色
        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string NewTheme;
            if (isInit || ThemeSelector.SelectedIndex < 0) return;
            if (ThemeSelector.SelectedIndex == 0)
            {
                RegistryKey key =
                    Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key == null)
                {
                    MsgBox.Show("当前系统不支持亮暗色，将自动设置为亮色。", "注意");
                    NewTheme = "Light";
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                }
                else
                {
                    int thm = (int)key.GetValue("AppsUseLightTheme", -1);
                    if (thm == 0)
                    {
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        NewTheme = "Auto";
                    }
                    else if (thm == 1)
                    {
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        NewTheme = "Auto";
                    }
                    else
                    {
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        MsgBox.Show("获取系统主题失败，将自动设置为亮色。", "注意");
                        NewTheme = "Light";
                    }
                }
            }
            else if (ThemeSelector.SelectedIndex == 1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                NewTheme = "Dark";
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                NewTheme = "Light";
            }
            _appSetting.Theme = NewTheme;
            WriteCurSetting();
        }

        // 设置窗口主题
        private void BackdropSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit || BackdropSelector.SelectedIndex < 0) return;
            string NewBackdrop;
            if (BackdropSelector.SelectedIndex == 1) // 云母
            {
                NewBackdrop = "Mica";
                WindowHelper.SetSystemBackdropType(Application.Current.MainWindow, BackdropType.Mica);
                WindowHelper.SetSystemBackdropType(App._timerWindow, BackdropType.Mica);
                MsgBox.DefaultBackdropType = BackdropType.Mica;
            }
            else if (BackdropSelector.SelectedIndex == 2) // 亚克力
            {
                NewBackdrop = "Acrylic";
                WindowHelper.SetSystemBackdropType(Application.Current.MainWindow, BackdropType.Acrylic11);
                WindowHelper.SetSystemBackdropType(App._timerWindow, BackdropType.Acrylic11);
                MsgBox.DefaultBackdropType = BackdropType.Acrylic11;
            }
            else if (BackdropSelector.SelectedIndex == 3) // 云母 Alt
            {
                NewBackdrop = "MicaAlt";
                WindowHelper.SetSystemBackdropType(Application.Current.MainWindow, BackdropType.Tabbed);
                WindowHelper.SetSystemBackdropType(App._timerWindow, BackdropType.Tabbed);
                MsgBox.DefaultBackdropType = BackdropType.Tabbed;
            }
            else // 无
            {
                NewBackdrop = "None";
                WindowHelper.SetSystemBackdropType(Application.Current.MainWindow, BackdropType.None);
                WindowHelper.SetSystemBackdropType(App._timerWindow, BackdropType.None);
                MsgBox.DefaultBackdropType = BackdropType.None;
            }
            _appSetting.BackDrop = NewBackdrop;
            WriteCurSetting();
        }

        // 生成报告
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string ReportStr = string.Empty;
            await Task.Run(() =>
            {
                ReportStr =
                    $"生成时间: {DateTime.Now:yyyy/MM/dd HH:mm:ss}" +
                    $"\n系统版本: {SystemInfo.GetWinVer()}" +
                    $"\n系统位数: {SystemInfo.GetBit()}" +
                    $"\n处理器: {SystemInfo.GetCPUName()}" +
                    $"\n应用内存占用: {SystemInfo.GetRAMSize()} MB / {SystemInfo.GetTotalRAM()} MB" +
                    $"\n环境: {SystemInfo.GetEnvVer()}" + 
                    $"\n木沪时间表版本: {Assembly.GetExecutingAssembly().GetName().Version} {(App.isBeta ? App.BetaVersion : "")}";
            });
           
            if (MsgBox.Show(ReportStr + "\n是否复制到剪贴板? ", "系统报告", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                Clipboard.SetText(ReportStr);
            ReportStr = string.Empty;
        }


        // 倒计时显示
        private void ETToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            if (ETToggle.IsOn)
            {
                _appSetting.EnableTarget = true;
                TargetExpanderGrid.IsEnabled = true;
                TargetExpander.IsExpanded = true;
            }
            else
            {
                _appSetting.EnableTarget = false;
                TargetExpanderGrid.IsEnabled = false;
                TargetExpander.IsExpanded = false;
            }
            WriteCurSetting();
            (Application.Current.MainWindow as MainWindow)._homePage.ReloadSettings();
        }

        // 倒计时目标名称
        private void TargetNameTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit) return;
            if (TargetNameTb.Text == string.Empty) _appSetting.TargetName = null;
            else _appSetting.TargetName = TargetNameTb.Text;
            WriteCurSetting();
            (Application.Current.MainWindow as MainWindow)._homePage.ReloadSettings();
        }

        // 倒计时目标日期
        private void TargetPick_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit) return;
            _appSetting.TargetDate = TargetPick.SelectedDate?.ToString("yyyy/MM/dd");
            WriteCurSetting();
            (Application.Current.MainWindow as MainWindow)._homePage.ReloadSettings();
        }

        // 显示 2 周内目标星期日
        private void TWToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            _appSetting.EnableTargetWeekday = TWToggle.IsOn;
            WriteCurSetting();
            (Application.Current.MainWindow as MainWindow)._homePage.ReloadSettings();
        }


        // 提前提醒
        private void ANToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            if (ANToggle.IsOn)
            {
                _appSetting.EnableAdvancedNotice = true;
                NoticeExpanderGrid.IsEnabled = true;
                NoticeExpander.IsExpanded = true;
            }
            else
            {
                _appSetting.EnableAdvancedNotice = false;
                NoticeExpanderGrid.IsEnabled = false;
                NoticeExpander.IsExpanded = false;
            }
            WriteCurSetting();
        }

        // 提前提醒分钟数
        private void AdvanceNb_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (isInit) return;
            int value = (int)args.NewValue;
            if (value <= 0)
                value = 1;
            else if (value > 60)
                value = 60;
            _appSetting.AdvancedMinutes = value;
            AdvanceNb.Value = value;
            App._timerWindow.ReloadTable();
            WriteCurSetting();
        }
    }
}
