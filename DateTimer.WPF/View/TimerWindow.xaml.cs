using iNKORE.UI.WPF.Modern;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static DateTimer.WPF.Utils.TimeTable;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// TimerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimerWindow : Window
    {
        public ObservableCollection<TableSource> _source;
        private string current_timetable_path;
        public Timetables _timetables;
        public static bool _isRunning = false;
        public List<int> undone = new List<int>();

        public TimerWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            _source = new ObservableCollection<TableSource>();
            TimetableView.ItemsSource = _source;
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
            // 设置背景样式
            switch (SettingsPage._appSetting.BackDrop)
            {
                case "Mica":
                    iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(this, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Mica);
                    break;
                case "MicaAlt":
                    iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(this, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Tabbed);
                    break;
                case "Acrylic":
                    iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(this, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.Acrylic11);
                    break;
                case "None":
                    iNKORE.UI.WPF.Modern.Controls.Helpers.
                    WindowHelper.SetSystemBackdropType(this, iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType.None);
                    break;
            }
            ReloadTable();
            Check();
        }
        private void Window_Loaded(object s, EventArgs e) => SizeToContent = SizeToContent.Height;

        public async void ReloadTable()
        {
            current_timetable_path = SettingsPage._appSetting.TimeTablePath;
            try { GetTimetables(current_timetable_path); }
            catch { App._taskbaricon.ShowBalloonTip("无法配置时间表", "请检查 json 结构是否正确", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error); return; }
            List<Timetables> File = GetTimetables(current_timetable_path).Timetables;
            _timetables = GetTodayList(File);
            if (_timetables == null)
            {
                _source.Clear();
                _source.Add(new TableSource { Title = "今日无时间计划", Time = "0:00 ~ 23:59" });
            }
            else
            {
                _source.Clear();
                if (_timetables.Tables.Count == 0)
                {
                    _source.Add(new TableSource { Title = "未配置今日时间", Time = "0:00 ~ 23:59" });
                    return;
                }
                foreach (Table table in _timetables.Tables)
                {
                    _source.Add(Utils.TimerShow.Table2Entry(table));
                    await Task.Delay(50);
                }
            }
        }

        public void Check()
        {
            if (_timetables == null)
            {
                GetTime(false);
                return;
            }
            if (!isTableShowable(_timetables.Tables))
            {
                App._taskbaricon.ShowBalloonTip("无法配置时间表", "请检查时间段配置是否正确", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
                GetTime(false);
                return;
            }
            if (_isRunning)
            {
                Console.WriteLine("当前进程正在运行");
                return;
            }

            undone = GetTodayUndone(_timetables.Tables); // 未完成项目
            GetTime(true);
        }

        public async void GetTime(bool ShowTable)
        {
            _isRunning = true;
            await Task.Run(async () =>
            {
                int SpanSeconds = 0;
                while (true)
                {
                    // 目标时间
                    string str = "目标";
                    if (SettingsPage._appSetting.EnableTarget)
                    {
                        if (SettingsPage._appSetting.TargetName != null) str = SettingsPage._appSetting.TargetName;
                        if (SettingsPage._appSetting.TargetDate != null)
                            await Dispatcher.InvokeAsync(() => InfoText.Text = Utils.TimerShow.TimetableShowTargetTime(DateTime.Parse(SettingsPage._appSetting.TargetDate), str));
                        else await Dispatcher.InvokeAsync(() => InfoText.Text = $"未配置{str}日期");
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() =>
                        InfoText.Text = $"今天是 {DateTime.Today.Month}月{DateTime.Today.Day}日 " +
                        $"星期{Utils.TimeConverter.NumToWeekday(Convert.ToInt16(DateTime.Today.DayOfWeek).ToString())}");
                    }

                    // 当前时间
                    if (!ShowTable)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    if (current_timetable_path != SettingsPage._appSetting.TimeTablePath) break;
                    List<int> CurZone = GetCurZone(_timetables.Tables);
                    if (CurZone.Count == 0)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    await Dispatcher.InvokeAsync(() => TimetableView.SelectedIndex = CurZone[0]);

                    int nowind = IsStart(_timetables.Tables, TimeSpan.Zero);
                    if (nowind != -1 && undone[nowind] > 0)
                    {
                        undone[nowind] = 0;
                        Dispatcher.Invoke(() =>
                        {
                            App._noticeWindow.Init($"{_timetables.Tables[nowind].Name} 时间到了",
                                $"提示: " + (_timetables.Tables[nowind].Notice == null ? "无" : _timetables.Tables[nowind].Notice),
                                "Data/Media/alarm.wav");
                        });
                    }

                    if (SettingsPage._appSetting.EnableAdvancedNotice)
                    {
                        int fminind = IsStart(_timetables.Tables, TimeSpan.FromMinutes(SettingsPage._appSetting.AdvancedMinutes));
                        if (fminind != -1 && undone[fminind] == 2)
                        {
                            if (nowind != -1 && SpanSeconds < 10) SpanSeconds++;
                            else
                            {
                                SpanSeconds = 0;
                                undone[fminind] = 1;
                                Dispatcher.Invoke(() =>
                                {
                                    App._noticeWindow.Init($"{_timetables.Tables[fminind].Name} 时间",
                                        $"将在 {SettingsPage._appSetting.AdvancedMinutes} 分钟后到达");
                                });
                            }
                        }
                    }
                    await Task.Delay(1000);
                }
            });
            _isRunning = false;
            ReloadTable();
            Check();
        }

        #region 自定义事件
        // 设置主题色
        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
                ThemeManager.Current.AccentColor = SystemParameters.WindowGlassColor;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            (Application.Current.MainWindow as MainWindow)._homePage.ShowTimer.Content = "显示时间表";
        }
        #endregion
    }
    public class TableSource
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string Notice { get; set; }
    }
}
