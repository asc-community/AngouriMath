//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        private static bool IsLogic(Entity a)
            => a is Statement or Variable;

        private static bool IsLogic(Entity a, Entity b)
            => IsLogic(a) && IsLogic(b);

        private static bool IsLogic(Entity a, Entity b, Entity c)
            => IsLogic(a, b) && IsLogic(c);

        internal static Entity BooleanRules(Entity x) => x switch
        {
            Impliesf(var ass, _) when ass == False && IsLogic(ass) => True,
            Andf(Notf(var any1), Notf(var any2)) when IsLogic(any1, any2) => !(any1 | any2),
            Orf(Notf(var any1), Notf(var any2)) when IsLogic(any1, any2) => !(any1 & any2),
            Orf(Notf(var any1), var any1a) when any1 == any1a && IsLogic(any1) => True,
            Orf(Notf(var any1), var any2) when IsLogic(any1, any2) => any1.Implies(any2),
            Andf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => any1,
            Orf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => any1,
            Impliesf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => True,
            Xorf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => False,
            Notf(Notf(var any1)) when IsLogic(any1) => any1,

            Orf(var any1, var any2) when (any1 == True || any2 == True) && IsLogic(any1, any2) => True,
            Andf(var any1, var any2) when (any1 == False || any2 == False) && IsLogic(any1, any2) => False,

            Orf(Andf(var any1, var any2), Andf(var any1a, var any3)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any1, var any2), Orf(var any1a, var any3)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),
            Orf(Andf(var any1, var any2), Andf(var any3, var any1a)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any1, var any2), Orf(var any3, var any1a)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),
            Orf(Andf(var any2, var any1), Andf(var any1a, var any3)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any2, var any1), Orf(var any1a, var any3)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),
            Orf(Andf(var any2, var any1), Andf(var any3, var any1a)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any2, var any1), Orf(var any3, var any1a)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),

            Orf(var any1, Andf(var any1a, _)) when any1 == any1a && IsLogic(any1) => any1,
            Andf(var any1, Orf(var any1a, _)) when any1 == any1a && IsLogic(any1) => any1,

            Orf(var any1, Andf(Notf(var any1a), var any2)) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(var any1, Orf(Notf(var any1a), var any2)) when any1 == any1a && IsLogic(any1) => any1 & any2,
            Orf(var any1, Andf(var any1a, Notf(var any2))) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(var any1, Orf(var any1a, Notf(var any2))) when any1 == any1a && IsLogic(any1) => any1 & any2,
            Orf(Andf(Notf(var any1a), var any2), var any1) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(Orf(Notf(var any1a), var any2), var any1) when any1 == any1a && IsLogic(any1) => any1 & any2,
            Orf(Andf(var any1a, Notf(var any2)), var any1) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(Orf(var any1a, Notf(var any2)), var any1) when any1 == any1a && IsLogic(any1) => any1 & any2,

            //xor
            Orf(Andf(var any1a, Notf(var any1b)), Andf(var any2b, Notf(var any2a))) when any1a == any2a && any1b == any2b && IsLogic(any1a, any1b) => any1a ^ any1b,
            Orf(Andf(Notf(var any1b), var any1a), Andf(var any2b, Notf(var any2a))) when any1a == any2a && any1b == any2b && IsLogic(any1a, any1b) => any1a ^ any1b,
            Orf(Andf(var any1a, Notf(var any1b)), Andf(Notf(var any2a) ,var any2b)) when any1a == any2a && any1b == any2b && IsLogic(any1a, any1b) => any1a ^ any1b,
            Orf(Andf(Notf(var any1b), var any1a), Andf(Notf(var any2a) ,var any2b)) when any1a == any2a && any1b == any2b && IsLogic(any1a, any1b) => any1a ^ any1b,

            Impliesf(Notf(var any1), Notf(var any2)) when IsLogic(any1, any2) => any2.Implies(any1),

            _ => x
        };
    }
}
