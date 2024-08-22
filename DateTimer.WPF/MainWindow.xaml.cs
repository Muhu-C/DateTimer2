using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DateTimer.WPF.View;

namespace DateTimer.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public HomePage _homePage = new HomePage();
        public SettingsPage _settingsPage = new SettingsPage();
        public TodoPage _todoPage = new TodoPage();
        public EditPage _editPage = new EditPage();

        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Navigate(_homePage);
            TitleText.Text = "主页";
        }

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
            ContentDialog closeDialog = new ContentDialog
            {
                Title = "确定关闭程序?",
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
