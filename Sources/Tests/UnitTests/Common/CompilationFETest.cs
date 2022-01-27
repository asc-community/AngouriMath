//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System.Numerics;
using Xunit;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Common
{
    public sealed class CompilationFETest
    {
        private static readonly Entity.Variable x = MathS.Var(nameof(x));
        private static readonly Entity.Variable y = MathS.Var(nameof(y));

        [Theory]
        [InlineData("x")]
        [InlineData("x + 2")]
        [InlineData("sin(x) + cos(x)")]
        [InlineData("arcsin(x) + arccos(x)")]
        [InlineData("arcsin(x) + arccos(x) / 2")]
        [InlineData("sqrt(x) + e ^ x")]
        [InlineData("sin(x) + 2")]
        [InlineData("cos(x) + 2")]
        [InlineData("tan(x) + 2")]
        [InlineData("cot(x) + 2")]
        [InlineData("sec(x) + 2")]
        [InlineData("csc(x) + 2")]
        [InlineData("arcsin(x) + 2")]
        [InlineData("arccos(x) + 2")]
        [InlineData("arctan(x) + 2")]
        [InlineData("arccot(x) + 2")]
        [InlineData("arcsec(x) + 2")]
        [InlineData("arccsc(x) + 2")]
        [InlineData("log(3, x) + 2")]
        [InlineData("log(x, 3) + 2")]
        [InlineData("3 ^ x + 2")]
        [InlineData("1 / x")]
        [InlineData("1 - x")]
        [InlineData("(x + 2)!")]
        [InlineData("sign(x + 2)")]
        [InlineData("(|x + 2|)")]
        public void Test(string expr, float? toSub = null)
        {
            toSub ??= 3;
            var exprCompiled = expr.Compile(x);
            var expected = (Complex)expr.Substitute("x", toSub).EvalNumerical();
            var actual = exprCompiled.Call((Complex)toSub);
            var error = Complex.Abs(expected - actual);
            Assert.True(error < 0.001, $"Error: {error}\nActual: {actual}\nExpected: {expected}");
        }
    }
}
