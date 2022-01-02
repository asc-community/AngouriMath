//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core
{
    /// <summary>
    /// From this interface all single-argument nodes are inherited
    /// </summary>
    public interface IUnaryNode
    {
        /// <summary>
        /// The only child of the node
        /// </summary>
        public Entity NodeChild { get; }
    }

    /// <summary>
    /// From this interface all double-argument nodes are inherited
    /// </summary>
    public interface IBinaryNode
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
