//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// The FE compiler conjugated arcsine unconditionally. That is right on the branch cuts --
    /// the real arguments outside [-1, 1], where the library does take the lower side and where
    /// the only test of it looked -- and wrong everywhere else, so compiled arcsine was the
    /// conjugate of the arcsine over the whole rest of the plane.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class CompiledArcsinBranchTest
    {
        private static readonly Entity.Variable x = MathS.Var(nameof(x));

        /// <summary>
        /// Both cuts, both sides of each, the interval between them, and points well away from
        /// the real axis.
        /// </summary>
        public static IEnumerable<object[]> Grid()
        {
            foreach (var re in new[] { -3.0, -1.5, -1.0, -0.5, 0.0, 0.5, 1.0, 1.5, 3.0 })
                foreach (var im in new[] { -2.0, -0.3, -1e-9, 0.0, 1e-9, 0.3, 2.0 })
                    yield return new object[] { re, im };
        }

        private static Complex Evaled(string expression, double re, double im)
        {
            var value = (Entity.Number.Complex)expression.ToEntity()
                .Substitute("x", MathS.Numbers.Create(EDecimal.FromDouble(re), EDecimal.FromDouble(im)))
                .EvalNumerical();
            return new Complex(value.RealPart.EDecimal.ToDouble(), value.ImaginaryPart.EDecimal.ToDouble());
        }

        /// <summary>
        /// Whatever the compilers answer, they have to answer what the library itself does.
        /// Before the fix the FE compiler agreed at 15 of these 63 points and the Linq compiler
        /// at 59; the 4 the latter missed are the cut, which it took the other side of.
        /// </summary>
        [Theory]
        [MemberData(nameof(Grid))]
        public void BothCompilersAgreeWithEvaledOnArcsine(double re, double im)
        {
            var z = new Complex(re, im);
            var expected = Evaled("arcsin(x)", re, im);
            Assert.True(Complex.Abs(expected - "arcsin(x)".Compile(x).Call(z)) < 1e-6,
                $"FE compiler at {z}: expected {expected}, got {"arcsin(x)".Compile(x).Call(z)}");
            Assert.True(Complex.Abs(expected - "arcsin(x)".ToEntity().Compile<Complex, Complex>("x")(z)) < 1e-6,
                $"Linq compiler at {z}: expected {expected}");
        }

        /// <summary>
        /// Arccosecant is arcsine of the reciprocal, so it lands on the same cuts -- for it, at
        /// the real arguments strictly inside [-1, 1].
        /// </summary>
        [Theory]
        [MemberData(nameof(Grid))]
        public void BothCompilersAgreeWithEvaledOnArccosecant(double re, double im)
        {
            if (re == 0 && im == 0) return; // arccsc(0) is not a number to agree about
            var z = new Complex(re, im);
            var expected = Evaled("arccsc(x)", re, im);
            if (double.IsNaN(expected.Real) || double.IsInfinity(expected.Real)) return;
            Assert.True(Complex.Abs(expected - "arccsc(x)".Compile(x).Call(z)) < 1e-6,
                $"FE compiler at {z}: expected {expected}");
            Assert.True(Complex.Abs(expected - "arccsc(x)".ToEntity().Compile<Complex, Complex>("x")(z)) < 1e-6,
                $"Linq compiler at {z}: expected {expected}");
        }

        /// <summary>
        /// The defining property of an inverse, which a conjugated arcsine does not have:
        /// sin(arcsin(0.5 + 0.1i)) came back as 0.5 - 0.1i.
        /// </summary>
        [Theory]
        [InlineData(0.5, 0.1)]
        [InlineData(-0.5, 0.1)]
        [InlineData(0.5, -0.1)]
        [InlineData(0.0, 2.0)]
        [InlineData(3.0, 0.0)]
        public void SineUndoesCompiledArcsine(double re, double im)
        {
            var z = new Complex(re, im);
            Assert.True(Complex.Abs(z - "sin(arcsin(x))".Compile(x).Call(z)) < 1e-9,
                $"at {z}: got {"sin(arcsin(x))".Compile(x).Call(z)}");
        }

        /// <summary>
        /// arcsin + arccos is pi/2 everywhere, not only on the real axis. The existing
        /// CompilationFETest case for this pair only ever looked at x = 3, where the two errors
        /// cancelled and the sum came out right for the wrong reason.
        /// </summary>
        [Theory]
        [InlineData(0.5, 0.1)]
        [InlineData(-0.5, 0.1)]
        [InlineData(0.0, 2.0)]
        [InlineData(3.0, 0.0)]
        public void ArcsineAndArccosineStillAddToARightAngle(double re, double im) =>
            Assert.True(
                Complex.Abs(new Complex(Math.PI / 2, 0) - "arcsin(x) + arccos(x)".Compile(x).Call(new Complex(re, im))) < 1e-9,
                $"at {re}+{im}i: got {"arcsin(x) + arccos(x)".Compile(x).Call(new Complex(re, im))}");

        /// <summary>
        /// The side of the cut the library takes, kept as it was. This is the one thing the old
        /// unconditional conjugate got right, and it stays right: arcsin(3) is pi/2 - 1.7627i
        /// here, where System.Numerics.Complex.Asin gives pi/2 + 1.7627i.
        /// </summary>
        [Theory]
        [InlineData(3.0)]
        [InlineData(-3.0)]
        [InlineData(1.5)]
        public void TheLibrarysSideOfTheCutIsUnchanged(double re)
        {
            var compiled = "arcsin(x)".Compile(x).Call(new Complex(re, 0));
            Assert.True(compiled.Imaginary < 0, $"arcsin({re}) came out as {compiled}");
            Assert.True(Complex.Abs(Evaled("arcsin(x)", re, 0) - compiled) < 1e-9);
        }
    }
}
