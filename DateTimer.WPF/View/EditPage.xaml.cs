using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static List<Timetables> timetables = new (); // 当前编辑的时间表
        public EditPage()
        {
            InitializeComponent();
            ControlPanel.Visibility = Visibility.Collapsed;
            TPDel.Visibility = Visibility.Collapsed;
            TPNew.Visibility = Visibility.Collapsed;

            EditDay.Visibility = Visibility.Collapsed;
            DelDay.Visibility = Visibility.Collapsed;

            AddDay.Visibility = Visibility.Collapsed;
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
                AddDay.Visibility = Visibility.Collapsed;
                PosText.Text = "";
                TPSelSpan.Text = "";
                SelDayTimeList.Items.Clear();
                TimeSel.Items.Clear();
                try
                {
                    GetTimetables(ofd.FileName);
                    timetable_file = ofd.FileName;
                    timetables = GetTimetables(timetable_file).Timetables;
                    foreach (var timetable in timetables)
                    {
                        if (timetable.Date != null)
                            TimeSel.Items.Add(timetable.Date);
                        else if (timetable.Date == null && timetable.Weekday != null)
                            TimeSel.Items.Add(timetable.Weekday);
                        else TimeSel.Items.Add("无");
                    }
                    AddDay.Visibility = Visibility.Visible;
                    PosText.Text = ofd.FileName;
                }
                catch { MsgBox.Show("时间表结构不正确", "错误"); return; }
            }
        }

        private void TimeSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeSel.SelectedIndex < 0)
            {
                EditDay.Visibility = Visibility.Collapsed;
                DelDay.Visibility = Visibility.Collapsed;
                return;
            }
            SelDayTimeList.Items.Clear();
            TPSelSpan.Text = "";
            foreach (Table table in timetables[TimeSel.SelectedIndex].Tables)
            {
                SelDayTimeList.Items.Add($"{table.Start} ~ {table.End}");
            }
            EditDay.Visibility = Visibility.Visible;
            DelDay.Visibility = Visibility.Visible;
        }

        private void SelDayTimeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelDayTimeList.SelectedIndex < 0)
            {
                ControlPanel.Visibility = Visibility.Collapsed;
                TPDel.Visibility = Visibility.Collapsed;
                TPNew.Visibility = Visibility.Collapsed;
                return;
            }
            ControlPanel.Visibility = Visibility.Visible;
            TPDel.Visibility = Visibility.Visible;
            TPNew.Visibility = Visibility.Visible;

            TPStart.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start;
            TPEnd.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End;
            TPElement.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Name;
            TPNotice.Text = timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Notice;
            TPSelSpan.Text = $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].Start} ~ " +
                $"{timetables[TimeSel.SelectedIndex].Tables[SelDayTimeList.SelectedIndex].End}";
        }
    }
}
