//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core
{
    /// <summary>
    /// Where to tend to the given number in limits
    /// </summary>
    public enum ApproachFrom : int // explicit size and enumerating for native exports
    {
        /// <summary>
        /// Means that the limit is considered valid if and only if
        /// Left-sided limit exists and Right-sided limit exists
        /// and they are equal
        /// </summary>
        BothSides = 0,

        /// <summary>
        /// If x tends from the left, i. e. it is never greater than the destination
        /// </summary>
        Left = 1,

        /// <summary>
        /// If x tends from the right, i. e. it is never less than the destination
        /// </summary>
        Right = 2,
    }
}

namespace AngouriMath
{
    using Core;
    using static Functions.Algebra.LimitFunctional;
    partial record Entity
    {
        /// <summary>
        /// Finds the limit of the given expression over the given variable
        /// </summary>
        /// <param name="x">
        /// The variable to be approaching
        /// </param>
        /// <param name="destination">
        /// A value where the variable approaches. It might be a symbolic
        /// expression, a finite number, or an infinite number, for example,
        /// "sqrt(x2 + x) / (3x + 3)".Limit("x", "+oo", ApproachFrom.BothSides)
        /// </param>
        /// <param name="side">
        /// From where to approach it: from the left, from the right,
        /// or BothSides, implying that if limits from either are not
        /// equal, there is no limit
        /// </param>
        /// <returns>
        /// A result or the <see cref="Limitf"/> node if the limit
        /// cannot be determined
        /// </returns>
        public Entity Limit(Variable x, Entity destination, ApproachFrom side)
        { 
            var res = ComputeLimit(this, x, destination, side); 
            if (res is null || res == MathS.NaN)
                return new Limitf(this, x, destination, side);
            return res.InnerSimplified;
        }

        /// <summary>
        /// Finds the limit of the given expression over the given variable
        /// </summary>
        /// <param name="x">
        /// The variable to be approaching
        /// </param>
        /// <param name="destination">
        /// A value where the variable approaches. It might be a symbolic
        /// expression, a finite number, or an infinite number, for example,
        /// "sqrt(x2 + x) / (3x + 3)".Limit("x", "+oo")
        /// </param>
        /// <returns>
        /// A result or the <see cref="Limitf"/> node if the limit
        /// cannot be determined
        /// </returns>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, a) = Var("x", "a");
        /// var expr = (1 + a / x).Pow(x);
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Limit(x, +oo));
        /// Console.WriteLine("----------------------");
        /// var expr1 = Sin(x * a) / x;
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Limit(x, +oo));
        /// Console.WriteLine("----------------------");
        /// var expr2 = (a * Sqr(x) + x + Sqr(a)) / (3 * x + 5 * Sqr(x) + 9);
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Limit(x, 0));
        /// Console.WriteLine("----------------------");
        /// var expr3 = (a * Sqr(x) + x + Sqr(a)) / (3 * x + 5 * Sqr(x) + 9);
        /// Console.WriteLine(expr3);
        /// Console.WriteLine(expr3.Limit(x, +oo));
        /// </code>
        /// Prints
        /// <code>
        /// (1 + a / x) ^ x
        /// e ^ a
        /// ----------------------
        /// sin(x * a) / x
        /// limit(sin(x * a) / x, x, +oo)
        /// ----------------------
        /// (a * x ^ 2 + x + a ^ 2) / (3 * x + 5 * x ^ 2 + 9)
        /// a ^ 2 / 9
        /// ----------------------
        /// (a * x ^ 2 + x + a ^ 2) / (3 * x + 5 * x ^ 2 + 9)
        /// a / 5
        /// </code>
        /// </example>
        public Entity Limit(Variable x, Entity destination)
        {
            var res = ComputeLimit(this, x, destination, ApproachFrom.BothSides);
            if (res is null || res == MathS.NaN)
                return new Limitf(this, x, destination, ApproachFrom.BothSides).InnerSimplified;
            return res.InnerSimplified;
        }

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Divide_and_rule"/>
        /// Divide and rule (Latin: divide et impera), or divide and conquer,
        /// in politics and sociology is gaining and maintaining power by
        /// breaking up larger concentrations of power into pieces that
        /// individually have less power than the one implementing the strategy.
        /// 
        /// In computer science, divide and conquer is an algorithm design paradigm
        /// based on multi-branched recursion. A divide-and-conquer algorithm works
        /// by recursively breaking down a problem into two or more sub-problems of
        /// the same or related type, until these become simple enough to be solved
        /// directly. The solutions to the sub-problems are then combined to give a
        /// solution to the original problem.
        /// </summary>
        // here we try to compute limit for each children and then merge them into limit of whole expression
        // theoretically, for cases such limit (x -> -1) 1 / (x + 1)
        // this method will return NaN, but thanks to replacement of x to an non-definite expression,
        // it is somehow compensated
        internal virtual Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            => new Limitf(this, x, dist, side);
    }
}