using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core
{
    /// <summary>
    /// From this interface all single-argument nodes are inherited
    /// </summary>
    public interface IOneArgumentNode
    {
        /// <summary>
        /// The only child of the node
        /// </summary>
        public Entity NodeChild { get; }
    }

    /// <summary>
    /// From this interface all double-argument nodes are inherited
    /// </summary>
    public interface ITwoArgumentNode
    {
        /// <summary>
        /// The left child of the node
        /// </summary>
        public Entity NodeFirstChild { get; }

        /// <summary>
        /// The right child of the node
        /// </summary>
        public Entity NodeSecondChild { get; }
    }
}
