using iNKORE.UI.WPF.Modern;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using iNKORE.UI.WPF.Modern.Controls;
using System.Threading.Tasks;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public static AppSetting _appSetting;
        public static bool isInit = false;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadPage();
        }

        #region 基础操作
        // 加载 _appSetting 中的内容到 UI
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
            string SettingJsonStr = JsonConvert.SerializeObject(_appSetting, Formatting.Indented);
            Utils.FileProcess.WriteFile(SettingJsonStr, App.AppSettingPath);
        }

        // 加载设置
        public static void LoadSettings()
            =>  _appSetting = // 反序列化 Settings.json
                JsonConvert.DeserializeObject<AppSetting>(Utils.FileProcess.ReadFile(App.AppSettingPath));
        #endregion


        private void TWToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            _appSetting.EnableTargetWeekday = TWToggle.IsOn;
            var mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            WriteCurSetting();
            mw._homePage.ReloadSettings();
        }


        private void EMToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInit) return;
            _appSetting.EnableMainWindowShow = EMToggle.IsOn;
            WriteCurSetting();
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
                string FileName;
                if (openFileDialog.FileName == App.DefTimetablePath)
                {
                    _appSetting.TimeTablePath = null;
                    WriteCurSetting();
                    ReloadPage();
                    return;
                }
                if (openFileDialog.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))
                    FileName = openFileDialog.FileName.Substring
                        (AppDomain.CurrentDomain.BaseDirectory.Length - 1, 
                        openFileDialog.FileName.Length - AppDomain.CurrentDomain.BaseDirectory.Length + 1);
                else
                {
                    File.Copy(openFileDialog.FileName, App.CopiedTimetablePath, true);
                    FileName = App.CopiedTimetablePath;
                }
                _appSetting.TimeTablePath = FileName;
                WriteCurSetting();
                ReloadPage();
            }
        }

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
            var mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            WriteCurSetting();
            mw._homePage.ReloadSettings();
        }

        private void TargetNameTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit) return;
            if (TargetNameTb.Text == string.Empty) _appSetting.TargetName = null;
            else _appSetting.TargetName = TargetNameTb.Text;
            var mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            WriteCurSetting();
            mw._homePage.ReloadSettings();
        }

        private void TargetPick_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit) return;
            _appSetting.TargetDate = TargetPick.SelectedDate?.ToString("yyyy/MM/dd");
            var mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            WriteCurSetting();
            mw._homePage.ReloadSettings();
        }

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

        private void AdvanceNb_ValueChanged(iNKORE.UI.WPF.Modern.Controls.NumberBox sender, iNKORE.UI.WPF.Modern.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (isInit) return;
            int value = (int)args.NewValue;
            if (value <= 0)
                value = 1;
            else if (value > 60)
                value = 60;
            _appSetting.AdvancedMinutes = value;
            AdvanceNb.Value = value;
            WriteCurSetting();
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string NewTheme = "Light";
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

        private void BackdropSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit || BackdropSelector.SelectedIndex < 0) return;
            var mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            string NewBackdrop = "None";
            if (BackdropSelector.SelectedIndex == 0)
            {
                NewBackdrop = "None";
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(mw, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.None);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._timerWindow, 
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.None);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._noticeWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.None);
            }
            else if (BackdropSelector.SelectedIndex == 1)
            {
                NewBackdrop = "Mica";
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(mw, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Mica);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._timerWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Mica);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._noticeWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Mica);
            }
            else if (BackdropSelector.SelectedIndex == 2)
            {
                NewBackdrop = "Acrylic";
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(mw, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Acrylic11);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._timerWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Acrylic11);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._noticeWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Acrylic11);
            }
            else
            {
                NewBackdrop = "MicaAlt";
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(mw, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Tabbed);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._timerWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Tabbed);
                iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(App._noticeWindow,
                    iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Tabbed);
            }
            _appSetting.BackDrop = NewBackdrop;
            WriteCurSetting();
        }

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
                    $"\n程序内存占用: {SystemInfo.GetRAMSize()} MB" +
                    $"\n系统总内存: {SystemInfo.GetTotalRAM()} MB" +
                    $"\n环境: {SystemInfo.GetEnvVer()}";
            });
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "系统报告",
                Content = ReportStr,
                PrimaryButtonText = "复制",
                SecondaryButtonText = "取消"
            };
            var a = await contentDialog.ShowAsync();
            if (a == ContentDialogResult.Primary)
                Clipboard.SetText(ReportStr);
        }
    }
}
