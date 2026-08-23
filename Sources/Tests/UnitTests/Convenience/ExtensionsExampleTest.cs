//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Globalization;
using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Compilation.IntoLinq;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// Every public member of <see cref="AngouriMath.Extensions"/> documents a worked example
    /// that names the output it produces. A documented output is a promise the library makes,
    /// so each one is asserted here against the value the member actually returns; a change of
    /// answer then fails a test instead of leaving a wrong claim standing in the XML docs.
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class ExtensionsExampleTest
    {
        /// <summary>
        /// What <c>Console.WriteLine</c> would put on the screen, which is what the examples show.
        /// </summary>
        private static void Printed(string expected, object? actual)
            => Assert.Equal(expected, actual?.ToString());

        private static void Printed(string expected, double actual)
            => Assert.Equal(expected, actual.ToString(CultureInfo.InvariantCulture));

        [Fact] public void ConcatToTheRight()
        {
            var a = MathS.Matrix(new Entity[,] { { 1, 2 }, { 3, 4 } });
            var b = MathS.Matrix(new Entity[,] { { 5 }, { 6 } });
            Printed("[[1, 2, 5], [3, 4, 6]]", a.ConcatToTheRight(b));
        }

        [Fact] public void ConcatToTheBottom()
        {
            var a = MathS.Matrix(new Entity[,] { { 1, 2 }, { 3, 4 } });
            var b = MathS.Matrix(new Entity[,] { { 5, 6 } });
            Printed("[[1, 2], [3, 4], [5, 6]]", a.ConcatToTheBottom(b));
        }

        [Fact] public void ToVector()
        {
            var v = new Entity[] { "x", "y", "z" }.ToVector();
            Printed("[x, y, z]", v);
            Printed("3", v.RowCount);
            Printed("1", v.ColumnCount);
        }

        [Fact] public void SumAll()
        {
            Printed("1 + 2 + 3 + 4", new Entity[] { 1, 2, 3, 4 }.SumAll());
            Printed("10", new Entity[] { 1, 2, 3, 4 }.SumAll().Evaled);
        }

        [Fact] public void MultiplyAll()
        {
            var factorial = Enumerable.Range(1, 5).Select(i => (Entity)i).MultiplyAll();
            Printed("1 * 2 * 3 * 4 * 5", factorial);
            Printed("120", factorial.EvalNumerical());
        }

        [Fact] public void ToPiecewise()
        {
            var abs = new[]
            {
                new Providedf("x", "x > 0"),
                new Providedf("-x", "x <= 0")
            }.ToPiecewise();
            Printed("piecewise(x provided (x > 0), (-x) provided (x <= 0))", abs);
            Printed("3", abs.Substitute("x", -3).Simplify());
        }

        [Fact] public void ToProvided()
            => Printed("1 / x provided not x = 0", ((Entity)"1 / x", (Entity)"x <> 0").ToProvided());

        [Fact] public void ToSet()
        {
            Printed("{ 1, 2, x }", new Entity[] { 1, 2, 2, "x", "x" }.ToSet());
            Printed("{ x + x, 2 * x }", new Entity[] { "x + x", "2x" }.ToSet());
        }

        [Fact] public void Unite()
        {
            var union = new Set[] { "[0; 1]", "[1; 2]" }.Unite();
            Printed(@"[0; 1] \/ [1; 2]", union);
            Printed("[0; 2]", union.Simplify());
        }

        [Fact] public void Intersect()
        {
            var meet = new Set[] { "{ 1, 2 }", "{ 2, 3 }" }.Intersect();
            Printed(@"{ 1, 2 } /\ { 2, 3 }", meet);
            Printed("{ 2 }", meet.Simplify());
            Printed("{  }", new Set[] { }.Intersect());
        }

        [Fact] public void ToEntityFromString()
        {
            Printed("2 + 3", "2 + 3".ToEntity());
            Printed("5", "2 + 3".ToEntity().Evaled);
            Printed("x ^ 2", "x2".ToEntity());
        }

        [Fact] public void ToEntityFromTuple()
        {
            var iv = ((Entity)0, false, (Entity)"pi", true).ToEntity();
            Printed("(0; pi]", iv);
            Printed("False", iv.Contains(0));
            Printed("True", iv.Contains("pi"));
        }

        [Fact] public void SimplifyString()
        {
            Printed("1", "sin(x) ^ 2 + cos(x) ^ 2".Simplify());
            Printed("4 * x", "(x + 1) ^ 2 - (x - 1) ^ 2".Simplify());
        }

        [Fact] public void SimplifyStringLevel()
        {
            Printed("a / (b / c)", "a / (b / c)".Simplify(0));
            Printed("a * c / b", "a / (b / c)".Simplify(1));
        }

        [Fact] public void EvalNumericalString()
        {
            Printed("1267650600228229401496703205376", "2 ^ 100".EvalNumerical());
            Printed("i", "sqrt(-1)".EvalNumerical());
        }

        [Fact] public void EvalBooleanString()
        {
            Printed("False", "true implies false".EvalBoolean());
            Printed("True", "3 > 2 and 2 > 1".EvalBoolean());
        }

        [Fact] public void ExpandString()
        {
            Printed("1 + 3 * x + 3 * x ^ 2 + x ^ 3", "(x + 1) ^ 3".Expand());
            Printed("x ^ 2 + 3 * x + 2", "(x + 1)(x + 2)".Expand());
        }

        [Fact] public void FactorizeString()
        {
            Printed("(a - b) * (a + b)", "a2 - b2".Factorize());
            Printed("x ^ 2 + 2 * x + 1", "x2 + 2x + 1".Factorize());
        }

        [Fact] public void SubstituteOne()
        {
            Printed("3 ^ 2 + 3", "x ^ 2 + x".Substitute("x", 3));
            Printed("12", "x ^ 2 + x".Substitute("x", 3).Evaled);
        }

        [Fact] public void SubstituteTwo()
            => Printed("x - x", "x - y".Substitute(("x", "y"), ("y", "x")));

        [Fact] public void SubstituteThree()
        {
            var quadratic = "a x2 + b x + c".Substitute(("a", "b", "c"), (1, -3, 2));
            Printed("1 * x ^ 2 + (-3) * x + 2", quadratic);
            Printed("{ 1, 2 }", quadratic.SolveEquation("x"));
        }

        [Fact] public void SubstituteFour()
        {
            Printed("1 + 2 + 3 + 4", "a + b + c + d".Substitute(("a", "b", "c", "d"), (1, 2, 3, 4)));
            Printed("10", "a + b + c + d".Substitute(("a", "b", "c", "d"), (1, 2, 3, 4)).Evaled);
        }

        [Fact] public void SolveEquationString()
            => Printed("{ 1, -1/2 + i * 1/2 * sqrt(3), -1/2 + i * -1/2 * sqrt(3) }", "x ^ 3 - 1".SolveEquation("x"));

        [Fact] public void SolveString()
        {
            Printed("{ 2, -2 }", "x2 = 4".Solve("x"));
            Printed("(1; 3)", "x > 1 and x < 3".Solve("x"));
        }

        [Fact] public void ToNumberInt()
        {
            Printed("5/2", 5.ToNumber() / 2.ToNumber());
            Printed("Rational", (5.ToNumber() / 2.ToNumber()).GetType().Name);
        }

        [Fact] public void ToNumberLong()
        {
            Printed("9223372036854775807", long.MaxValue.ToNumber());
            Printed("85070591730234615847396907784232501249", (long.MaxValue.ToNumber() * long.MaxValue.ToNumber()).Evaled);
        }

        [Fact] public void ToNumberEInteger()
            => Printed("340282366920938463463374607431768211456", EInteger.FromString("2").Pow(128).ToNumber());

        [Fact] public void ToNumberFloat()
            => Printed("0.100000001490116119384765625", 0.1f.ToNumber());

        [Fact] public void ToNumberDouble()
        {
            Printed("0.1000000000000000055511151231257827021181583404541015625", 0.1.ToNumber());
            Printed("1/2", 0.5.ToNumber());
        }

        [Fact] public void ToNumberDecimal()
        {
            Printed("1/10", 0.1m.ToNumber());
            Printed("0.1000000000000000055511151231257827021181583404541015625", 0.1.ToNumber());
        }

        [Fact] public void ToNumberEDecimal()
            => Printed("1/10", EDecimal.FromString("0.1").ToNumber());

        [Fact] public void ToNumberComplex()
        {
            var z = new System.Numerics.Complex(3, 4).ToNumber();
            Printed("3 + 4i", z);
            Printed("5", z.Abs().Evaled);
        }

        [Fact] public void ToBooleanBool()
        {
            Printed("True and False", true.ToBoolean() & false.ToBoolean());
            Printed("False", (true.ToBoolean() & false.ToBoolean()).EvalBoolean());
        }

        [Fact] public void LatexizeString()
        {
            Printed(@"\frac{\sqrt{x}}{2}", "sqrt(x) / 2".Latexize());
            Printed(@"\int {x}^{2}\,\mathrm{d}x", "integral(x ^ 2, x)".Latexize());
        }

        [Fact] public void CompileToFastExpression()
        {
            var f = "x ^ 2 + 1".Compile("x");
            Printed("10", f.Call(3).Real);
            Printed("0", f.Call(new System.Numerics.Complex(0, 1)).Real);
        }

        [Fact] public void DifferentiateString()
        {
            Printed("3 * x ^ 2", "x ^ 3".Differentiate("x"));
            Printed("(cos(x) * x - sin(x)) / x ^ 2", "sin(x) / x".Differentiate("x"));
        }

        [Fact] public void IntegrateIndefiniteString()
        {
            Printed("ln(x) + C", "1 / x".Integrate("x"));
            Printed("integral(e ^ x ^ 2, x)", "e ^ (x ^ 2)".Integrate("x"));
        }

        [Fact] public void IntegrateDefiniteString()
        {
            Printed("-cos(pi) - -cos(0)", "sin(x)".Integrate("x", 0, "pi"));
            Printed("2", "sin(x)".Integrate("x", 0, "pi").Simplify());
        }

        [Fact] public void LimitSidedString()
        {
            Printed("-oo", "1 / x".Limit("x", 0, ApproachFrom.Left));
            Printed("+oo", "1 / x".Limit("x", 0, ApproachFrom.Right));
            Printed("NaN", "1 / x".Limit("x", 0, ApproachFrom.BothSides));
        }

        [Fact] public void LimitString()
        {
            Printed("1", "sin(x) / x".Limit("x", 0));
            Printed("e", "(1 + 1/x) ^ x".Limit("x", "+oo"));
        }

        // The generated overloads of AdditionalExtensions.cs and CompilationExtensions.cs.
        // Their examples come out of one template each, so a template that starts lying
        // starts lying in every overload at once, and these pin all of them.

        [Fact] public void SolveSystem2()
            => Printed("[[1, 2], [-1, -2]]",
                ("a^2 - 1", "b - 2 * a").SolveSystem("a", "b"));

        [Fact] public void SolveSystem3()
            => Printed("[[1, 2, 3], [-1, -2, -3]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a").SolveSystem("a", "b", "c"));

        [Fact] public void SolveSystem4()
            => Printed("[[1, 2, 3, 4], [-1, -2, -3, -4]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a", "d - 4 * a").SolveSystem("a", "b", "c", "d"));

        [Fact] public void SolveSystem5()
            => Printed("[[1, 2, 3, 4, 5], [-1, -2, -3, -4, -5]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a", "d - 4 * a", "f - 5 * a").SolveSystem("a", "b", "c", "d", "f"));

        [Fact] public void SolveSystem6()
            => Printed("[[1, 2, 3, 4, 5, 6], [-1, -2, -3, -4, -5, -6]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a", "d - 4 * a", "f - 5 * a", "g - 6 * a").SolveSystem("a", "b", "c", "d", "f", "g"));

        [Fact] public void SolveSystem7()
            => Printed("[[1, 2, 3, 4, 5, 6, 7], [-1, -2, -3, -4, -5, -6, -7]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a", "d - 4 * a", "f - 5 * a", "g - 6 * a", "h - 7 * a").SolveSystem("a", "b", "c", "d", "f", "g", "h"));

        [Fact] public void SolveSystem8()
            => Printed("[[1, 2, 3, 4, 5, 6, 7, 8], [-1, -2, -3, -4, -5, -6, -7, -8]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a", "d - 4 * a", "f - 5 * a", "g - 6 * a", "h - 7 * a", "j - 8 * a").SolveSystem("a", "b", "c", "d", "f", "g", "h", "j"));

        [Fact] public void SolveSystem9()
            => Printed("[[1, 2, 3, 4, 5, 6, 7, 8, 9], [-1, -2, -3, -4, -5, -6, -7, -8, -9]]",
                ("a^2 - 1", "b - 2 * a", "c - 3 * a", "d - 4 * a", "f - 5 * a", "g - 6 * a", "h - 7 * a", "j - 8 * a", "k - 9 * a").SolveSystem("a", "b", "c", "d", "f", "g", "h", "j", "k"));

        [Fact] public void ToIntervalClosed()
        {
            Printed("[2; 5]", (2, 5).ToInterval());
            Printed("[2; 9/2]", (2, 4.5).ToInterval());
            Printed("[2; 9/2]", (2, 4.5f).ToInterval());
            Printed("[2; 2 * pi]", (2, "2 pi").ToInterval());
            Printed("[1/2; 5]", (0.5, 5).ToInterval());
            Printed("[1/2; 9/2]", (0.5, 4.5).ToInterval());
            Printed("[1/2; 9/2]", (0.5, 4.5f).ToInterval());
            Printed("[1/2; 2 * pi]", (0.5, "2 pi").ToInterval());
            Printed("[1/2; 5]", (0.5f, 5).ToInterval());
            Printed("[1/2; 9/2]", (0.5f, 4.5).ToInterval());
            Printed("[1/2; 9/2]", (0.5f, 4.5f).ToInterval());
            Printed("[1/2; 2 * pi]", (0.5f, "2 pi").ToInterval());
            Printed("[pi; 5]", ("pi", 5).ToInterval());
            Printed("[pi; 9/2]", ("pi", 4.5).ToInterval());
            Printed("[pi; 9/2]", ("pi", 4.5f).ToInterval());
            Printed("[pi; 2 * pi]", ("pi", "2 pi").ToInterval());
        }

        [Fact] public void ToIntervalHalfOpen()
        {
            Printed("(2; 5]", (2, false, 5, true).ToInterval());
            Printed("(2; 9/2]", (2, false, 4.5, true).ToInterval());
            Printed("(2; 9/2]", (2, false, 4.5f, true).ToInterval());
            Printed("(2; 2 * pi]", (2, false, "2 pi", true).ToInterval());
            Printed("(1/2; 5]", (0.5, false, 5, true).ToInterval());
            Printed("(1/2; 9/2]", (0.5, false, 4.5, true).ToInterval());
            Printed("(1/2; 9/2]", (0.5, false, 4.5f, true).ToInterval());
            Printed("(1/2; 2 * pi]", (0.5, false, "2 pi", true).ToInterval());
            Printed("(1/2; 5]", (0.5f, false, 5, true).ToInterval());
            Printed("(1/2; 9/2]", (0.5f, false, 4.5, true).ToInterval());
            Printed("(1/2; 9/2]", (0.5f, false, 4.5f, true).ToInterval());
            Printed("(1/2; 2 * pi]", (0.5f, false, "2 pi", true).ToInterval());
            Printed("(pi; 5]", ("pi", false, 5, true).ToInterval());
            Printed("(pi; 9/2]", ("pi", false, 4.5, true).ToInterval());
            Printed("(pi; 9/2]", ("pi", false, 4.5f, true).ToInterval());
            Printed("(pi; 2 * pi]", ("pi", false, "2 pi", true).ToInterval());
        }

        [Fact] public void CompileWithProtocol()
        {
            var f = "a / b".Compile<Func<int, int, int>>(
                new CompilationProtocol(),
                typeof(int),
                new[] { (typeof(int), (Variable)"a"), (typeof(int), (Variable)"b") });
            Printed("3", f(7, 2));
        }

        [Fact] public void CompileArity1()
            => Printed("10", "a".Compile<int, int>("a")(10));

        [Fact] public void CompileArity2()
            => Printed("30", "a + b".Compile<int, int, int>("a", "b")(10, 20));

        [Fact] public void CompileArity3()
            => Printed("60", "a + b + c".Compile<int, int, int, int>("a", "b", "c")(10, 20, 30));

        [Fact] public void CompileArity4()
            => Printed("100", "a + b + c + d".Compile<int, int, int, int, int>("a", "b", "c", "d")(10, 20, 30, 40));

        [Fact] public void CompileArity5()
            => Printed("150", "a + b + c + d + f".Compile<int, int, int, int, int, int>("a", "b", "c", "d", "f")(10, 20, 30, 40, 50));

        [Fact] public void CompileArity6()
            => Printed("210", "a + b + c + d + f + g".Compile<int, int, int, int, int, int, int>("a", "b", "c", "d", "f", "g")(10, 20, 30, 40, 50, 60));

        [Fact] public void CompileArity7()
            => Printed("280", "a + b + c + d + f + g + h".Compile<int, int, int, int, int, int, int, int>("a", "b", "c", "d", "f", "g", "h")(10, 20, 30, 40, 50, 60, 70));

        [Fact] public void CompileArity8()
            => Printed("360", "a + b + c + d + f + g + h + j".Compile<int, int, int, int, int, int, int, int, int>("a", "b", "c", "d", "f", "g", "h", "j")(10, 20, 30, 40, 50, 60, 70, 80));
    }
}
