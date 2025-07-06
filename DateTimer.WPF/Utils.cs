using DateTimer.WPF.View;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                using StreamWriter sw = new(Path, false, Encoding.UTF8); sw.Write(Text);
            }

            /// <summary> 用流读取文件 </summary>
            /// <param name="Path">文件路径</param>
            /// <returns></returns>
            public static string ReadFile(string Path)
            {
                using StreamReader sr = new(Path);
                return sr.ReadToEnd();
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

            public static Dictionary<string, string> weekMap = new Dictionary<string, string>
            {
                { "1", "周一" }, { "2", "周二" }, { "3", "周三" }, { "4", "周四" },
                { "5", "周五" }, { "6", "周六" }, { "7", "周日" }, { "0", "周日" }
            };

            public static string NumToWeekday(string num)
            {
                if ("12345670".IndexOf(num) > -1) return "一二三四五六日日".Substring("12345670".IndexOf(num), 1);
                return string.Empty;
            }

            /// <summary> 把json星期日转DateTime星期日 </summary>
            /// <param name="tweekday">周一为1,周日为7</param>
            /// <returns>周一为1,周日为0</returns>
            public static string TWeekdays2DWeekdays(string tweekday)
            {
                List<string> res = new();
                foreach (string wday in tweekday.Split('/'))
                    res.Add((Convert.ToInt32(wday) % 7).ToString());
                return string.Join("/", res);
            }

            /// <summary> 把DateTime星期日转json星期日 </summary>
            /// <param name="tweekday">周一为1,周日为0</param>
            /// <returns>周一为1,周日为7</returns>
            public static string DWeekdays2TWeekdays(string dweekday)
            {
                List<string> res = new();
                foreach (string wday in dweekday.Split('/'))
                    res.Add(Convert.ToInt32(wday) == 0 ? "7" : Convert.ToInt32(wday).ToString());
                return string.Join("/", res);
            }
        }
        /// <summary> 时间表处理 </summary>
        public class TimeTable
        {
            #region 时间表结构

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

            /// <summary>
            /// 将时间表 TimeTableFile 格式转为 json 并写入文件
            /// </summary>
            /// <param name="file"></param>
            /// <param name="Path"></param>
            public static void WriteTimeTables(TimeTableFile file, string Path)
            {
                FileProcess.WriteFile(JsonConvert.SerializeObject(file, Formatting.Indented), Path);
            }

            /// <summary>
            /// 判断时间表时间段是否错误
            /// </summary>
            /// <param name="tables"></param>
            /// <returns></returns>
            public static int IsTableInverted(List<Table> tables)
            {
                int ind = -1;
                foreach (Table table in tables)
                {
                    ind++;
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    TimeSpan end = TimeSpan.Parse(table.End);
                    if (start >= end) return ind;
                }
                return -1;
            }

            /// <summary>
            /// 判断时间表时间段间是否有冲突
            /// </summary>
            /// <param name="tables"></param>
            /// <returns></returns>
            public static List<int> IsTableSorted(List<Table> tables)
            {
                List<int> inds = new();
                int ind = -1;
                TimeSpan last = TimeSpan.Zero;
                foreach (Table table in tables)
                {
                    ind++;
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    if (start < last) inds.Add(ind);
                    TimeSpan end = TimeSpan.Parse(table.End);
                    last = end;
                }
                return inds;
            }

            /// <summary> 获取当前所在时间段 </summary>
            /// <param name="tables"> 时间段列表 </param>
            /// <returns>当前时间在时间段的下标</returns>
            public static int GetCurZone(List<Table> tables)
            {
                int i = 0;
                foreach (Table table in tables)
                {
                    TimeSpan start = TimeSpan.Parse(table.Start);
                    TimeSpan end = TimeSpan.Parse(table.End);
                    TimeSpan now = DateTime.Now.TimeOfDay;
                    if (now > start && now < end) return i;
                    i++;
                }
                return -1; // 没有在任何时间段内
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

            public enum TimeStatus
            {
                NotArrived,
                Notified,
                Arrived,
                None
            }

            /// <summary> 获取未完成列表 </summary>
            /// <param name="tables">时间表</param>
            /// <param name="advanceMinutes">提前提醒分钟数</param>
            /// <returns>TimeStatus 列表</returns>
            public static List<TimeStatus> GetTodayUndone(List<Table> tables, int advanceMinutes)
            {
                var result = new List<TimeStatus>();
                var now = DateTime.Now.TimeOfDay;
                foreach (var table in tables)
                {
                    var start = TimeSpan.Parse(table.Start);
                    if (now >= start)
                        result.Add(TimeStatus.Arrived);
                    else if (now >= start - TimeSpan.FromMinutes(advanceMinutes))
                        result.Add(TimeStatus.Notified); // 进入提前提醒区间
                    else
                        result.Add(TimeStatus.NotArrived);
                }
                return result;
            }

            /// <summary> 获取当天对应时间表 </summary>
            /// <param name="timetables">时间表类</param>
            /// <returns>索引</returns>
            public static Timetables GetTodayList(List<Timetables> timetables)
            {
                if (timetables == null || timetables.Count == 0) return null;
                int weekday = Convert.ToInt16(DateTime.Today.DayOfWeek); // 0 为周日

                // 优先查找今天的时间表
                foreach (Timetables t in timetables)
                    if (t.Date != null && DateTime.Parse(t.Date) == DateTime.Today)
                        return t;
                // 如果没有今天的时间表，则查找今天的星期日的时间表
                foreach (Timetables t in timetables)
                    if (t.Weekday != null && TimeConverter.TWeekdays2DWeekdays(t.Weekday).Contains(weekday.ToString()))
                        return t;
                return null;
            }

            #endregion

            #region 显示

            public enum DayDisplayMode
            {
                Weekday,
                Date,
                None
            }
            
            public static DayDisplayMode GetDayDisplayMode(Timetables timetables)
            {
                // 优先显示日期
                if (timetables.Date != null)
                    return DayDisplayMode.Date;
                else if(timetables.Weekday != null) return DayDisplayMode.Weekday;
                else return DayDisplayMode.None;
            }

            public static bool isOutdated(Timetables timetables)
            {
                if (timetables.Date != null)
                    return DateTime.Parse(timetables.Date) < DateTime.Today;
                else return false;
            }

            public static string DisplaySingleTimetable(Timetables timetables)
            {
                if (GetDayDisplayMode(timetables) == DayDisplayMode.Date)
                    return DateTime.Parse(timetables.Date).ToString("yyyy年M月d日");
                else if (GetDayDisplayMode(timetables) == DayDisplayMode.Weekday)
                {
                    // 区间合并
                    Dictionary<string, string> weekMap = TimeConverter.weekMap;
                    List<int> weekdays = timetables.Weekday.Split('/')
                        .Select(w => int.TryParse(w, out int n) ? n : -1)
                        .Where(n => n >= 1 && n <= 7)
                        .OrderBy(n => n)
                        .ToList();

                    var result = new List<string>();
                    int i = 0;
                    while (i < weekdays.Count)
                    {
                        int start = weekdays[i];
                        int end = start;
                        while (i + 1 < weekdays.Count && weekdays[i + 1] == end + 1)
                        {
                            end = weekdays[i + 1];
                            i++;
                        }
                        if (start == end) result.Add(weekMap[start.ToString()]);
                        else result.Add($"{weekMap[start.ToString()]} ~ {weekMap[end.ToString()]}");
                        i++;
                    }
                    return string.Join("、", result);
                }
                else return "未配置日期";
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


            private readonly static List<string> _verbs = new() // 动词表
            {
                "考", "看", "有", "听", "到",
                "写", "去", "存", "取", "读",
                "吃", "喝", "编", "找", "跳",
                "跑", "走", "退", "进", "来",
                "赶", "放", "开", "关", "能",
                "会", "拿", "丢", "做", "说",
                "开始", "结束", "停止", "复习", "预习", "到达"
            };

            /// <summary> 部分词语判断动词 </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string Str2Verb(string str)
            {
                foreach (string verb in _verbs)
                    if (str.Contains(verb) && !str.EndsWith("时间"))
                        return str;
                return $"到达{str}";
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
            string WinVer;
            switch ($"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}")
            {
                case "6.0":
                    WinVer = $"Windows Vista Build {Environment.OSVersion.Version.Build}"; break;
                case "6.1":
                    WinVer = $"Windows 7 Build {Environment.OSVersion.Version.Build}"; break;
                case "6.2":
                    WinVer = $"Windows 8 Build {Environment.OSVersion.Version.Build}"; break;
                case "6.3":
                    WinVer = $"Windows 8.1 Build {Environment.OSVersion.Version.Build}"; break;
                case "10.0":
                    if (Environment.OSVersion.Version.Build >= 22000) WinVer = $"Windows 11 Build {Environment.OSVersion.Version.Build}";
                    else WinVer = $"Windows 10 Build {Environment.OSVersion.Version.Build}";
                    break;
                default:
                    WinVer = $"Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor} Build {Environment.OSVersion.Version.Build}"; break;
            }
            return WinVer;
        }

        /// <summary>
        /// 获取运行时版本
        /// </summary>
        /// <returns>.NET 版本</returns>
        public static string GetEnvVer()
        {
            return RuntimeInformation.FrameworkDescription;
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
                foreach (ManagementObject mo in moc.Cast<ManagementObject>())
                    Name = mo["Name"].ToString();
            return Name;
        }

        public static double GetRAMSize()
        {
            return 1.0000 * Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;
        }

        public static int GetTotalRAM()
        {
            long Size = -1;
            using (ManagementObjectCollection moc = new ManagementClass("Win32_PhysicalMemory").GetInstances())
                foreach (ManagementObject mo in moc.Cast<ManagementObject>())
                    Size += Convert.ToInt64(mo.Properties["Capacity"].Value.ToString());
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
