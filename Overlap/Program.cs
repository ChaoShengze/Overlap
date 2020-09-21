using Newtonsoft.Json;
using System.IO;

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
                throw new System.Exception("监视路径必须和目标路径一一对应 \\ Number of target folders should be same as source folder");
        }

        /// <summary>
        /// 启动监视线程 \
        /// Start wathcing task.
        /// </summary>
        private static void StartWatchingThreads()
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

            #endregion
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!CheckExtName(e))
                return;

            var target = GetTargetPath(e);
            if (target == null)
                return;
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!CheckExtName(e))
                return;

            var target = GetTargetPath(e);
            if (target == null)
                return;
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!CheckExtName(e))
                return;

            var target = GetTargetPath(e);
            if (target == null)
                return;
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
    }
}
