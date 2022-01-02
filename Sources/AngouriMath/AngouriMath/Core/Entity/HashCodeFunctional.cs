//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;

namespace AngouriMath.Core.HashCode
{
    internal static class HashCodeFunctional
    {
        public enum HashCodeShifts
        {
            FiniteSet = 0x10000000,
            Piecewise = 0x20000000
        }

        public static int Multielement<T>(HashCodeShifts type, IEnumerable<T> objects)
            => (int)type |
            objects.Count() switch
            {
                0 => 0,
                1 => objects.First()?.GetHashCode() ?? throw new AngouriBugException("If count is 1, it can't be null"),
                _ => objects.Select(c => c?.GetHashCode() ?? throw new AngouriBugException("Can't be null here")).Aggregate((acc, next) => (acc, next).GetHashCode())
            };

        public static int HashCodeOfSequence<T>(this IEnumerable<T> @this, HashCodeShifts type)
            => Multielement(type, @this);
    }
}
