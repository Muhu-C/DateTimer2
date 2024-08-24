using iNKORE.UI.WPF.Modern;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            ReloadSettings();
            GetTime();
        }

        public void ReloadSettings()
        {
            if (SettingsPage._appSetting.EnableTarget == false)
                TarTimeText.Visibility = TarNameText.Visibility = Visibility.Hidden;
            else
                TarNameText.Visibility = TarTimeText.Visibility = Visibility.Visible;

            if (SettingsPage._appSetting.TargetDate == null)
                TarTimeText.Text = "未设置";
            else TarTimeText.Text = $"{SettingsPage._appSetting.TargetDate} " +
                    $"{Utils.TimerShow.GetTargetTime(DateTime.Parse(SettingsPage._appSetting.TargetDate))}";

            if (SettingsPage._appSetting.TargetName == null)
                TarNameText.Text = "目标时间";
            else TarNameText.Text = $"{SettingsPage._appSetting.TargetName}时间";
        }

        private async void GetTime()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        CurTimeText.Text = 
                        $"{DateTime.Now:yyyy/MM/dd} " +
                        $"周{Utils.TimeConverter.NumToWeekday(Convert.ToInt32(DateTime.Today.DayOfWeek).ToString())}";
                    });
                    await Task.Delay(10000);
                }
            });
        }

        private void GotoSetting_Click(object sender, RoutedEventArgs e)
        {
            var mw = Application.Current.Windows.Cast<Window>().
                FirstOrDefault(window => window is MainWindow) as MainWindow;
            mw.ContentFrame.Navigate(mw._settingsPage);
            mw.TitleText.Text = "设置";
            mw.Navigation.SelectedItem = mw.SettingButton;
        }

        private void ShowTimer_Click(object sender, RoutedEventArgs e)
        {
            if (App._timerWindow.IsVisible)
            {
                App._timerWindow.Hide();
                ShowTimer.Content = "显示时间表";
            }
            else
            {
                App._timerWindow.Show();
                ShowTimer.Content = "隐藏时间表";
            }
        }
    }
}
