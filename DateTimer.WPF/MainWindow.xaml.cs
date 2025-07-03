using DateTimer.WPF.View;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace DateTimer.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public HomePage _homePage = new ();
        public SettingsPage _settingsPage = new ();
        public TodoPage _todoPage = new ();
        public EditPage _editPage = new ();

        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Navigate(_homePage);
            TitleText.Text = "主页";
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
            // 设置背景样式
            switch (SettingsPage._appSetting.BackDrop)
            {
                case "Mica": WindowHelper.SetSystemBackdropType(this, BackdropType.Mica); break;
                case "MicaAlt": WindowHelper.SetSystemBackdropType(this, BackdropType.Tabbed); break;
                case "Acrylic": WindowHelper.SetSystemBackdropType(this, BackdropType.Acrylic11); break;
                case "None": WindowHelper.SetSystemBackdropType(this, BackdropType.None); break;
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
                    ContentFrame.Navigate(_settingsPage);
                    TitleText.Text = "设置";
                    break;
                case not null when pageType == typeof(TodoPage):
                    ContentFrame.Navigate(_todoPage);
                    TitleText.Text = "待办";
                    break;
                case not null when pageType == typeof(EditPage):
                    ContentFrame.Navigate(_editPage);
                    TitleText.Text = "编辑";
                    break;
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            ContentDialog closeDialog = new()
            {
                Title = "确定关闭应用?",
                Content = "按\"是\"关闭\n按\"否\"将窗口隐藏到托盘",
                PrimaryButtonText = "是",
                SecondaryButtonText = "否",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };
            var result = await closeDialog.ShowAsync();
            if (result == ContentDialogResult.Primary) Application.Current.Shutdown();
            else if (result == ContentDialogResult.Secondary) this.Hide();
        }
    }
}
