using DateTimer.WPF.View.CustomComponents;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static DateTimer.WPF.Utils.TimeTable;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// EditPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditPage : Page
    {
        private static string timetable_file;               // 选择时间表位置
        public static List<Timetables> timetables = new (); // 当前编辑的时间表
        private static bool isInternalChange = false;              // 是否是内部修改
        public EditPage()
        {
            InitializeComponent();
            UpperControlPanel.Visibility = Visibility.Collapsed;
            ControlPanel.Visibility = Visibility.Collapsed;
            TPDel.Visibility = Visibility.Collapsed;
            TPNew.Visibility = Visibility.Collapsed;

            EditDay.Visibility = Visibility.Collapsed;
            DelDay.Visibility = Visibility.Collapsed;

            AddDay.Visibility = Visibility.Collapsed;
            TableSave.Visibility = Visibility.Collapsed;

            PosText.Text = "未选择时间表";
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
                if (saveFileDialog.SafeFileName == "Settings.json")
                {
                    MsgBox.Show("请勿创建设置文件", "提示");
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

                GetTimetables(saveFileDialog.FileName); // 尝试读取时间表
                timetable_file = saveFileDialog.FileName;
                timetables = GetTimetables(timetable_file).Timetables;
                UpperControlPanel.Visibility = Visibility.Visible;
                AddDay.Visibility = Visibility.Visible;
                PosText.Text = saveFileDialog.FileName;
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
                    MsgBox.Show("请勿选择设置文件", "提示");
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
                        foreach (var timetable in timetables)
                        {
                            if (isOutdated(timetable))
                                TimeSel.Items.Add(new TextBlock
                                {
                                    Foreground = Brushes.Orange,
                                    Text = DisplaySingleTimetable(timetable),
                                    ToolTip = new TextBlock { Foreground = Brushes.Orange, Text = "该时间表已过期！" }
                                });
                            else
                                TimeSel.Items.Add(DisplaySingleTimetable(timetable));
                        }
                    UpperControlPanel.Visibility = Visibility.Visible;
                    AddDay.Visibility = Visibility.Visible;
                    PosText.Text = ofd.FileName;
                }
                catch 
                {
                    MsgBox.Show("时间表文件读取失败! \n请检查时间表json文件是否无误\n或新建一个时间表文件", "错误");
                    TableSave.Visibility = Visibility.Collapsed;
                    return;
                }
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

            isInternalChange = true; // 设置为内部修改，防止触发 TextChanged 事件
            TPStart.SelectedDateTime = DateTime.Parse(timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start);
            TPEnd.SelectedDateTime = DateTime.Parse(timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End);
            TPElement.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Name;
            TPNotice.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Notice;
            TPSelSpan.Text = $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start} ~ " +
                $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End}" + 
                $" (第 {SelDayTimeList.SelectedIndex + 1} 个)";
            isInternalChange = false; // 重置为非内部修改
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
            if (isInternalChange) return; // 如果是内部修改，则不进行处理
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start = $"{TPStart.SelectedDateTime:HH:mm}";
            int sel = SelDayTimeList.SelectedIndex;
            SelDayTimeList.Items[SelDayTimeList.SelectedIndex] = $"{TPStart.SelectedDateTime:HH:mm} ~ {timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End}";
            SelDayTimeList.SelectedIndex = sel; // 保持选中状态
        }
        private void TPEnd_TextChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (isInternalChange) return;
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End = $"{TPEnd.SelectedDateTime:HH:mm}";
            int sel = SelDayTimeList.SelectedIndex;
            SelDayTimeList.Items[SelDayTimeList.SelectedIndex] = $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start} ~ {TPEnd.SelectedDateTime:HH:mm}";
            SelDayTimeList.SelectedIndex = sel; // 保持选中状态
        }
        private void TPElement_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInternalChange) return;
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Name = TPElement.Text;
        }
        private void TPNotice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInternalChange) return;
            timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Notice = TPNotice.Text;
        }

        /// <summary>
        /// 保存时间表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableSave_Click(object sender, RoutedEventArgs e)
        {
            int ind = 0;
            var errorStrList = new List<string>();
            foreach (var timeTable in timetables)
            {
                ind++;
                var sortedConflicts = IsTableSorted(timeTable.Tables);
                if (sortedConflicts.Count > 0)
                    errorStrList.Add($"在第 {ind} 个时间表内, \n{string.Join("，", sortedConflicts.ConvertAll(i => $"第{i + 1}个"))} 时间段的 开始时间 与前一个时间段冲突");
                int invertedIndex = IsTableInverted(timeTable.Tables);
                if (invertedIndex != -1)
                    errorStrList.Add($"在第 {ind} 个时间表内, \n第 {invertedIndex + 1} 个时间段的 开始时间 与 结束时间 冲突");
            }
            if (errorStrList.Count > 0)
                MsgBox.Show(string.Join("\n\n", errorStrList), "时间表保存失败！", MessageBoxButton.OK);
            else if (MsgBox.Show("是否保存时间表？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                WriteTimeTables(new TimeTableFile() { Timetables = timetables }, timetable_file);
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
                    foreach (var timetable in timetables)
                    {
                        if (isOutdated(timetable))
                            TimeSel.Items.Add(new TextBlock
                            {
                                Foreground = Brushes.Orange,
                                Text = DisplaySingleTimetable(timetable),
                                ToolTip = new TextBlock { Foreground = Brushes.Orange, Text = "该时间表已过期！" }
                            });
                        else
                            TimeSel.Items.Add(DisplaySingleTimetable(timetable));
                    }
            }
        }
    }
}
