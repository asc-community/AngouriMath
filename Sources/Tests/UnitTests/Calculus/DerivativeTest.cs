//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using HonkSharp.Fluency;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Calculus
{
    [Trait("Area", "Calculus")]
    public sealed class DerivativeTest
    {
        private static readonly Variable x = "x";
        private static readonly Variable y = "y";
        private static readonly Variable f = "f";

        [Fact]
        public void Test1()
        {
            var func = MathS.Sqr(x) + 2 * x + 1;
            var derived = func.Differentiate(x);
            Assert.Equal(2 + 2 * x, derived.Simplify());
        }
        [Fact]
        public void TestSin()
        {
            var func = MathS.Sin(x);
            Assert.Equal(MathS.Cos(x), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestCosCustom()
        {
            var func = MathS.Cos(MathS.Pow(x, 3));
            var expected = -3 * MathS.Sin(MathS.Pow(x, 3)) * MathS.Sqr(x);
            var actual = func.Differentiate(x).Simplify();
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void TestPow()
        {
            var func = MathS.Pow(MathS.e, x);
            Assert.Equal(func, func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestPoly()
        {
            var func = MathS.Pow(x, 4);
            Assert.Equal(4 * MathS.Pow(x, 3), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestCusfunc()
        {
            var func = MathS.Sin(x).Pow(2);
            Assert.Equal(MathS.Sin(2 * x), func.Differentiate(x).Simplify(3));
        }
        [Fact]
        public void TestTan()
        {
            var func = MathS.Tan(2 * x);
            Assert.Equal(2 / MathS.Pow(MathS.Cos(2 * x), 2), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestCoTan()
        {
            var func = MathS.Cotan(2 * x);
            Assert.Equal(-2 / MathS.Pow(MathS.Sin(2 * x), 2), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc1()
        {
            var func = MathS.Arcsin(x);
            Assert.Equal(1 / MathS.Sqrt(1 - MathS.Sqr(x)), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc2()
        {
            var func = MathS.Arcsin(2 * x);
            Assert.Equal(1 / MathS.Sqrt(1 - MathS.Sqr(2 * x)) * 2, func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc3()
        {
            var func = MathS.Arccos(2 * x);
            Assert.Equal((-1) / MathS.Sqrt(1 - MathS.Sqr(2 * x)) * 2, func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc4()
        {
            var func = MathS.Arctan(2 * x);
            Assert.Equal(2 / (1 + 4 * MathS.Sqr(x)), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc5()
        {
            var func = MathS.Arccotan(2 * x);
            Assert.Equal(-2 / (1 + 4 * MathS.Sqr(x)), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestNaN()
        {
            var func = MathS.Numbers.Create(double.NaN);
            Assert.Equal(MathS.Numbers.Create(double.NaN), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestNaN2()
        {
            var func = MathS.Pow(21, MathS.Numbers.Create(double.NaN));
            Assert.Equal(MathS.Numbers.Create(double.NaN), func.Differentiate(x).Simplify());
        }

        [Fact]
        public void TestDerOverDer2()
        {
            var func = MathS.Derivative("x + 2", "y");
            var derFunc = func.Differentiate(x);
            Assert.Equal(0, derFunc);
        }

        // sgn is flat either side of zero and has no derivative at zero, so its
        // derivative is 0 wherever it exists, stated with the condition that says where
        // that is.
        [Fact]
        public void TestSgnDer()
        {
            Entity func = "sgn(x + 2)";
            var derived = func.Differentiate("x");
            Assert.Equal("0 provided not x + 2 = 0".ToEntity(), derived);
        }

        [Fact]
        public void TestAbsDer()
        {
            Entity func = "abs(x + 2)";
            var derived = func.Differentiate("x");
            Assert.Equal(MathS.Signum("2 + x").Provided("not 2 + x = 0"), derived.Simplify());
        }

        [Fact]
        public void TestSecant()
        {
            Entity func = "sec(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal(2 * MathS.Sec("2x") * MathS.Tan("2x"), derived.Simplify());
        }

        [Fact]
        public void TestCosecant()
        {
            Entity func = "csc(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal(-2 * MathS.Cosec("2x") * MathS.Cotan("2x"), derived.Simplify());
        }

        [Fact]
        public void TestArcsecant()
        {
            Entity func = "arcsec(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal("(1/2) / (sqrt(1 + (-1/4) / x2)x2)".Simplify(), derived.Simplify());
        }

        [Fact]
        public void TestArccosecant()
        {
            Entity func = "arccosec(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal("-1/2 / (sqrt(1 + (-1/4) / x2)x2)".Simplify(), derived.Simplify());
        }

        

        [Fact] public void TestNamedAppliedFunctions1()
            => f.Apply(x.Pow(2))  // f (x ^ 2)
                .Differentiate(x)
                .ShouldBe(
                    MathS.Derivative(f.Apply(x.Pow(2)), x.Pow(2)) * (2 * x) // derivative (f (x ^ 2), x ^ 2) * (2 * x)
                    );

        [Fact] public void TestNamedAppliedFunctions2()
            => f.Apply(x.Pow(2))
                .Differentiate(x)
                .LambdaOver(f)
                .Apply("sin")
                .InnerSimplified
                .ShouldBe(MathS.Cos(x.Pow(2)) * (2 * x));

        [Fact] public void TestNamedAppliedFunctions3()
            =>f.Apply(x.Pow(2), x.Pow(3))
                .Alias(out var ff)
                .Differentiate(x)
                .ShouldBe(
                    MathS.Derivative(ff, x.Pow(2)) * (2 * x)
                    + MathS.Derivative(ff, x.Pow(3)) * (3 * x.Pow(2))
                );

        /// <summary>
        /// https://github.com/asc-community/AngouriMath/issues/958 -- differentiating a factorial
        /// answered <c>NaN</c>, which claims the derivative does not exist. It does: <c>x!</c> is
        /// smooth away from the poles and the library evaluates <c>(1/2)!</c> happily. The honest
        /// answer is the unevaluated derivative, which is what every other node it cannot
        /// differentiate already returns.
        /// </summary>
        [Fact] public void AFactorialDeclinesRatherThanClaimingNoDerivativeExists()
        {
            var derivative = MathS.Derivative(MathS.Factorial(x), x).InnerSimplified;
            Assert.NotEqual(MathS.NaN, derivative);
            Assert.Contains(derivative.Nodes, node => node is Entity.Derivativef);
        }

        /// <summary>
        /// The reason it mattered: <c>NaN</c> propagates, so one factorial destroyed the whole
        /// derivative. Declining leaves every other term intact.
        /// </summary>
        [Fact] public void APartOfADerivativeSurvivesAFactorialItCannotTake()
        {
            var derivative = (MathS.Factorial(x) + x.Pow(2)).Differentiate(x).InnerSimplified;
            Assert.NotEqual(MathS.NaN, derivative);
            Assert.Contains(derivative.Nodes, node => node == 2 * x);
        }

        /// <summary>
        /// **Regression guard for a hang.** Declining instead of answering <c>NaN</c> removed the
        /// thing that used to stop l'Hopital: differentiating an unevaluated derivative only
        /// raises its <c>Iterations</c>, so every pass looked new to the repetition guard and the
        /// rule ran to 200,000 differentiations. It now refuses when a derivative it cannot take
        /// appears, and this limit -- which is meant to be refused -- terminates.
        /// </summary>
        [Fact(Timeout = 30000)] public void ALimitOverAFactorialTerminatesRatherThanDifferentiatingForever()
        {
            var limit = "ln(x!) * x - x^2 * ln(x)".ToEntity()
                .Limit(x, Entity.Number.Real.PositiveInfinity);
            Assert.True(limit.Evaled is Entity.Limitf,
                $"the expansion should be refused here, and it came back {limit.Evaled}");
        }

        /// <summary>
        /// The two overloads reached different code. <c>Differentiate(Variable)</c> goes through
        /// the transformation, which ends at <c>DifferentiateOnce</c> and simplifies;
        /// <c>Differentiate(Variable, int)</c> called <c>InnerDifferentiate</c> straight in its
        /// loop, so every <c>0 *</c> and <c>* 1</c> the chain rule produces stayed in and the
        /// next iteration differentiated those too. <c>x ^ 3</c> twice came back as
        /// <c>(0 * x ^ 2 + 2 * x ^ 1 * 1 * 3) * 1 + 0 * 3 * x ^ 2</c> where differentiating twice
        /// by hand gives <c>2 * x * 3</c>.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1002">#1002</a>
        /// </summary>
        [Theory]
        [InlineData("x ^ 4", 0)]
        [InlineData("x ^ 4", 1)]
        [InlineData("x ^ 4", 2)]
        [InlineData("x ^ 4", 3)]
        [InlineData("sin(x) * x", 2)]
        [InlineData("sin(x) / x", 2)]
        [InlineData("x ^ 5 + sin(x)", 3)]
        public void DifferentiatingNTimesIsDifferentiatingOnceNTimes(string exprRaw, int power)
        {
            var expr = exprRaw.ToEntity();
            var once = expr;
            for (var k = 0; k < power; k++)
                once = once.Differentiate(x);
            Assert.Equal(once, expr.Differentiate(x, power));
        }

        /// <summary>
        /// The negative-power path integrates instead, and is not what changed.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1002">#1002</a>
        /// </summary>
        [Fact] public void ANegativePowerStillIntegrates()
            => Assert.Equal("x ^ 2".ToEntity().Integrate(x), "x ^ 2".ToEntity().Differentiate(x, -1));

        /// <summary>
        /// **Regression guard for a stack overflow.** <see cref="Derivativef"/>'s simplification
        /// asks for the derivative and keeps the node when what comes back is still a
        /// <c>Derivativef</c> -- that test is what terminates it. Routing it through the public
        /// <c>Differentiate(Variable, int)</c>, which simplifies each pass, made it simplify the
        /// very node it was deciding about, arrive back at itself, and recurse until the stack
        /// ran out: <c>derivative(x!, x, 2)</c> overflowed after 3214 frames. The raw
        /// <c>InnerDifferentiate(Variable, int)</c> is what that caller needs, and the public
        /// overload is free to simplify.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1002">#1002</a>
        /// </summary>
        [Theory(Timeout = 30000)]
        // a derivative that cannot be taken at all, so the node survives every pass
        [InlineData("derivative(x!, x, 2)")]
        [InlineData("derivative(x!, x, 3)")]
        [InlineData("derivative(sin(x!) + x, x, 2)")]
        public void ADerivativeThatCannotBeTakenTerminatesAtEveryPower(string exprRaw)
        {
            var expr = exprRaw.ToEntity();
            Assert.NotNull(expr.InnerSimplified);
            Assert.NotNull(expr.Evaled);
        }

        /// <summary>
        /// And it is still refused rather than answered.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1002">#1002</a>
        /// </summary>
        [Fact] public void ADerivativeThatCannotBeTakenIsStillUnevaluated()
            => Assert.True("derivative(x!, x, 2)".ToEntity().InnerSimplified is Derivativef,
                $"expected it to stay a Derivativef, got {"derivative(x!, x, 2)".ToEntity().InnerSimplified}");
    }
}
