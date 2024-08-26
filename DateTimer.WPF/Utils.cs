﻿using DateTimer.WPF.View;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

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

            /// <summary> 将 json 字符串格式化 </summary>
            /// <param name="oldjson">单行 json 字符串</param>
            /// <returns>格式化后的 json 字符串</returns>
            public static string Json_Optimization(string oldjson)
            {
                int l = 0, k = 0;
                bool isInString = false;
                string newjson = string.Empty;
                foreach (char c in oldjson)
                {
                    if (c == '\"') { if (!isInString) isInString = true; else isInString = false; }
                    newjson += c;
                    if (c == '{' && !isInString)
                    {
                        l++;
                        newjson += '\n';
                        for (int i = 1; i <= l; i++) newjson += "    ";
                    }
                    else if (oldjson.Length > k + 1 && oldjson[k + 1] == '}' && !isInString)
                    {
                        l--;
                        newjson += '\n';
                        for (int i = 1; i <= l; i++) newjson += "    ";
                    }
                    else if (c == ',' && !isInString)
                    {
                        newjson += '\n';
                        for (int i = 1; i <= l; i++) newjson += "    ";
                    }
                    k++;
                }
                return newjson;
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

            /// <summary> 把json星期日转DateTime星期日 </summary>
            /// <param name="tweekday">周一为1,周日为7</param>
            /// <returns>周一为1,周日为0</returns>
            public static string TWeekdays2DWeekdays(string tweekday)
            {
                List<string> res = new List<string>();
                string[] wdays = tweekday.Split('/');
                foreach (string wday in wdays)
                {
                    int wd = Convert.ToInt32(wday);
                    res.Add((wd % 7).ToString());
                }

                return string.Join("/", res);
            }

            /// <summary> 把DateTime星期日转json星期日 </summary>
            /// <param name="tweekday">周一为1,周日为0</param>
            /// <returns>周一为1,周日为7</returns>
            public static string DWeekdays2TWeekdays(string dweekday)
            {
                List<string> res = new List<string>();
                string[] wdays = dweekday.Split('/');
                foreach (string wday in wdays)
                {
                    int wd = Convert.ToInt32(wday);
                    res.Add(wd == 0 ? "7" : wd.ToString());
                }

                return string.Join("/", res);
            }
        }
        /// <summary> 时间表处理 </summary>
        public class TimeTable
        {
            #region timetable-JSON

            /// <summary> json 反序列化的类 </summary>
            public class TimeTableFile // json第一层
            {
                public List<Timetables> Timetables { get; set; } // 第二层
            }

            /// <summary> 时间表列表类 </summary>
            public class Timetables // json第二层
            {
                #nullable enable
                public string? Date { get; set; }
                public string? Weekday { get; set; }
                #nullable disable
                public List<Table> Tables { get; set; } // 第三层
            }

            /// <summary> 时间表类 </summary>
            public class Table // json第三层
            {
                public string Name { get; set; }
                public string Start { get; set; }
                public string End { get; set; }
                #nullable enable
                public string? Notice { get; set; }
                #nullable disable
            }
            #endregion



            #region 处理
            /// <summary> 反序列化时间表 json 文件 </summary>
            /// <param name="Path">json 位置</param>
            /// <returns>时间表类</returns>
            public static TimeTableFile GetTimetables(string Path)
            {
                if (Path == null) return JsonConvert.DeserializeObject<TimeTableFile>(FileProcess.ReadFile(App.DefTimetablePath));
                return JsonConvert.DeserializeObject<TimeTableFile>(FileProcess.ReadFile(Path));
            }

            public static bool isTableShowable(List<Table> tables)
            {
                foreach (Table table in tables)
                {
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    TimeSpan end = TimeSpan.Parse(table.End);
                    if (start >= end)
                        return false;
                }
                return true;
            }

            /// <summary> 获取当前所在时间段 </summary>
            /// <param name="table"></param>
            /// <returns>当前时间在时间段的下标</returns>
            public static List<int> GetCurZone(List<Table> tables)
            {
                List<int> indexes = new List<int>();
                int i = 0;
                foreach (Table table in tables)
                {
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    TimeSpan end = TimeSpan.Parse(table.End);
                    TimeSpan now = DateTime.Now.TimeOfDay;
                    if (now > start && now < end) indexes.Add(i);
                    i++;
                }
                return indexes;
            }

            /// <summary> 判断是否到点 </summary>
            /// <param name="tables"></param>
            /// <returns></returns>
            public static int IsStart(List<Table> tables, TimeSpan front)
            {
                int i = 0;
                foreach (Table table in tables)
                {
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    TimeSpan now = DateTime.Now.TimeOfDay + front;
                    if (start.Hours == now.Hours && start.Minutes == now.Minutes)
                        return i;
                    i++;
                }
                return -1;
            }

            /// <summary> 获取未完成列表 </summary>
            /// <param name="tables">时间表</param>
            /// <returns>int 值列表 1为未到时间 2为在五分钟外</returns>
            public static List<int> GetTodayUndone(List<Table> tables)
            {
                List<int> result = new();
                foreach (Table table in tables)
                {
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    TimeSpan now = DateTime.Now.TimeOfDay;
                    if ((int)start.TotalMinutes >= (int)now.TotalMinutes) result.Add(2);
                    else result.Add(0);
                }
                return result;
            }

            /// <summary> 获取当天对应时间表 </summary>
            /// <param name="timetables">时间表类</param>
            /// <returns>索引</returns>
            public static Timetables GetTodayList(List<Timetables> timetables)
            {
                if (timetables == null) return null;
                if (timetables.Count == 0) return null;
                int weekday = Convert.ToInt16(DateTime.Today.DayOfWeek); // 0 为周日

                foreach (Timetables t in timetables)
                {
                    if (t.Date != null)
                    {
                        if (DateTime.Parse(t.Date) == DateTime.Today) return t;
                    }
                    else if (t.Date == null && t.Weekday != null)
                    {
                        if (TimeConverter.TWeekdays2DWeekdays(t.Weekday).Contains(weekday.ToString())) return t;
                    }
                    else return null;
                }
                return null;
            }
            #endregion
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

            /// <summary> 获取目标距离时间 </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            public static string TimetableShowTargetTime(DateTime target, string targetName)
            {
                DateTime nextMon = DateTime.Now.AddDays(8 - Convert.ToInt16(DateTime.Now.DayOfWeek));
                DateTime nextSun = DateTime.Now.AddDays(14 - Convert.ToInt16(DateTime.Now.DayOfWeek));

                if (target < DateTime.Now)
                    return $"已{Str2Verb(targetName)} 今天是 {DateTime.Today.Month}月{DateTime.Today.Day}日 " +
                           $"星期{TimeConverter.NumToWeekday(Convert.ToInt16(DateTime.Today.DayOfWeek).ToString())}";
                else if (target < nextMon)
                    return SettingsPage._appSetting.EnableTargetWeekday ?
                        $"本周{TimeConverter.NumToWeekday(Convert.ToInt32(target.DayOfWeek).ToString())}将{Str2Verb(targetName)} 还有" +
                        $" {(int)(target - DateTime.Now).TotalDays}天 {(target - DateTime.Now).Hours}时" +
                        $" {(target - DateTime.Now).Minutes}分 {(target - DateTime.Now).Seconds}秒"
                        : $"距{Str2Verb(targetName)}还有 {(int)(target - DateTime.Now).TotalDays}天 {(target - DateTime.Now).Hours}时" +
                        $" {(target - DateTime.Now).Minutes}分 {(target - DateTime.Now).Seconds}秒";
                else if (target >= nextMon && target <= nextSun)
                    return SettingsPage._appSetting.EnableTargetWeekday ?
                        $"下周{TimeConverter.NumToWeekday(Convert.ToInt32(target.DayOfWeek).ToString())}将{Str2Verb(targetName)} 还有" +
                        $" {(int)(target - DateTime.Now).TotalDays}天 {(target - DateTime.Now).Hours}时" +
                        $" {(target - DateTime.Now).Minutes}分 {(target - DateTime.Now).Seconds}秒"
                        : $"距{Str2Verb(targetName)}还有 {(int)(target - DateTime.Now).TotalDays}天 {(target - DateTime.Now).Hours}时" +
                        $" {(target - DateTime.Now).Minutes}分 {(target - DateTime.Now).Seconds}秒";
                else
                    return $"距{Str2Verb(targetName)}还有 {(int)(target - DateTime.Now).TotalDays}天 {(target - DateTime.Now).Hours}时" +
                        $" {(target - DateTime.Now).Minutes}分 {(target - DateTime.Now).Seconds}秒";
            }

            /// <summary> 部分词语判断动词 </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string Str2Verb(string str)
            {
                if (str.Contains("考") || str.Contains("看") ||
                    str.Contains("有") || str.Contains("听") ||
                    str.Contains("到") || str.Contains("写") ||
                    str.Contains("去") || str.Contains("存") ||
                    str.Contains("开始") || str.Contains("读") ||
                    str.Contains("结束") || str.Contains("编") ||
                    str.Contains("复习") || str.Contains("喝") ||
                    str.Contains("预习") || str.Contains("吃") ||
                    str.Contains("找") || str.Contains("跳") ||
                    str.Contains("退") || str.Contains("赶") ||
                    str.Contains("进") || str.Contains("上") ||
                    str.Contains("跑") || str.Contains("下") ||
                    str.Contains("取") || str.Contains("放") ||
                    str.Contains("开") || str.Contains("关") ||
                    str.Contains("能") || str.Contains("会") ||
                    str.Contains("拿") || str.Contains("做") ||
                    str.Contains("说") || str.Contains("到达"))
                    return str;
                else return $"到达{str}";
            }

            public static TableSource Table2Entry(TimeTable.Table table)
            {
                return new TableSource
                {
                    Title = table.Name,
                    Notice = table.Notice ?? string.Empty,
                    Time = $"{table.Start} ~ {table.End}"
                };
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
