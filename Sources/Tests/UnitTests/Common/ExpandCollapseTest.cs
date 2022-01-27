//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Common
{
    public sealed class ExpandFactorizeTest
    {
        public static readonly Entity.Variable x = MathS.Var(nameof(x));
        public static readonly Entity.Variable y = MathS.Var(nameof(y));
        [Fact] public void ExpandAlgebra1() =>
            Assert.Equal(16, ((x + y) * (x - y)).Expand().Substitute(x, 5).Substitute(y, 3).EvalNumerical());
        [Fact] public void ExpandAlgebra2() =>
            Assert.Equal(64, ((x + y + x + y) * (x - y + x - y)).Expand().Substitute(x, 5).Substitute(y, 3).EvalNumerical());
        [Fact] public void Factorize1() =>
            Assert.Equal(x * (1 + y), (x * y + x).Factorize());
        [Fact]
        public void Factorial()
        {
            var expr = MathS.Factorial(x + 3) / MathS.Factorial(x + 1);
            Assert.Equal(MathS.Pow(x, 2) + x * 3 + (2 * x + 6), expr.Expand());
            expr = MathS.Factorial(x + -3) / MathS.Factorial(x + -1);
            Assert.Equal(1 / (x + -2) / (x + -1), expr.Expand());
        }
    }
}