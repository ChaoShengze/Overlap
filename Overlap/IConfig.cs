using System;
using System.Collections.Generic;
using System.Text;

namespace Overlap
{
    /// <summary>
    /// 程序配置结构体 \
    /// Struct of configuration file
    /// </summary>
    public class IConfig
    {
        /// <summary>
        /// 需要监视的目录 \
        /// Folder to watch
        /// </summary>
        public string[] Folder;
        /// <summary>
        /// 监视目录对应的目标目录 \
        /// Folder which will be root of synchronization
        /// </summary>
        public string[] Target;
        /// <summary>
        /// 需要排除的文件类型 \
        /// File types to ignore
        /// </summary>
        public string[] Filter;
    }
}
