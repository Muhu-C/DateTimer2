using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static DateTimer.WPF.Utils.TimeTable;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace DateTimer.WPF.View.CustomComponents
{
    /// <summary>
    /// EditTimetableDay.xaml 的交互逻辑
    /// </summary>
    public partial class EditTimetableDay : ContentDialog
    {
        public EditTimetableDay()
        {
            InitializeComponent();
            

        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string weekday = "";
            if (SelAll.IsChecked == true) weekday = "/1/2/3/4/5/6/7";
            else if (SelAll.IsChecked == false) weekday = "/";
            else
            {
                if (Sel1.IsChecked == true) weekday += "/1";
                if (Sel2.IsChecked == true) weekday += "/2";
                if (Sel3.IsChecked == true) weekday += "/3";
                if (Sel4.IsChecked == true) weekday += "/4";
                if (Sel5.IsChecked == true) weekday += "/5";
                if (Sel6.IsChecked == true) weekday += "/6";
                if (Sel7.IsChecked == true) weekday += "/7";
            }
            weekday = weekday.Remove(0, 1);

            if (Title.ToString() == "新建时间表")
            {
                EditPage.timetables ??= new List<Timetables>();
                EditPage.timetables.Add(new Timetables()
                {
                    Date = (SelDate.SelectedDate == null ? null : $"{SelDate.SelectedDate:yyyy/MM/dd}"),
                    Weekday = (weekday == "" ? null : weekday),
                    Tables = new List<Utils.TimeTable.Table>()
                });
            }
            else
            {
                EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday = (weekday == "" ? null : weekday);
                EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Date = (SelDate.SelectedDate == null ? null : $"{SelDate.SelectedDate:yyyy/MM/dd}");
            }
            (Application.Current.MainWindow as MainWindow)._editPage.TimeSel.Items.Clear();
            if (EditPage.timetables != null)
                foreach (var timetable in EditPage.timetables)
                {
                    if (isOutdated(timetable))
                        (App.Current.MainWindow as MainWindow)._editPage.TimeSel.Items
                            .Add(new TextBlock { Foreground = Brushes.Orange, 
                                Text = DisplaySingleTimetable(timetable), ToolTip = new TextBlock 
                                { Foreground = Brushes.Orange, Text = "该时间表已过期！" } });

                    else
                        (App.Current.MainWindow as MainWindow)._editPage.TimeSel.Items.Add(DisplaySingleTimetable(timetable));
                }
        }

        private void SelAll_Checked(object sender, RoutedEventArgs e)
        { Sel1.IsChecked = Sel2.IsChecked = Sel3.IsChecked = Sel4.IsChecked = Sel5.IsChecked = Sel6.IsChecked = Sel7.IsChecked = true; }
        private void SelAll_Unchecked(object sender, RoutedEventArgs e)
        { Sel1.IsChecked = Sel2.IsChecked = Sel3.IsChecked = Sel4.IsChecked = Sel5.IsChecked = Sel6.IsChecked = Sel7.IsChecked = false; }
        private void UpdateState()
        {
            if (Sel1.IsChecked == true && Sel2.IsChecked == true &&
                Sel3.IsChecked == true && Sel4.IsChecked == true &&
                Sel5.IsChecked == true && Sel6.IsChecked == true &&
                Sel7.IsChecked == true)
            {
                SelDate.SelectedDate = null;
                SelDate.IsEnabled = false;
                SelAll.IsChecked = true;
            }
            else if (Sel1.IsChecked == false && Sel2.IsChecked == false &&
                     Sel3.IsChecked == false && Sel4.IsChecked == false &&
                     Sel5.IsChecked == false && Sel6.IsChecked == false &&
                     Sel7.IsChecked == false)
            {
                SelDate.SelectedDate = null;
                SelDate.IsEnabled = true;
                SelAll.IsChecked = false;
            }
            else
            {
                SelDate.SelectedDate = null;
                SelDate.IsEnabled = false;
                SelAll.IsChecked = null;
            }
        }
        private void SelAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (Sel1.IsChecked == true && Sel2.IsChecked == true &&
                Sel3.IsChecked == true && Sel4.IsChecked == true &&
                Sel5.IsChecked == true && Sel6.IsChecked == true &&
                Sel7.IsChecked == true) 
                SelAll.IsChecked = false;
        }
        private void Sel_Checked(object sender, RoutedEventArgs e) { UpdateState(); }
        private void Sel_Unchecked(object sender, RoutedEventArgs e) { UpdateState(); }

        private void ClearDateBtn_Click(object sender, RoutedEventArgs e)
        {
            SelDate.SelectedDate = null;
        }

        private void SelDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelDate.SelectedDate == null)
            {
                ClearDateBtn.IsEnabled = false;
                SelAll.IsEnabled = Sel1.IsEnabled = Sel2.IsEnabled = Sel3.IsEnabled = Sel4.IsEnabled = Sel5.IsEnabled = Sel6.IsEnabled = Sel7.IsEnabled = true;
            }
            else
            {
                ClearDateBtn.IsEnabled = true;
                SelAll.IsEnabled = Sel1.IsEnabled = Sel2.IsEnabled = Sel3.IsEnabled = Sel4.IsEnabled = Sel5.IsEnabled = Sel6.IsEnabled = Sel7.IsEnabled = false;
            }
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (Title.ToString() == "新建时间表")
            {
                ClearDateBtn.IsEnabled = false;
                Sel1.IsChecked = Sel2.IsChecked = Sel3.IsChecked = Sel4.IsChecked = Sel5.IsChecked = Sel6.IsChecked = Sel7.IsChecked = false;
                SelDate.SelectedDate = null;
            }
            else if ((Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex != -1)
            {
                if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday != null)
                {
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("1")) Sel1.IsChecked = true;
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("2")) Sel2.IsChecked = true;
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("3")) Sel3.IsChecked = true;
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("4")) Sel4.IsChecked = true;
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("5")) Sel5.IsChecked = true;
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("6")) Sel6.IsChecked = true;
                    if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Weekday.Contains("7")) Sel7.IsChecked = true;
                }
                if (EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Date != null)
                    SelDate.SelectedDate = DateTime.Parse(EditPage.timetables[(Application.Current.MainWindow as MainWindow)._editPage.TimeSel.SelectedIndex].Date);
                else
                {
                    ClearDateBtn.IsEnabled = false;
                    SelDate.SelectedDate = null;
                }
            }
        }
    }
}
