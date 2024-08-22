using iNKORE.UI.WPF.Modern;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if(ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            else ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }
    }
}
