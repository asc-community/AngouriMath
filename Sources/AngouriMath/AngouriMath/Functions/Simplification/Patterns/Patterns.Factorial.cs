//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        /// <summary>(x + a)! / (x + b)! -> (x+b+1)*(x+b+2)*...*(x+a)</summary>
        internal static Entity ExpandFactorialDivisions(Entity expr)
        {
            Entity ExpandFactorialDivisions(Entity x, Entity x2, Number num, Number den)
            {
                static Entity Add(Entity a, Number b) =>
                    b is Integer(0) ? a : a + b;
                if (x == x2
                    && num - den is Integer { EInteger: var diff }
                    && !diff.IsZero && diff.Abs() < 20) // We don't want to expand (x+100)!/x!
                    if (diff > 0) // e.g. (x+3)!/x! = (x+1)(x+2)(x+3)
                    {
                        var expr = Add(x, den + 1);
                        for (var i = 2; i <= diff; i++)
                            expr *= Add(x, den + i);
                        return expr;
                    }
                    else // e.g. x!/(x+3)! = 1/(x+1)/(x+2)/(x+3)
                    {
                        diff = -diff;
                        var expr = 1 / Add(x, num + 1);
                        for (var i = 2; i <= diff; i++)
                            expr /= Add(x, num + i);
                        return expr;
                    }
                return expr;
            }
            return expr switch
            {
                Divf(Factorialf(Sumf(var any1, Number const1)), Factorialf(Sumf(var any1a, Number const2)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(Sumf(var any1, Number const1)), Factorialf(Sumf(Number const2, var any1a)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(Sumf(Number const1, var any1)), Factorialf(Sumf(var any1a, Number const2)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(Sumf(Number const1, var any1)), Factorialf(Sumf(Number const2, var any1a)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(var any1), Factorialf(Sumf(var any1a, Number const2)))
                    => ExpandFactorialDivisions(any1, any1a, 0, const2),
                Divf(Factorialf(var any1), Factorialf(Sumf(Number const2, var any1a)))
                    => ExpandFactorialDivisions(any1, any1a, 0, const2),
                Divf(Factorialf(Sumf(var any1, Number const1)), Factorialf(var any1a))
                    => ExpandFactorialDivisions(any1, any1a, const1, 0),
                Divf(Factorialf(Sumf(Number const1, var any1)), Factorialf(var any1a))
                    => ExpandFactorialDivisions(any1, any1a, const1, 0),
                _ => expr
            };
        }

        // https://en.wikipedia.org/wiki/Reflection_formula
        // (z-1)! (-z)! -> Γ(z) Γ(1 - z) = π/sin(π z), z ∉ ℤ // actually, when z ∈ ℤ, both sides include division by zero, so we can still replace
        // Replace z with -z => z! (-z-1)! = π/sin(-π z)
        // TODO: Modify the complexity criteria to rank non-elementary functions more complex than elementary functions
        //       so that this formula can be used to simplify
        // TODO: Other than the reflection formula,
        // (z-1)! (z-1/2)! -> Γ(z) Γ(z + 1/2) = 2^(1 - 2 z) sqrt(π) Γ(2 z) -> 2^(1 - 2 z) sqrt(π) (2 z - 1)!
        // is also another possible simplification
        /// <summary>(x-1)! x -> x!, x! (x+1) -> (x+1)!, etc. <!--as well as z! (-z-1)! -> -π/sin(π z)--></summary>
        internal static Entity FactorizeFactorialMultiplications(Entity expr)
        {
            Entity FactorizeFactorialMultiplications(Entity x, Entity x2, Number factConst, Number @const) =>
                x == x2 && factConst + 1 == @const ? new Factorialf(x + @const) : expr;
            return expr switch
            {
                Mulf(Factorialf(Sumf(var any1, Number const1)), Sumf(var any1a, Number const2)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(Sumf(Number const1, var any1)), Sumf(var any1a, Number const2)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(Sumf(var any1, Number const1)), Sumf(Number const2, var any1a)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(Sumf(Number const1, var any1)), Sumf(Number const2, var any1a)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(var any1), Sumf(var any1a, Number const2)) =>
                    FactorizeFactorialMultiplications(any1, any1a, 0, const2),
                Mulf(Factorialf(var any1), Sumf(Number const2, var any1a)) =>
                    FactorizeFactorialMultiplications(any1, any1a, 0, const2),
                Mulf(Factorialf(Sumf(var any1, Number const1)), var any1a) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, 0),
                Mulf(Factorialf(Sumf(Number const1, var any1)), var any1a) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, 0),
                _ => expr
            };
        }
    }
}
