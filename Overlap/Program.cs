using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace Overlap
{
    class Program
    {
        /// <summary>
        /// 固定的配置文件的名称 \
        /// Name of configuration file.
        /// </summary>
        private readonly static string ConfigFileName = "config.json";
        /// <summary>
        /// 从配置文件中读取的配置对象 \
        /// Config object which loaded from configuration file.
        /// </summary>
        private static IConfig Config;

        static void Main(string[] args)
        {
            LoadConfig();
            StartWatchingThreads();

            while (true)
            {

            }
        }

        /// <summary>
        /// 读取配置文件 \
        /// Load configuration file.
        /// </summary>
        private static void LoadConfig()
        {
            Config = JsonConvert.DeserializeObject<IConfig>(File.ReadAllText(ConfigFileName));

            if (Config.Folder.Length != Config.Target.Length)
                throw new Exception("监视路径必须和目标路径一一对应 \\ Number of target folders should be same as source folder");
        }

        /// <summary>
        /// 启动监视线程 \
        /// Start wathcing task.
        /// </summary>
        private static void StartWatchingThreads()
        {
            if (Config.WorkMode == 0)
            {
                foreach (var folder in Config.Folder)
                {
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = folder;
                    watcher.Filter = "*.*";

                    watcher.Changed += Watcher_Changed;
                    watcher.Created += Watcher_Created;
                    watcher.Deleted += Watcher_Deleted;
                    watcher.Renamed += Watcher_Renamed;

                    watcher.IncludeSubdirectories = true;
                    watcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        Wathcer_Passivity();
                        Thread.Sleep(Config.WorkMode * 60 * 1000);
                    }
                });
                thread.IsBackground = true;
                thread.Name = "OverlapWatcher";
                thread.Start();
            }
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            #region 通用处理

            if (!CheckExtName(e))
                return;

            var path = GetTargetPath(e);
            if (path == null)
                return;

            if (!Directory.Exists(path[1]))
                Directory.CreateDirectory(path[1]);

            var extPath = Path.GetDirectoryName(e.FullPath).Replace(path[0], "");
            if (extPath.StartsWith("\\"))
                extPath = extPath.Remove(0, 1);

            var realPath = Path.Combine(path[1], extPath, e.Name);
            var lastPath = Path.Combine(path[1], extPath, e.OldName);

            #endregion

            if (File.Exists(lastPath))
            {
                File.Delete(lastPath);
                Log.GetInstance().Write("FileLog", "Rename", "Rename", $@"del {lastPath}");
            }

            CopyFile(e.FullPath, realPath, "Rename");
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            #region 通用处理

            if (!CheckExtName(e))
                return;

            var path = GetTargetPath(e);
            if (path == null)
                return;

            if (!Directory.Exists(path[1]))
                Directory.CreateDirectory(path[1]);

            var extPath = Path.GetDirectoryName(e.FullPath).Replace(path[0], "");
            if (extPath.StartsWith("\\"))
                extPath = extPath.Remove(0, 1);

            var realPath = Path.Combine(path[1], extPath, e.Name);

            #endregion

            if (File.Exists(realPath))
            {
                File.Delete(realPath);
                Log.GetInstance().Write("FileLog", "Delete", "Delete", $@"del {realPath}");
            }
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            #region 通用处理

            if (!CheckExtName(e))
                return;

            var path = GetTargetPath(e);
            if (path == null)
                return;

            if (!Directory.Exists(path[1]))
                Directory.CreateDirectory(path[1]);

            var extPath = Path.GetDirectoryName(e.FullPath).Replace(path[0], "");
            if (extPath.StartsWith("\\"))
                extPath = extPath.Remove(0, 1);

            var realPath = Path.Combine(path[1], extPath, e.Name);

            #endregion

            Thread.Sleep(5000);

            CopyFile(e.FullPath, realPath, "Create");
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            #region 通用处理

            if (!CheckExtName(e))
                return;

            var path = GetTargetPath(e);
            if (path == null)
                return;

            if (!Directory.Exists(path[1]))
                Directory.CreateDirectory(path[1]);

            var extPath = Path.GetDirectoryName(e.FullPath).Replace(path[0], "");
            if (extPath.StartsWith("\\"))
                extPath = extPath.Remove(0, 1);

            var realPath = Path.Combine(path[1], extPath, e.Name);

            #endregion

            CopyFile(e.FullPath, realPath, "Change");
        }

        private static void Wathcer_Passivity()
        { 
        
        }

        /// <summary>
        /// 检查发生变化的文件的后缀名，若在忽略列表中就返回false。 \
        /// Check file's ext name which has changed.If ignore list contain it, return false.
        /// </summary>
        /// <param name="e">变化文件的消息对象 \ Message obj of changed file</param>
        /// <returns></returns>
        private static bool CheckExtName(FileSystemEventArgs e)
        {
            var boolean = true;

            foreach (var ext in Config.Filter)
                if (Path.GetExtension(e.FullPath) == ext)
                    return false;

            return boolean;
        }

        /// <summary>
        /// 根据变化文件的路径来获取目标路径 \
        /// Get target folder from the path of changed file
        /// </summary>
        /// <param name="e">变化文件的消息对象 \ Message obj of changed file</param>
        /// <returns></returns>
        private static string[] GetTargetPath(FileSystemEventArgs e)
        {
            string[] path = null;

            for (int i = 0; i < Config.Folder.Length; i++)
            {
                var folder = Config.Folder[i];
                if (e.FullPath.StartsWith(folder))
                {
                    path = new string[] { Config.Folder[i], Config.Target[i] };
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// 复制文件 \ Copy files
        /// </summary>
        /// <param name="from">来源</param>
        /// <param name="to">目标</param>
        private static void CopyFile(string from, string to, string events)
        {
            try
            {
                if (File.Exists(to))
                    File.Delete(to);

                var fromFile = new FileStream(from, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var toFile = new FileStream(to, FileMode.OpenOrCreate);

                var buffer = new byte[fromFile.Length];
                fromFile.Read(buffer, 0, buffer.Length);
                toFile.Write(buffer, 0, buffer.Length);

                fromFile.Close();
                toFile.Flush();
                toFile.Close();

                Log.GetInstance().Write("FileLog", "CopyFile", events, $@"{from} => {to}");
            }
            catch (Exception ex)
            {
                Log.GetInstance().Write("FileLog", "ERROR", events, $@"{ex.Message}");
            }
        }
    }
}
