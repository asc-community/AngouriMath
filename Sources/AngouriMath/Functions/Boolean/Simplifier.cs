//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using HonkSharp.Laziness;

namespace AngouriMath
{
    partial record Entity
    {
        // The flat list of a chain of one associative connective, cached on the node exactly as
        // Sumf.LinearChildren and Mulf.LinearChildren are: a node is immutable, the list is a
        // property of it, and a nested chain's list is built from the lists cached on its parts.
        // https://github.com/asc-community/AngouriMath/issues/224

        partial record Orf
        {
            /// <summary>The disjuncts of a chain of <c>or</c>, flattened.</summary>
            internal static IReadOnlyList<Entity> LinearChildren(Entity expr)
                => expr is Orf chain ? chain.Linear : new[] { expr };
            private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Sumf.Flatten(LinearChildren(@this.Left), LinearChildren(@this.Right)), this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;
        }

        partial record Andf
        {
            /// <summary>The conjuncts of a chain of <c>and</c>, flattened.</summary>
            internal static IReadOnlyList<Entity> LinearChildren(Entity expr)
                => expr is Andf chain ? chain.Linear : new[] { expr };
            private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Sumf.Flatten(LinearChildren(@this.Left), LinearChildren(@this.Right)), this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;
        }

        partial record Xorf
        {
            /// <summary>The operands of a chain of <c>xor</c>, flattened.</summary>
            internal static IReadOnlyList<Entity> LinearChildren(Entity expr)
                => expr is Xorf chain ? chain.Linear : new[] { expr };
            private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Sumf.Flatten(LinearChildren(@this.Left), LinearChildren(@this.Right)), this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;
        }

        partial record Set
        {
            partial record Unionf
            {
                /// <summary>The sets of a chain of unions, flattened.</summary>
                internal static IReadOnlyList<Entity> LinearChildren(Entity expr)
                    => expr is Unionf chain ? chain.Linear : new[] { expr };
                private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Sumf.Flatten(LinearChildren(@this.Left), LinearChildren(@this.Right)), this);
                private LazyPropertyA<IReadOnlyList<Entity>> linear;
            }

            partial record Intersectionf
            {
                /// <summary>The sets of a chain of intersections, flattened.</summary>
                internal static IReadOnlyList<Entity> LinearChildren(Entity expr)
                    => expr is Intersectionf chain ? chain.Linear : new[] { expr };
                private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Sumf.Flatten(LinearChildren(@this.Left), LinearChildren(@this.Right)), this);
                private LazyPropertyA<IReadOnlyList<Entity>> linear;
            }
        }
    }
}
