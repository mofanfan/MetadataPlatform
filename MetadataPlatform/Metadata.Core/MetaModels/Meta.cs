using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaModels
{
    /// <summary>
    /// 元数据
    /// </summary>
    [DebuggerDisplay("Name:{Name}")]
    public class Meta
    {
        /// <summary>
        /// 名字
        /// </summary>
        public virtual string Name
        {
             get;
        }

        public Meta(string name)
        {
            Name = name;
        }
    }
}
