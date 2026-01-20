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
        /// Integrates indefinitely the given expression over the `x` variable, if can.
        /// May return an unresolved <see cref="Integralf"/> node.
        /// </summary>
        /// <param name="x">Over which variable to integrate</param>
        /// <returns>
        /// An integrated expression. It might remain the same or be transformed into nodes with no integrals.
        /// </returns>
        public Entity Integrate(Variable x) =>
            Integration.ComputeIndefiniteIntegral(this, x) is { } antiderivative
            ? antiderivative + (antiderivative.VarsAndConsts.Contains("C") ? Variable.CreateUnique(antiderivative, "C") : "C")
            : new Integralf(this, x, null);
        /// <summary>
        /// Integrates definitely the given expression over the `x` variable, if can.
        /// May return an unresolved <see cref="Integralf"/> node.
        /// </summary>
        /// <param name="x">Over which variable to integrate</param>
        /// <param name="from">The lower bound for integrating</param>
        /// <param name="to">The upper bound for integrating</param>
        /// <returns>
        /// An integrated expression. It might remain the same or be transformed into nodes with no integrals.
        /// </returns>
        public Entity Integrate(Variable x, Entity from, Entity to) =>
            Integration.ComputeIndefiniteIntegral(this, x)?.InnerSimplified is { } antiderivative
            ? antiderivative.Substitute(x, to) - antiderivative.Substitute(x, from)
            : new Integralf(this, x, (from, to));
    }
}

namespace AngouriMath.Functions.Algebra
{
    internal static partial class Integration
    {
        /// <summary>Does not add the constant of integration because this is called recursively.</summary>
        internal static Entity? ComputeIndefiniteIntegral(Entity expr, Entity.Variable x)
        {
            if (!expr.ContainsNode(x)) return expr * x; // base case, handle here

            Entity? answer;

            answer = IntegralPatterns.TryStandardIntegrals(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveAsPolynomialTerm(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveLogarithmic(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveBySubstitution(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveIntegratingByParts(expr, x);
            if (answer is { }) return answer;

            answer = IndefiniteIntegralSolver.SolveBySplittingSum(expr, x); // placed last because this may expand to too many terms
            if (answer is { }) return answer;

            return null;
        }
        /// <summary>
        /// Returns the approximate numeric value of a definite integral of a function. Only works for one-variable functions.
        /// Accuracy is limited to the number specified (default is 100).
        /// See more at <see cref="MathS.Compute.DefiniteIntegral(Entity, Entity.Variable, EDecimal, EDecimal)"/>
        /// </summary>
        /// <param name="expr">Expression to integrate</param>
        /// <param name="x">Variable to integrate over</param>
        /// <param name="from">The complex lower bound for integrating</param>
        /// <param name="to">The complex upper bound for integrating</param>
        /// <param name="accuracy">Accuracy (for now, number of iterations)</param>
        internal static Complex IntegrateNumerically(Entity expr, Entity.Variable x, Complex from, Complex to, int accuracy = 100)
        {
            System.Numerics.Complex res = 0;
            var cfunc = expr.Compile(x);
            for (int i = 0; i <= accuracy; i++)
            {
                var share = ((EDecimal)i) / accuracy;
                var tmp = Complex.Create(from.RealPart.EDecimal * share + to.RealPart.EDecimal * (1 - share), from.ImaginaryPart.EDecimal * share + to.ImaginaryPart.EDecimal * (1 - share));
                res += cfunc.Substitute(tmp.ToNumerics());
            }
            return res.ToNumber() / (accuracy + 1) * (to - from);
        }
    }
}
