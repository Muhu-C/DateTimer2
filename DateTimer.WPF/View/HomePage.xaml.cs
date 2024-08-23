using iNKORE.UI.WPF.Modern;
using System.Linq;
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
        }

        private void GotoSetting_Click(object sender, RoutedEventArgs e)
        {
            var mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            mw.ContentFrame.Navigate(mw._settingsPage);
            mw.TitleText.Text = "设置";
            mw.Navigation.SelectedItem = mw.SettingButton;
        }

        private void ShowTimer_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            else ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        }
    }
}
