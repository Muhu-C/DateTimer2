using DateTimer.WPF.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
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

        public class TimeConverter
        {
            /// <summary> 时间转为字符串 </summary>
            /// <param name="time">TimeSpan</param>
            /// <returns>HH:mm</returns>
            public static string Time2Str(TimeSpan time)
            {
                return $"{time.Hours:00}:{time.Minutes:00}";
            }

            public static string NumToWeekday(string num)
            {
                string numStr = "12345670";
                string chineseStr = "一二三四五六日日";
                int numIndex = numStr.IndexOf(num);
                if (numIndex > -1)
                    return chineseStr.Substring(numIndex, 1);
                return string.Empty;
            }
        }

        public class TimerShow
        {
            /// <summary> 获取目标距离时间 </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            public static string GetTargetTime(DateTime target)
            {
                DateTime nextMon = DateTime.Now.AddDays(8 - Convert.ToInt16(DateTime.Now.DayOfWeek));
                DateTime nextSun = DateTime.Now.AddDays(14 - Convert.ToInt16(DateTime.Now.DayOfWeek));

                if (target < DateTime.Now)
                    return "已到达";
                else if (target < nextMon)
                    return SettingsPage._appSetting.EnableTargetWeekday ?
                        $"本周{TimeConverter.NumToWeekday(Convert.ToInt32(target.DayOfWeek).ToString())}"
                        : $"{(target - DateTime.Today).TotalDays} 天后";
                else if (target >= nextMon && target <= nextSun)
                    return SettingsPage._appSetting.EnableTargetWeekday ?
                        $"下周{TimeConverter.NumToWeekday(Convert.ToInt32(target.DayOfWeek).ToString())}"
                        : $"{(target - DateTime.Today).TotalDays} 天后";
                else
                    return SettingsPage._appSetting.EnableTargetWeekday ?
                        $"{(target - DateTime.Today).TotalDays} 天后 周{TimeConverter.NumToWeekday(Convert.ToInt32(target.DayOfWeek).ToString())}"
                        : $"{(target - DateTime.Today).TotalDays} 天后";
            }
        }
    }

    public class SystemInfo
    {
        /// <summary>
        /// 获取 Windows 版本
        /// </summary>
        /// <returns>Windows 版本字符串</returns>
        public static string GetWinVer()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")) // 获取注册表目录
            {
                string productName = key.GetValue("ProductName") as string; // 系统名称（Win11不适用）
                try
                {
                    int majorVersion = (int)key.GetValue("CurrentMajorVersionNumber"); // 系统版本
                    var buildNumber = int.Parse(key.GetValue("CurrentBuildNumber").ToString()); // 构建(大于22000为Win11)

                    if (!string.IsNullOrEmpty(productName) && productName.ToLower().Contains("windows"))
                    {
                        if (majorVersion > 10 || majorVersion == 10 && buildNumber >= 22000)
                        {
                            if (majorVersion > 10) return "Windows " + majorVersion + " Build " + buildNumber;
                            else return "Windows 11 Build " + buildNumber;
                        }
                        else if (majorVersion == 10 && buildNumber < 22000) return "Windows 10 Build " + buildNumber;
                        else return productName;
                    }
                    else return "错误";
                }
                catch
                {
                    return productName;
                }
            }
        }

        /// <summary>
        /// 获取运行时版本
        /// </summary>
        /// <returns>.NET 版本</returns>
        public static string GetEnvVer()
        {
            try { return RuntimeInformation.FrameworkDescription; }
            catch (Exception e) { throw e; }
        }

        /// <summary>
        /// 获取系统位数
        /// </summary>
        /// <returns>64 或 32</returns>
        public static int GetBit()
        {
            if (Environment.Is64BitOperatingSystem) return 64;
            else return 32;
        }

        public static string GetCPUName()
        {
            string Name = string.Empty;
            using (ManagementObjectCollection moc = new ManagementClass("Win32_Processor").GetInstances())
            {
                foreach (ManagementObject mo in moc)
                    Name = mo["Name"].ToString();
            }
            return Name;
        }

        public static double GetRAMSize()
        {
            Process process = Process.GetCurrentProcess();
            return 1.0000*process.PrivateMemorySize64 / 1024 / 1024;
        }

        public static int GetTotalRAM()
        {
            long Size = -1;
            using (ManagementObjectCollection moc = new ManagementClass("Win32_PhysicalMemory").GetInstances())
            {
                foreach (ManagementObject mo in moc)
                    Size += Int64.Parse(mo.Properties["Capacity"].Value.ToString());
            }
            return (int)Math.Round(1.0000 * Size / 1024 / 1024, 0);
        }
    }

    public class AppSetting
    {
        public string Theme { get; set; } // 主题
        public bool EnableTarget { get; set; } // 目标时间
        public bool EnableTargetWeekday { get; set; }
        public bool EnableMainWindowShow { get; set; }
        #nullable enable
        public string? TargetDate { get; set; }
        public string? TargetName { get; set; }
        public string? TimeTablePath { get; set; } // 时间表路径
        #nullable restore
        public string BackDrop { get; set; } // 背景
        public bool EnableAdvancedNotice { get; set; } // 提前提醒
        public int AdvancedMinutes { get; set; }
    }
}
