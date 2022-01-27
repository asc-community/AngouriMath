//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Algebra.PolynomialSolverTests
{
    using static AngouriMath.Entity;
    using static AngouriMath.Entity.Set;
    using static Entity.Number;
    public sealed class NumericalEquationsSolve
    {
        private readonly Entity.Variable x = nameof(x);

        private void AssertExprRoots(Set roots, Entity expr)
        {
            roots = (Set)roots.InnerSimplified;
            var finite = Assert.IsType<FiniteSet>(roots);
            foreach (var root in finite)
                SolveOneEquation.AssertRoots(expr, x, root);
        }

        [Theory, CombinatorialData]
        public void RealRootsPower3(
            [CombinatorialValues(-4, 1, 5)] int v1,
            [CombinatorialValues(-3, 3, 6)] int v2,
            [CombinatorialValues(-1, 2, 10)] int v3)
        {
            
            var expr = (x + v1) * (x + v2) * (x + v3);
            var newexpr = expr.Expand();
            AssertExprRoots(newexpr.SolveEquation(x), newexpr);
        }

        [Theory, CombinatorialData]
        public void ComplexRootsPower3(
            [CombinatorialValues(-3, 3)] int v1re,
            [CombinatorialValues(-4, 4)] int v1im,
            [CombinatorialValues(-5, 5)] int v2re,
            [CombinatorialValues(-6, 6)] int v2im,
            [CombinatorialValues(-7, 7)] int v3re,
            [CombinatorialValues(-8, 8)] int v3im
            )
        {
            var v1 = Complex.Create(v1re, v1im);
            var v2 = Complex.Create(v2re, v2im);
            var v3 = Complex.Create(v3re, v3im);
            var expr = (x + v1) * (x + v2) * (x + v3);
            var newexpr = expr.Expand();
            AssertExprRoots(newexpr.SolveEquation(x), newexpr);
        }

        [Theory, CombinatorialData]
        public void RealRootsPower4(
            [CombinatorialValues(-4, 5)] int v1,
            [CombinatorialValues(-3, 5)] int v2,
            [CombinatorialValues(-1, 7)] int v3,
            [CombinatorialValues(-2, 9, 3)] int v4)
        {
            var expr = (x - v1) * (x - v2) * (x - v3) * (x - v4);
            var newexpr = expr.Expand();
            AssertExprRoots(newexpr.SolveEquation(x), newexpr);
        }

        [Theory, CombinatorialData]
        public void ComplexRootsPower4(
            [CombinatorialValues(-3, 3)] int v1re,
            [CombinatorialValues(-4, 4)] int v1im,
            [CombinatorialValues(-5, 5)] int v2re,
            [CombinatorialValues(1)] int v2im,
            [CombinatorialValues(-7, 7)] int v3re,
            [CombinatorialValues(2)] int v3im,
            [CombinatorialValues(-9, 9)] int v4re,
            [CombinatorialValues(-10, 10)] int v4im
            )
        {
            var v1 = Complex.Create(v1re, v1im);
            var v2 = Complex.Create(v2re, v2im);
            var v3 = Complex.Create(v3re, v3im);
            var v4 = Complex.Create(v4re, v4im);
            var expr = (x - v1) * (x - v2) * (x - v3) * (x - v4);
            var newexpr = expr.Expand();
            AssertExprRoots(newexpr.SolveEquation(x), newexpr);
        }
    }
}
