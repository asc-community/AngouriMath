//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;

namespace AngouriMath
{
    partial record Entity
    {
        private static T FromString<T>(string expr) where T : Entity
        {
            var ent = expr.ToEntity();
            if (ent is T tEnt)
                return tEnt;
            throw new CannotParseInstanceException(typeof(T), expr);
        }

        partial record Variable
        {
            /// <summary>
            /// Converts from string to specifically variable
            /// </summary>
            /// <param name="expr">Its future name</param>
            public static implicit operator Variable(string expr)
                => FromString<Variable>(expr);
        }

        partial record Matrix
        {
            /// <summary>
            /// Converts from string to specifically tensor
            /// </summary>
            /// <param name="expr">From where to parse (available since 1.3)</param>
            public static implicit operator Matrix(string expr)
                => FromString<Matrix>(expr);
        }

        partial record Set
        {
            /// <summary>
            /// Converts from string to specifically set
            /// </summary>
            /// <param name="expr">From where to parse</param>

            public static implicit operator Set(string expr)
                => FromString<Set>(expr);

            partial record FiniteSet
            {
                /// <summary>
                /// Converts from string to specifically finite set
                /// </summary>
                /// <param name="expr">From where to parse</param>
                public static implicit operator FiniteSet(string expr)
                    => FromString<FiniteSet>(expr);
            }
        }
    }
}
