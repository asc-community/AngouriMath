//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Functions.Algebra.AnalyticalSolving;
using static AngouriMath.Entity.Set;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Solves a <see cref="Statement"/>
        /// Statement is an Entity such that its value is true for
        /// any x in X, where X is the result of this method.
        /// See more about <see cref="Set"/>
        /// </summary>
        /// <param name="var">Over which variable to solve</param>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// var eq = Sin(x).Equalizes(0.5);
        /// Console.WriteLine(eq);
        /// Console.WriteLine(eq.Solve(x));
        /// Console.WriteLine("-----------------");
        /// var eq2 = (Sqr(x) + 2 * x * y - y).Equalizes(0);
        /// Console.WriteLine(eq2);
        /// Console.WriteLine(eq2.Solve(x));
        /// Console.WriteLine("-----------------");
        /// var eq3 = ((x - 3) * (x - 6)).Equalizes(0) &amp; ((x - 3) * (x - 7)).Equalizes(0);
        /// Console.WriteLine(eq3);
        /// Console.WriteLine(eq3.Solve(x));
        /// Console.WriteLine("-----------------");
        /// var eq4 = ((x - 3) * (x - 6)).Equalizes(0) | ((x - 3) * (x - 7)).Equalizes(0);
        /// Console.WriteLine(eq4);
        /// Console.WriteLine(eq4.Solve(x));
        /// Console.WriteLine("-----------------");
        /// var eq6 = GreaterThan((x - 3) * (x + 6), 0);
        /// Console.WriteLine(eq6);
        /// Console.WriteLine(eq6.Solve(x));
        /// Console.WriteLine("-----------------");
        /// var eq7 = LessThan((x - 3) * (x + 6), 0);
        /// Console.WriteLine(eq7);
        /// Console.WriteLine(eq7.Solve(x));
        /// Console.WriteLine("-----------------");
        /// var eq8 = (Sin(x) * x - 3).Equalizes(0);
        /// Console.WriteLine(eq8);
        /// Console.WriteLine(eq8.Solve(x));
        /// Console.WriteLine("-----------------");
        /// using var _ = Settings.AllowNewton.Set(false);
        /// var eq9 = (Sin(x) * x - 3).Equalizes(0);
        /// Console.WriteLine(eq9);
        /// Console.WriteLine(eq9.Solve(x));
        /// 
        /// </code>
        /// Prints
        /// <code>
        /// sin(x) = 1/2
        /// { arcsin(1/2) + 2 * pi * n_1, pi - arcsin(1/2) + 2 * pi * n_1 }
        /// -----------------
        /// x ^ 2 + 2 * x * y - y = 0
        /// { (-2 * y - sqrt((2 * y) ^ 2 - 4 * -y)) / 2, (-2 * y + sqrt((2 * y) ^ 2 - 4 * -y)) / 2 }
        /// -----------------
        /// (x - 3) * (x - 6) = 0 and (x - 3) * (x - 7) = 0
        /// { 3 }
        /// -----------------
        /// (x - 3) * (x - 6) = 0 or (x - 3) * (x - 7) = 0
        /// { 3, 6, 7 }
        /// -----------------
        /// (x - 3) * (x + 6) &gt; 0
        /// (-oo; -6) \/ (3; +oo)
        /// -----------------
        /// (x - 3) * (x + 6) &lt; 0
        /// (-6; 3)
        /// -----------------
        /// sin(x) * x - 3 = 0
        /// { 9.08837698687877... }
        /// -----------------
        /// sin(x) * x - 3 = 0
        /// {  }
        /// </code>
        /// </example>
        public Set Solve(Variable var)
        {
            if (this is Statement)
            {
                var res = StatementSolver.Solve(this, var);
                return (Set)res.InnerSimplified;
            }
            if (this == var)
                return new FiniteSet(Boolean.True);
            throw new SolveRequiresStatementException();
        }
    }
}
