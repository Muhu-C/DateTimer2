using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateTimer.WPF
{
    public class Utils
    {
        /// <summary> 文件流处理 </summary>
        public class FileProcess // 文件流处理
        {
            /// <summary> 用流写入文件 </summary>
            /// <param name="Text">字符串</param>
            /// <param name="Path">存放位置</param>
            public static void WriteFile(string Text, string Path)
            {
                using (StreamWriter sw = new StreamWriter(Path, false, Encoding.UTF8))
                {
                    sw.Write(Text);
                }
            }

            /// <summary> 用流读取文件 </summary>
            /// <param name="Path">文件路径</param>
            /// <returns></returns>
            public static string ReadFile(string Path)
            {
                using (StreamReader sr = new StreamReader(Path))
                {
                    string content;
                    content = sr.ReadToEnd();
                    return content;
                }
            }
        }
    }

    public class AppSetting
    {
        public string Theme { get; set; } // 主题
        public bool EnableTarget { get; set; } // 目标时间
        public string TargetDate { get; set; }
        #nullable enable
        public string? TargetName { get; set; }
        public string? TimeTablePath { get; set; } // 时间表路径
        #nullable restore
        public string BackDrop { get; set; } // 背景
        public bool EnableAdvancedNotice { get; set; } // 提前提醒
        public int AdvancedMinutes { get; set; }
    }
}
