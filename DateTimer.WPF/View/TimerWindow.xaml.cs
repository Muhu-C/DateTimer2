using iNKORE.UI.WPF.Modern;
using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace DateTimer.WPF.View
{
    /// <summary>
    /// TimerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimerWindow : Window
    {
        public TimerWindow()
        {
            InitializeComponent();
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
        }

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
            var mw = Application.Current.Windows.Cast<Window>().
                FirstOrDefault(window => window is MainWindow) as MainWindow;
            mw._homePage.ShowTimer.Content = "显示时间表";
        }
    }
}
