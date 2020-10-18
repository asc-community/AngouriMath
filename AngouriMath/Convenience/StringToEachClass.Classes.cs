using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

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
            public static implicit operator Variable(string expr)
                => FromString<Variable>(expr);
        }

        partial record Tensor
        {
            public static implicit operator Tensor(string expr)
                => FromString<Tensor>(expr);
        }

        partial record Set
        {
            public static implicit operator Set(string expr)
                => FromString<Set>(expr);

            partial record FiniteSet
            {
                public static implicit operator FiniteSet(string expr)
                    => FromString<FiniteSet>(expr);
            }
        }
    }
}
