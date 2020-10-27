using LogLib;
using MultithreadingScaffold;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace MD5Lib
{
    public class MD5
    {
        /// <summary>
        /// 获取指定文件的MD5值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string GetFileMD5(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                var retVal = md5.ComputeHash(file);
                file.Close();

                var sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                    sb.Append(retVal[i].ToString("x2"));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                WriteLog("GetMD5HashFromFile", ex.Message);
                return "";
            }
        }

        /// <summary>
        /// 获取指定目录下所有文件的列表,方便后续进行多线程处理
        /// </summary>
        /// <param name="folderPath">目录路径</param>
        /// <returns></returns>
        public static List<string> GetFolderFileList(string folderPath, ref List<string> list)
        {
            if (!Directory.Exists(folderPath))
                return null;

            var files = Directory.GetFiles(folderPath);
            if (files.Length > 0)
                foreach (var file in files)
                    list.Add(file);

            var dirs = Directory.GetDirectories(folderPath);
            if (dirs.Length > 0)
                foreach (var dir in dirs)
                    GetFolderFileList(dir, ref list);

            return list;
        }

        /// <summary>
        /// 使用多线程计算之前处理好的文件列表
        /// </summary>
        /// <param name="basePath">基础路径，列表中的路径会删除此路径部分以获得相对路径</param>
        /// <param name="files">已整理的文件列表</param>
        /// <returns></returns>
        public static Dictionary<string, string> CalcFileList(string basePath, List<string> files)
        {
            var listName = "md5_list.txt";
            if (File.Exists(listName))
                File.Delete(listName);

            File.AppendAllText(listName, $@"{basePath}{Environment.NewLine}");

            var dic = new Dictionary<string, string>();

            if (files.Count == 0)
                return dic;

            var mTScaffold = new MTScaffold();
            mTScaffold.Workload = files.Count;
            mTScaffold.ThreadLimit = Environment.ProcessorCount * 4;
            mTScaffold.Worker = (i) =>
            {
                var file = files[i];
                var md5 = GetFileMD5(file);

                lock (dic)
                {
                    var fileName = file.Replace(basePath, "").Replace('\\', '/');
                    dic.Add(fileName, md5);
                    File.AppendAllText(listName, $@"{fileName},{md5}{Environment.NewLine}");
                }
            };
            mTScaffold.Start();

            return dic;
        }

        /// <summary>
        /// 比较两个数据源，并返回一个体现比对结果的DataTable
        /// 返回DT列名依次是：结论（特有、不同、两者）；文件名；来源；MD5值
        /// </summary>
        /// <param name="source_local">本地源</param>
        /// <param name="source_remote">远程源</param>
        /// <param name="from_local">本地源来源</param>
        /// <param name="from_remote">远程源来源</param>
        /// <returns></returns>
        public static DataTable CompareTwoSource(Dictionary<string, string> source_local, Dictionary<string, string> source_remote, string from_local, string from_remote)
        {
            var dt = new DataTable();
            dt.Columns.Add("结论");
            dt.Columns.Add("文件名");
            dt.Columns.Add("来源");
            dt.Columns.Add("MD5值");

            foreach (var kv in source_local)
                if (!source_remote.ContainsKey(kv.Key))
                    dt.Rows.Add("特有", kv.Key, from_local, kv.Value);
                else if (source_remote[kv.Key] != source_local[kv.Key])
                    dt.Rows.Add("不同", kv.Key, "两者", kv.Value);

            foreach (var kv in source_remote)
                if (!source_local.ContainsKey(kv.Key))
                    dt.Rows.Add("特有", kv.Key, from_remote, kv.Value);

            return dt;
        }

        /// <summary>
        /// 写日志功能二级封装
        /// </summary>
        /// <param name="postion">出错位置</param>
        /// <param name="desc">出错信息</param>
        private static void WriteLog(string postion, string desc)
        {
            Log.GetInstance().Write("MD5", "MD5", postion, desc);
        }
    }
}
