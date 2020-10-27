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
        /// <summary>
        /// 工作模式，0为主动模式，其余值为被动模式的间隔时间（分钟） \ WorkMode, 0 for initiative mode, other for interval(min) of passivity mode.
        /// </summary>
        public int WorkMode;
    }
}
