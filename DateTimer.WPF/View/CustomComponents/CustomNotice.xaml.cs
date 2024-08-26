using System;
using System.ComponentModel;
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
        public BindContent Ctt = new BindContent();

        public CustomNotice()
        {
            // 初始化弹出窗口
            InitializeComponent();
            DataContext = Ctt;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 显示窗口行为
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom;
        }

        // 弹出提示
        public async void Init(string Title = "", string Text = "", string uri = "Data/Media/notice.wav")
        {
            Show();
            Ctt.NoticeText1 = Title;
            Ctt.NoticeText2 = Text;
            // 等待窗口加载完毕
            await Task.Run(async () => await Task.Delay(100));
            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                To = SystemParameters.WorkArea.Bottom - Height,
            };

            // 弹出窗口
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(uri, UriKind.Relative));
            player.Play();
            BeginAnimation(TopProperty, animation);

            // 关闭窗口
            await Task.Run(async() => await Task.Delay(5000));
            Close();
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            var animation2 = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.15)),
                To = SystemParameters.WorkArea.Bottom,
            };
            BeginAnimation(TopProperty, animation2);
            await Task.Run(async () => await Task.Delay(1000));

            Ctt.NoticeText1 = "";
            Ctt.NoticeText2 = "";
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
    }
}
