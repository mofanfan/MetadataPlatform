using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaModels.Views.Aggt
{
    /// <summary>
    /// 界面块的类型
    /// 
    /// 界面块的类型是可以扩展的。
    /// 注意，自定义的块的值，需要大于 10。（10 以内的值是系统预留块）。
    /// </summary>
    [DebuggerDisplay("{Name}({Value})")]
    public struct BlockType : IEquatable<BlockType>
    {
        /// <summary>
        /// 列表（树型表格、表格）。
        /// </summary>
        public static readonly BlockType List = new BlockType(1, "List");
        /// <summary>
        /// 详细（表单）
        /// </summary>
        public static readonly BlockType Detail = new BlockType(2, "Detail");
        /// <summary>
        /// 报表、图表
        /// </summary>
        public static readonly BlockType Report = new BlockType(3, "Report");

        /// <summary>
        /// 块类型的 Id。
        /// 
        /// 整个系统中必须唯一。
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 块类型的名称
        /// 
        /// 无特别的用处，只为标识。
        /// </summary>
        public string Name { get; private set; }

        #region 构造器 & 注册

        private BlockType(int id, string name)
            : this()
        {
            this.Name = name;
            this.Id = id;
        }

        /// <summary>
        /// 自定义块类型从 11 开始。
        /// </summary>
        private static int _customTypeFrom = 11;

        /// <summary>
        /// 通过值和名称构造器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BlockType Register(string name)
        {
            return new BlockType(Interlocked.Increment(ref _customTypeFrom), name);
        }

        #endregion

        public static bool operator ==(BlockType a, BlockType b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(BlockType a, BlockType b)
        {
            return a.Id != b.Id;
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        public override readonly int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public bool Equals(BlockType other)
        {
            return this.Id == other.Id;
        }   
    }
}
