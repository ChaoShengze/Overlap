using System;
using System.Collections.Generic;
using System.IO;

namespace LogLib
{
    public class Log
    {
        #region 单例模式

        private static Log Instance = null;
        private Log() { }
        public static Log GetInstance()
        {
            if (Instance == null)
                Instance = new Log();

            return Instance;
        }

        #endregion

        private readonly object LockObj = new object();

        /// <summary>
        /// 写入程序错误日志至文件中
        /// </summary>
        /// <param name="fileName">文件名，用于区分不同模块，已自动添加后缀名</param>
        /// <param name="title">出错类</param>
        /// <param name="position">出错方法</param>
        /// <param name="desc">详细信息</param>
        /// <param name="consoleLog">是否在控制台打印</param>
        public void Write(string fileName ,string title, string position, string desc, bool consoleLog = true)
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG", $@"{DateTime.Now.ToString("yyyy-MM-dd")}");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            lock (LockObj)
            {
                var f = File.AppendText(Path.Combine(logPath, $@"{fileName}.txt"));
                f.AutoFlush = true;

                var l = new List<string>();
                l.Add($@"==============={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}================");
                l.Add($@"TITLE  ：{title}");
                l.Add($@"POSTION：{position}");
                l.Add($@"DESC   ：{desc}");
                l.Add($@"==================================================");

                foreach (var str in l)
                {
                    f.WriteLine(str);

                    if (consoleLog)
                        Console.WriteLine(str);
                }

                f.Close();
                f.Dispose();
            }
        }

        /// <summary>
        /// 按带有日期的格式在控制台输出
        /// </summary>
        /// <param name="info"></param>
        /// <param name="noTime"></param>
        public void Out(string info, bool noTime = false)
        {
            var time = noTime ? ">>>>>>>>>>>>>>>>>>>" : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 若为notime则不带时间，但保持对齐
            Console.WriteLine($@">>> {time}：{info}");
        }
    }
}
