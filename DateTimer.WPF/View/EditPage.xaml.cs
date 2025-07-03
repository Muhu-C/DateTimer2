using DateTimer.WPF.View.CustomComponents;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static DateTimer.WPF.Utils.TimeTable;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// EditPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditPage : Page
    {
        private static string timetable_file;                // 选择时间表位置
        public static List<Timetables> timetables = new (); // 当前编辑的时间表
        public EditPage()
        {
            InitializeComponent();
            ControlPanel.Visibility = Visibility.Collapsed;
            TPDel.Visibility = Visibility.Collapsed;
            TPNew.Visibility = Visibility.Collapsed;

            EditDay.Visibility = Visibility.Collapsed;
            DelDay.Visibility = Visibility.Collapsed;

            AddDay.Visibility = Visibility.Collapsed;
            TableSave.Visibility = Visibility.Collapsed;
        }

        #region 文件操作
        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "新建并打开一个时间表文件",
                Filter = "时间表的 json 文件 |*.json"
            };
            if ((bool)saveFileDialog.ShowDialog())
            {
                if(saveFileDialog.SafeFileName == "Settings.json")
                {
                    MsgBox.Show("请勿创建设置文件", "错误");
                    return;
                }
                WriteTimeTables(new() { Timetables = new() }, saveFileDialog.FileName);

                TableSave.Visibility = Visibility.Visible;
                AddDay.Visibility = Visibility.Collapsed;
                TPNew.Visibility = Visibility.Collapsed;
                PosText.Text = "";
                TPSelSpan.Text = "";
                SelDayTimeList.Items.Clear();
                TimeSel.Items.Clear();

                try
                {
                    GetTimetables(saveFileDialog.FileName); // 尝试读取时间表
                    timetable_file = saveFileDialog.FileName;
                    timetables = GetTimetables(timetable_file).Timetables;
                    if (timetables != null)
                    {
                        foreach (var timetable in timetables)
                        {
                            if (timetable.Date != null)
                                TimeSel.Items.Add(timetable.Date);
                            else if (timetable.Date == null && timetable.Weekday != null)
                                TimeSel.Items.Add(timetable.Weekday);
                            else TimeSel.Items.Add("无");
                        }
                    }
                    AddDay.Visibility = Visibility.Visible;
                    PosText.Text = saveFileDialog.FileName;
                }
                catch { MsgBox.Show("时间表结构不正确", "错误"); TableSave.Visibility = Visibility.Collapsed; return; }
            }
        }

        private void ChooseTable_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Title = "选取时间表文件",
                Filter = "时间表的 json 文件 |*.json"
            };
            if ((bool)ofd.ShowDialog())
            {
                if(ofd.SafeFileName == "Settings.json")
                {
                    MsgBox.Show("请勿选择设置文件", "错误");
                    return;
                }

                TableSave.Visibility = Visibility.Visible;
                AddDay.Visibility = Visibility.Collapsed;
                TPNew.Visibility = Visibility.Collapsed;
                PosText.Text = "";
                TPSelSpan.Text = "";
                SelDayTimeList.Items.Clear();
                TimeSel.Items.Clear();

                try
                {
                    GetTimetables(ofd.FileName); // 尝试读取时间表
                    timetable_file = ofd.FileName;
                    timetables = GetTimetables(timetable_file).Timetables;
                    if (timetables != null)
                    {
                        foreach (var timetable in timetables)
                        {
                            if (timetable.Date != null)
                                TimeSel.Items.Add(timetable.Date);
                            else if (timetable.Date == null && timetable.Weekday != null)
                                TimeSel.Items.Add(timetable.Weekday);
                            else TimeSel.Items.Add("无");
                        }
                    }
                    AddDay.Visibility = Visibility.Visible;
                    PosText.Text = ofd.FileName;
                }
                catch { MsgBox.Show("时间表结构不正确", "错误"); TableSave.Visibility = Visibility.Collapsed; return; }
            }
        }
        #endregion

        /// <summary>
        /// 日期更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeSel.SelectedIndex < 0)
            {
                SelDayTimeList.Items.Clear();
                EditDay.Visibility = Visibility.Collapsed;
                DelDay.Visibility = Visibility.Collapsed;
                TPNew.Visibility = Visibility.Collapsed;
                return;
            }
            SelDayTimeList.Items.Clear();
            TPSelSpan.Text = "";
            if (timetables[TimeSel.SelectedIndex].Tables != null) 
                foreach (Table table in timetables[TimeSel.SelectedIndex].Tables) 
                    SelDayTimeList.Items.Add($"{table.Start} ~ {table.End}");
            else timetables[TimeSel.SelectedIndex].Tables = new List<Table>();
            EditDay.Visibility = Visibility.Visible;
            DelDay.Visibility = Visibility.Visible;
            TPNew.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 时间段更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelDayTimeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelDayTimeList.SelectedIndex < 0)
            {
                ControlPanel.Visibility = Visibility.Collapsed;
                TPDel.Visibility = Visibility.Collapsed;
                return;
            }
            ControlPanel.Visibility = Visibility.Visible;
            TPDel.Visibility = Visibility.Visible;
            TPNew.Visibility = Visibility.Visible;

            TPStart.SelectedDateTime = DateTime.Parse(timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start);
            TPEnd.SelectedDateTime = DateTime.Parse(timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End);
            TPElement.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Name;
            TPNotice.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Notice;
            TPSelSpan.Text = $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start} ~ " +
                $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End}";
        }

        /// <summary>
        /// 删除时间段
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TPDel_Click(object sender, RoutedEventArgs e)
        {
            if (SelDayTimeList.SelectedIndex < 0)
            {
                ControlPanel.Visibility = Visibility.Collapsed;
                TPDel.Visibility = Visibility.Collapsed;
                return;
            }
            timetables[TimeSel.SelectedIndex].Tables.Remove(timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex]);
            SelDayTimeList.Items.Clear();
            TPSelSpan.Text = "";
            foreach (Table table in timetables[TimeSel.SelectedIndex].Tables)
                SelDayTimeList.Items.Add($"{table.Start} ~ {table.End}");
            EditDay.Visibility = Visibility.Visible;
            DelDay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 新建时间段
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TPNew_Click(object sender, RoutedEventArgs e)
        {
            int previousSelectedIndex;
            if (SelDayTimeList.SelectedIndex < 0)
            {
                timetables[TimeSel.SelectedIndex].Tables.Add(new Table() { Start = "07:00", End = "22:00" });
                previousSelectedIndex = 0;
            }
            else
            {
                timetables[TimeSel.SelectedIndex].Tables.Insert(SelDayTimeList.SelectedIndex + 1, new Table() { Start = "07:00", End = "22:00" });
                previousSelectedIndex = SelDayTimeList.SelectedIndex + 1;
            }
            SelDayTimeList.Items.Clear();
            TPSelSpan.Text = "";
            foreach (Table table in timetables[TimeSel.SelectedIndex].Tables)
                SelDayTimeList.Items.Add($"{table.Start} ~ {table.End}");
            SelDayTimeList.SelectedIndex = previousSelectedIndex;
            EditDay.Visibility = Visibility.Visible;
            DelDay.Visibility = Visibility.Visible;
        }

        private void TPStart_TextChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start = $"{TPStart.SelectedDateTime:HH:mm}";
            int previousSelectedIndex = SelDayTimeList.SelectedIndex;
            SelDayTimeList.Items.Clear();
            TPSelSpan.Text = "";
            foreach (Table table in timetables[TimeSel.SelectedIndex].Tables)
                SelDayTimeList.Items.Add($"{table.Start} ~ {table.End}");
            SelDayTimeList.SelectedIndex = previousSelectedIndex;
        }
        private void TPEnd_TextChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End = $"{TPEnd.SelectedDateTime:HH:mm}";
            int previousSelectedIndex = SelDayTimeList.SelectedIndex;
            SelDayTimeList.Items.Clear();
            TPSelSpan.Text = "";
            foreach (Table table in timetables[TimeSel.SelectedIndex].Tables)
                SelDayTimeList.Items.Add($"{table.Start} ~ {table.End}");
            SelDayTimeList.SelectedIndex = previousSelectedIndex;
        }
        private void TPElement_TextChanged(object sender, TextChangedEventArgs e)
        {
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Name = TPElement.Text;
        }
        private void TPNotice_TextChanged(object sender, TextChangedEventArgs e)
        {
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Notice = TPNotice.Text;
        }

        /// <summary>
        /// 保存时间表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableSave_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Show("是否保存时间表？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                WriteTimeTables(new TimeTableFile() { Timetables = timetables }, timetable_file);
            }
        }

        private void AddDay_Click(object sender, RoutedEventArgs e)
        {
            EditTimetableDay editTimetableDay = new () { Title = "新建时间表" };
            editTimetableDay.ShowAsync();
        }

        private void EditDay_Click(object sender, RoutedEventArgs e)
        {
            EditTimetableDay editTimetableDay = new() { Title = "编辑时间表" };
            editTimetableDay.ShowAsync();
        }

        private void DelDay_Click(object sender, RoutedEventArgs e)
        {
            if(MsgBox.Show("是否删除选中日期的计划？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                timetables.Remove(timetables[TimeSel.SelectedIndex]);
                TimeSel.Items.Clear();
                if (timetables != null)
                {
                    foreach (var timetable in timetables)
                    {
                        if (timetable.Date != null)
                            TimeSel.Items.Add(timetable.Date);
                        else if (timetable.Date == null && timetable.Weekday != null)
                            TimeSel.Items.Add(timetable.Weekday);
                        else TimeSel.Items.Add("无");
                    }
                }
            }
        }
    }
}
