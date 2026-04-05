using DateTimer.WPF.View;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using MsgBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace DateTimer.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 延迟创建页面，避免在字段初始化阶段触发页面的 InitializeComponent
        public HomePage _homePage = null;
        public SettingsPage _settingsPage = null;
        public TodoPage _todoPage = null;
        public EditPage _editPage = null;

        public MainWindow()
        {
            InitializeComponent();

            // 仅立即创建需要的页面（HomePage），其它按需创建
            _homePage = new HomePage();

            ContentFrame.Navigate(_homePage);
            TitleText.Text = "主页";
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
            // 设置背景样式
            switch (SettingsPage._appSetting.BackDrop)
            {
                case "Mica": 
                    WindowHelper.SetSystemBackdropType(this, BackdropType.Mica);
                    MsgBox.DefaultBackdropType = BackdropType.Mica;
                    break;
                case "MicaAlt":
                    WindowHelper.SetSystemBackdropType(this, BackdropType.Tabbed);
                    MsgBox.DefaultBackdropType = BackdropType.Tabbed;
                    break;
                case "Acrylic":
                    WindowHelper.SetSystemBackdropType(this, BackdropType.Acrylic11);
                    MsgBox.DefaultBackdropType = BackdropType.Acrylic11;
                    break;
                case "None":
                    WindowHelper.SetSystemBackdropType(this, BackdropType.None);
                    MsgBox.DefaultBackdropType = BackdropType.None;
                    break;
            }
            InitUI();
        }
        public async void InitUI()
        {
            await Task.Delay(200);
            TitleBarGrid.Visibility = Visibility.Visible;
            Navigation.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
            animation.Completed += (s, e) => SplashScreenGrid.Visibility = Visibility.Collapsed;
            SplashScreenGrid.BeginAnimation(UIElement.OpacityProperty, animation);
            await Task.Delay(1000);
            SplashScreenGrid.Visibility = Visibility.Collapsed;
        }

        // 设置主题色
        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
                ThemeManager.Current.AccentColor = SystemParameters.WindowGlassColor;
        }

        // 切换页面
        private void NavigationItemChanged(NavigationView s, NavigationViewItemInvokedEventArgs e)
        {
            if (e.InvokedItemContainer == null) return;
            Type pageType = Type.GetType(e.InvokedItemContainer.Tag.ToString());
            switch (pageType)
            {
                case not null when pageType == typeof(HomePage):
                    ContentFrame.Navigate(_homePage);
                    TitleText.Text = "主页";
                    break;
                case not null when pageType == typeof(SettingsPage):
                    if (_settingsPage == null) _settingsPage = new SettingsPage();
                    ContentFrame.Navigate(_settingsPage);
                    TitleText.Text = "设置";
                    break;
                case not null when pageType == typeof(TodoPage):
                    if (_todoPage == null) _todoPage = new TodoPage();
                    ContentFrame.Navigate(_todoPage);
                    TitleText.Text = "待办";
                    break;
                case not null when pageType == typeof(EditPage):
                    if (_editPage == null) _editPage = new EditPage();
                    ContentFrame.Navigate(_editPage);
                    TitleText.Text = "编辑";
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // 取消关闭事件
            var result = MsgBox.Show("按\"是\"关闭应用\n按\"否\"隐藏到托盘", "是否关闭应用?", MessageBoxButton.YesNoCancel, SegoeFluentIcons.IncidentTriangle);
            if (result == MessageBoxResult.Yes) Application.Current.Shutdown();
            else if (result == MessageBoxResult.No) this.Hide();
        }
    }
}