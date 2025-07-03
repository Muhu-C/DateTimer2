using iNKORE.UI.WPF.Modern;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using static DateTimer.WPF.Utils.TimeTable;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// TimerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimerWindow : Window
    {
        public ObservableCollection<TableSource> _source; // 时间表显示列表
        private string current_timetable_path;            // 当前使用的时间表 -- 与 SettingsPage._appSetting.TimeTablePath 略有不同
        public Timetables _timetables;                    // 当日时间表
        public static bool _isRunning = false;            // 窗口命令是否在 GetTime() 循环中
        public static bool _isCancelled = false; // 未被使用。
        public List<int> undone = new ();                 // 未到达时间段列表，按照 _timetables.Tables 下标，其中 值 2 为未提醒，值 1 为未到达

        public TimerWindow()
        {
            InitializeComponent();

            // 事件
            Loaded += Window_Loaded;
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;

            // 数据
            _source = new ObservableCollection<TableSource>();
            TimetableView.ItemsSource = _source;

            // 设置背景样式
            switch (SettingsPage._appSetting.BackDrop)
            {
                case "Mica": WindowHelper.SetSystemBackdropType(this, BackdropType.Mica); break;
                case "MicaAlt": WindowHelper.SetSystemBackdropType(this, BackdropType.Tabbed); break;
                case "Acrylic": WindowHelper.SetSystemBackdropType(this, BackdropType.Acrylic11); break;
                case "None": WindowHelper.SetSystemBackdropType(this, BackdropType.None); break;
            }

            // 开始执行
            ReloadTable();
            Check();
        }

        // 缩放至合适高度
        private void Window_Loaded(object s, EventArgs e) => SizeToContent = SizeToContent.Height;

        // 加载 SettingsPage._appSetting.TimeTablePath 中的时间表
        public async void ReloadTable()
        {
            // 检验时间表文件格式是否符合要求
            current_timetable_path = SettingsPage._appSetting.TimeTablePath;
            try { GetTimetables(current_timetable_path); }
            catch
            {
                App._taskbaricon.ShowBalloonTip("时间表文件读取失败!", " 请检查时间表json文件是否无误\n或者新建一个时间表文件",
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
                return;
            }

            // 读取并显示当天时间表
            List<Timetables> File = GetTimetables(current_timetable_path).Timetables;
            _timetables = GetTodayList(File);
            if (_timetables == null)
            {
                _source.Clear();
                _source.Add(new TableSource { Title = "今天没有时间计划" });
            }
            else
            {
                _source.Clear();
                if (_timetables.Tables.Count == 0)
                {
                    _source.Add(new TableSource { Title = "今天的时间计划为空" });
                    return;
                }
                foreach (Table table in _timetables.Tables)
                {
                    _source.Add(Utils.TimerShow.Table2Entry(table));
                    await Task.Delay(50);
                }
            }
        }

        // 检查时间表并分类进入 GetTime() 函数
        public void Check()
        {
            Console.WriteLine("awdadawdawdawdawdawdadwwdawd");
            if (_timetables == null)
            {
                GetTime(false);
                return;
            }
            if (IsTableInverted(_timetables.Tables) != -1)
            {
                App._taskbaricon.ShowBalloonTip("时间表文件配置错误!", $"位置: 第 {IsTableInverted(_timetables.Tables) + 1} 个时间段\n时间段的开始时间晚于结束时间",
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
                GetTime(false);
                return;
            }
            if (IsTableSorted(_timetables.Tables).Count > 0)
            {
                string str = "以下时间段的 开始时间 与前一个时间段冲突\n第";
                foreach (int i in IsTableSorted(_timetables.Tables))
                    str += $" {i + 1}";
                MsgBox.Show(str + "个时间段", "时间表文件配置错误", MessageBoxButton.OK);
                GetTime(false);
                return;
            }
            undone = GetTodayUndone(_timetables.Tables);
            if (_isRunning)
            {
                Console.WriteLine("窗口命令在 GetTime() 循环中");
                return;
            }
            GetTime(true);
        }

        // 获取当前时间，检测时段，并显示倒计时
        // ShowTable = false : 显示倒计时，不检测时段
        public async void GetTime(bool ShowTable)
        {
            _isRunning = true;
            await Task.Run(async () =>
            {
                int SpanSeconds = 0;
                while (!_isCancelled)
                {
                    if (current_timetable_path != SettingsPage._appSetting.TimeTablePath) break;  // 判断时间表位置更改

                    string str = "目标";                                                          // 目标时间与倒计时
                    if (SettingsPage._appSetting.EnableTarget)
                    {
                        if (SettingsPage._appSetting.TargetName != null) str = SettingsPage._appSetting.TargetName;
                        if (SettingsPage._appSetting.TargetDate != null)
                            await Dispatcher.InvokeAsync(() => 
                            InfoText.Text = Utils.TimerShow.TimetableShowTargetTime
                            (DateTime.Parse(SettingsPage._appSetting.TargetDate), str));
                        else await Dispatcher.InvokeAsync(() => InfoText.Text = $"未配置{str}日期");
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() =>
                        InfoText.Text = $"今天是 {DateTime.Today.Month}月{DateTime.Today.Day}日 " +
                        $"星期{Utils.TimeConverter.NumToWeekday(Convert.ToInt16(DateTime.Today.DayOfWeek).ToString())}");
                    }

                    if (!ShowTable)                                                               // 跳过后续步骤，进入下一循环
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    List<int> CurZone = GetCurZone(_timetables.Tables);                           // 显示当前时间段
                    if (CurZone.Count != 0)
                        await Dispatcher.InvokeAsync(() => TimetableView.SelectedIndex = CurZone[0]);
                    CurZone = null;

                    int nowind = IsStart(_timetables.Tables, TimeSpan.Zero);                      // 到达时间提示
                    if (nowind != -1 && undone[nowind] > 0)
                    {
                        undone[nowind] = 0;
                        Dispatcher.Invoke(() =>
                        {
                            App._noticeWindow.Init($"{_timetables.Tables[nowind].Name} 时间到了",
                                $"提示: " + (_timetables.Tables[nowind].Notice ?? "无"),
                                "Data/Media/alarm.wav");
                        });
                    }

                    if (SettingsPage._appSetting.EnableAdvancedNotice)                            // 到达时间提前提示
                                                                                                  // 条件: _appSetting.EnableAdvancedNotice
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

    // 显示在窗口的当天时间表
    public class TableSource
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string Notice { get; set; }
    }
}
