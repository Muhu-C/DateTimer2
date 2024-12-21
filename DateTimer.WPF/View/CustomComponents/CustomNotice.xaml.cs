using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DateTimer.WPF.View.CustomComponents
{
    /// <summary>
    /// CustomNotice.xaml 的交互逻辑
    /// </summary>
    public partial class CustomNotice : Window
    {
        public BindContent Ctt = new ();
        private int lasttime = 0;
        private readonly MediaPlayer player = new();

        public CustomNotice()
        {
            InitializeComponent();
            DataContext = Ctt;
        }

        // 弹出提示
        public async void Init(string Title = "", string Text = "", string uri = "Data/Media/notice.wav")
        {
            lasttime = 5;
            player.Open(new Uri(uri, UriKind.Relative));
            player.Play();
            // 显示窗口行为
            (Application.Current.MainWindow as MainWindow)._homePage.NoticesText.Text += 
                $"{Title} : {Text} - {DateTime.Now.Hour}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}\n";
            if (IsVisible)
            {
                Ctt.NoticeText2 += $"\n{Text}";
                ScrollV.ScrollToEnd();
                OutlineBorder.Background = Brushes.Yellow;
                await Task.Run(async () => await Task.Delay(100));
                OutlineBorder.Background = SystemParameters.WindowGlassBrush;
                await Task.Run(async () => await Task.Delay(100));
                OutlineBorder.Background = Brushes.Yellow;
                await Task.Run(async () => await Task.Delay(100));
                OutlineBorder.Background = SystemParameters.WindowGlassBrush;
            }
            else
            {
                Left = SystemParameters.WorkArea.Right - Width - 5;
                Top = SystemParameters.WorkArea.Bottom - Height - 5;
                Opacity = 0;
                Show();
                Ctt.NoticeText1 = Title;
                Ctt.NoticeText2 = $"{Text}";
                OutlineBorder.Background = SystemParameters.WindowGlassBrush; // 设置窗口边框
                                                                              // 等待窗口加载完毕
                await Task.Run(async () => await Task.Delay(50));
                var animation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.2)), To = 1 };

                // 弹出窗口
                BeginAnimation(OpacityProperty, animation);
                while (lasttime > 0)
                {
                    lasttime -= 1;
                    await Task.Run(async () => await Task.Delay(1000));
                }
                animation = null;
                Close();
            }
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            var animation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.2)), To = 0 };
            BeginAnimation(OpacityProperty, animation);
            await Task.Run(async () => await Task.Delay(300));

            Ctt.NoticeText1 = "";
            Ctt.NoticeText2 = "";
            animation = null;
            Hide();
        }

        public class BindContent : INotifyPropertyChanged // 通过 Foreground Binding 实时设置页面文本颜色
        {
            private string ntxt1;
            public string NoticeText1
            {
                get { return ntxt1; }
                set { if (ntxt1 != value) { ntxt1 = value; OnPropertyChanged("NoticeText1"); } }
            }

            private string ntxt2;
            public string NoticeText2
            {
                get { return ntxt2; }
                set { if (ntxt2 != value) { ntxt2 = value; OnPropertyChanged("NoticeText2"); } }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App._timerWindow != null && App._timerWindow.Visibility != Visibility.Visible)
            {
                App._timerWindow.Show();
                try
                {
                    (Application.Current.Windows.Cast<Window>().
                    FirstOrDefault(window => window is MainWindow) as MainWindow).
                    _homePage.ShowTimer.Content = "隐藏时间表";
                }
                catch {; }
            }
        }
    }
}
