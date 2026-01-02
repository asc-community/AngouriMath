//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using AngouriMath.Functions.Algebra;
using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Integrates the given expression over the `x` variable, if can.
        /// May return an unresolved <see cref="Integralf"/> node.
        /// </summary>
        /// <param name="x">Over which variable to integrate</param>
        /// <returns>
        /// An integrated expression. It might remain the same,
        /// it might have no integrals, and it might be transformed so that
        /// only a few nodes have unresolved integrals.
        /// </returns>
        public Entity Integrate(Variable x)
            => Integration.ComputeIndefiniteIntegral(this, x).InnerSimplified;
    }
}

namespace AngouriMath.Functions.Algebra
{
    internal static partial class Integration
    {
        internal static Entity ComputeIndefiniteIntegral(Entity expr, Entity.Variable x)
        {
            if (!expr.ContainsNode(x)) return expr * x; // base case, handle here

            Entity? answer;

            answer = IntegralPatterns.TryStandardIntegrals(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveBySplittingSum(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveAsPolynomialTerm(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveIntegratingByParts(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveLogarithmic(expr, x);
            if (answer is { }) return answer;

            return new Entity.Integralf(expr, x, 1); // return as integral if nothing can be done with expression
        }



        /// <summary>
        /// Numerical definite integration
        /// See more at <see cref="MathS.Compute.DefiniteIntegral(Entity, Entity.Variable, EDecimal, EDecimal)"/>
        /// </summary>
        internal static Complex Integrate(Entity func, Entity.Variable x, (EDecimal Re, EDecimal Im) from, (EDecimal Re, EDecimal Im) to, int stepCount)
        {
            System.Numerics.Complex res = 0;
            var cfunc = func.Compile(x);
            for (int i = 0; i <= stepCount; i++)
            {
                var share = ((EDecimal)i) / stepCount;
                var tmp = Complex.Create(from.Re * share + to.Re * (1 - share), from.Im * share + to.Im * (1 - share));
                res += cfunc.Substitute(tmp.ToNumerics());
            }
            return res.ToNumber() / (stepCount + 1) * (Complex.Create(to.Re, to.Im) - Complex.Create(from.Re, from.Im));
        }
    }
}
