using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DateTimer.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        

        public App()
        {
            Startup += AppStartUp;
        }

        private void AppStartUp(object s, EventArgs e)
        {
            // 在此编写 Mutex 和 其他窗口的初始化
        }
    }
}
