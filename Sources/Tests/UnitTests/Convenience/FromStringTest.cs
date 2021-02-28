using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System;
using System.Linq;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using AngouriMath.Extensions;

namespace UnitTests.Convenience
{
    public sealed class FromStringTest
    {
        public static readonly Entity.Variable x = Var(nameof(x));
        public static readonly Entity.Variable y = Var(nameof(y));
        public static readonly Entity.Variable z = Var(nameof(z));

        [Theory]
        [InlineData("", "line 1:0")]
        [InlineData(" ", "line 1:1")]
        [InlineData("\t", "line 1:1")]
        [InlineData("  ", "line 1:2")]
        [InlineData("//", "line 1:0")]
        [InlineData("//\n", "line 2:0")]
        [InlineData("/**/", "line 1:4")]
        [InlineData("a+", "line 1:2")]
        [InlineData("*a", "line 1:0")]
        [InlineData("a*a_", "line 1:3")]
        [InlineData("+!", "line 1:1")]
        [InlineData("_", "line 1:0")]
        [InlineData("()", "line 1:1")]
        public void Error(string input, string errorPrefix) =>
            Assert.StartsWith(errorPrefix, 
                Assert.Throws<UnhandledParseException>(() => (Entity)input).Message);
        [Theory]
        [InlineData("limitleft()", "limitleft should have exactly 3 arguments but 0 arguments are provided")]
        [InlineData("derivative(3)", "derivative should have exactly 3 arguments or 2 arguments but 1 argument is provided")]
        [InlineData("integral(3)", "integral should have exactly 3 arguments or 2 arguments but 1 argument is provided")]
        [InlineData("ln(3, 5)", "ln should have exactly 1 argument but 2 arguments are provided")]
        [InlineData("sin(3, 5, 8)", "sin should have exactly 1 argument but 3 arguments are provided")]
        [InlineData("log()", "log should have exactly 1 argument or 2 arguments but 0 arguments are provided")]
        [InlineData("log(1, 1, 1)", "log should have exactly 1 argument or 2 arguments but 3 arguments are provided")]
        public void WrongNumbersOfArgs(string input, string message) =>
            Assert.Equal(message, Assert.Throws<FunctionArgumentCountException>(() => (Entity)input).Message);
        [Theory]
        [InlineData("x+1")]
        [InlineData("sin(x)")]
        [InlineData("2s")]
        public void NotAVariable(string input) =>
            Assert.Throws<CannotParseInstanceException>(() => (Entity.Variable)input);
        [Fact] public void TestFormula1() => Assert.Equal(2, FromString("1 + 1").EvalNumerical());
        [Fact] public void TestFormula2() => Assert.Equal(0, FromString("sin(0)").EvalNumerical());
        [Fact] public void TestFormula3() => Assert.Equal(2, FromString("log(2, 4)").EvalNumerical());
        [Fact] public void TestFormula4() => Assert.Equal(2, FromString("log(100)").EvalNumerical());
        [Fact] public void TestFormula5() => Assert.Equal(Cos(x), FromString("sin(x)").Differentiate(x).Simplify());
        [Fact] public void TestFormula6() => Assert.Equal(7625597484987L, FromString("3 ^ 3 ^ 3").EvalNumerical());
        [Fact] public void TestFormula7() => Assert.Equal(6, FromString("2 + 2 * 2").EvalNumerical());
        [Fact] public void TestFormula8() =>
            // Only needed for Mac
            Settings.PrecisionErrorZeroRange.As(2e-16m, () =>
                Assert.Equal(i, FromString("x^2+1").SolveNt(x).First())
            );
        [Fact] public void TestFormula9() => Assert.Equal(1, FromString("cos(sin(0))").EvalNumerical());
        [Fact] public void TestFormula10() => Assert.Equal(Entity.Number.Complex.Create(4, 1), FromString("2i + 2 * 2 - 1i").EvalNumerical());
        [Fact] public void TestFormula11() => Assert.Equal(-1, FromString("i^2").EvalNumerical());
        [Fact] public void TestFormula12() => Assert.Equal(3, FromString("x^2-1").Substitute(x, 2).EvalNumerical());
        [Fact] public void TestFormula13() => Assert.Equal(DecimalConst.pi / 2, FromString("arcsin(x)").Substitute(x, 1).EvalNumerical());
        [Fact] public void TestFormula14() => Assert.Equal(Sqr(x), FromString("x2"));
        [Fact] public void TestFormula15() =>  Assert.Equal(2 * x, FromString("2x"));
        [Fact] public void TestFormula16() => Assert.Equal(9, FromString("3 2").EvalNumerical());
        [Fact] public void TestFormula17() => Assert.Equal(5 * x, FromString("x(2 + 3)").Simplify());
        [Fact] public void TestFormula18() => Assert.Equal(x * y, FromString("x y"));
        [Fact] public void TestFormula19() => Assert.Equal(x * MathS.Sqrt(3), FromString("x sqrt(3)"));
        [Fact] public void TestFormula20() => Assert.Equal(Factorial(x), FromString("x!"));
        [Fact] public void TestFormula21() => Assert.Throws<UnhandledParseException>(() => FromString("x!!"));
        [Fact] public void TestFormula22() => Assert.Equal(Factorial(Sin(x)), FromString("sin(x)!"));
        [Fact] public void TestFormula23() => Assert.Equal(Pow(2, MathS.Factorial(3)), FromString("2^3!"));
        [Fact] public void TestFormula24() => Assert.Equal(Pow(MathS.Factorial(2), MathS.Factorial(3)), FromString("2!^3!"));
        [Fact] public void TestFormula25() => Assert.Equal(Pow(MathS.Factorial(2), Factorial(x + 2)), FromString("2!^(x+2)!"));
        [Fact] public void TestFormula26() => Assert.Equal(-MathS.Factorial(1), FromString("-1!"));
        [Fact] public void TestFormula27() => Assert.Equal(MathS.Factorial("-1"), FromString("(-1)!"));
        [Fact] public void TestFormulaSys() => Assert.Equal(Sqr(x), FromString("x2"));
        [Fact] public void TestNode28() => Assert.Equal(Derivative("x + 1", x), FromString("derivative(x + 1, x, 1)"));
        [Fact] public void TestNode29() => Assert.Equal(Derivative("x + 1", x, 5), FromString("derivative(x + 1, x, 5)"));
        [Fact] public void TestNode30() => Assert.Equal(Integral("x + 1", x), FromString("integral(x + 1, x, 1)"));
        [Fact] public void TestNode31() => Assert.Equal(Integral("x + y", x, 3), FromString("integral(x + y, x, 3)"));
        [Fact] public void TestNode32() => Assert.Equal(Limit("x + y", x, 3), FromString("limit(x + y, x, 3)"));
        [Fact] public void TestNode33() => Assert.Equal(Limit("x + y", x, 3, ApproachFrom.Left), FromString("limitleft(x + y, x, 3)"));
        [Fact] public void TestNode34() => Assert.Equal(Signum("x"), FromString("signum(x)"));
        [Fact] public void TestNode35() => Assert.Equal(Abs("x"), FromString("abs(x)"));
        [Fact] public void TestMultipleUnary36() => Assert.Equal(-1 * (-1 * (-1 * x)), FromString("---x"));
        [Fact] public void TestMultipleUnary37() => Assert.Equal(-1 * (-1 * (-1 * x)), FromString("-++-+-+x"));
        [Fact] public void TestMultipleUnary38() => Assert.Equal(!!!x, FromString("not not not x"));
        [Fact] public void TestBool39() => Assert.Equal(x & x | x.Implies(x), FromString("x and x or (x -> x)"));
        [Fact] public void TestBool40() => Assert.Equal(x & x & x & x, FromString("x and x and x and x"));
        [Fact] public void TestBool41() => Assert.Equal(x | x | x | x, FromString("x or x or x or x"));
        [Fact] public void TestBoolConstants1() => Assert.Equal(Entity.Boolean.True, FromString("true"));
        [Fact] public void TestBoolConstants2() => Assert.Equal(Entity.Boolean.True, FromString("True"));
        [Fact] public void TestBoolConstants3() => Assert.Equal(Entity.Boolean.False, FromString("false"));
        [Fact] public void TestBoolConstants4() => Assert.Equal(Entity.Boolean.False, FromString("False"));
        [Fact] public void TestAbs42() => Assert.Equal(Abs(x), FromString("(|x|)"));
        [Fact] public void TestAbs43() => Assert.Equal(Abs(Abs(x) + 2).Pow(2), FromString("(|(|x|) + 2|) ^ 2"));
        [Fact] public void TestCoDomain44() => Assert.Equal(Sqrt(x).WithCodomain(Domain.Real), FromString("domain(sqrt(x), RR)"));
        [Fact] public void TestCoDomain45() => Assert.Equal(Sqrt(x).WithCodomain(Domain.Real), FromString("domain(domain(sqrt(x), RR), RR)"));
        [Fact] public void TestCoDomain46() => Assert.Equal(Sqrt(x).WithCodomain(Domain.Complex), FromString("domain(domain(sqrt(x), RR), CC)"));
        [Fact] public void TestInequality47() => Assert.Equal(x > y, FromString("x > y"));
        [Fact] public void TestInequality48() => Assert.Equal(x < y, FromString("x < y"));
        [Fact] public void TestInequality49() => Assert.Equal(x >= y, FromString("x >= y"));
        [Fact] public void TestInequality50() => Assert.Equal(x <= y, FromString("x <= y"));
        [Fact] public void TestInequality51() => Assert.Equal(x.Equalizes(y), FromString("x = y"));
        [Fact] public void TestInequality52() => Assert.Equal((x > y).Equalizes(x < y), FromString("x > y = x < y"));
        [Fact] public void TestInterval1() => Assert.Equal(new Interval(x, true, y, true), FromString("[x; y]"));
        [Fact] public void TestInterval2() => Assert.Equal(new Interval(x, false, y, true), FromString("(x; y]"));
        [Fact] public void TestInterval3() => Assert.Equal(new Interval(x, true, y, false), FromString("[x; y)"));
        [Fact] public void TestInterval4() => Assert.Equal(new Interval(x, false, y, false), FromString("(x; y)"));
        [Fact] public void TestFiniteSet1() => Assert.Equal(new FiniteSet(x, y, 3), FromString("{ x, y, 3 }"));
        [Fact] public void TestFiniteSet2() => Assert.Equal(new FiniteSet(), FromString("{}"));
        [Fact] public void TestCSet1() => Assert.Equal(new ConditionalSet(x, "x + 3 > 0"), FromString("{ x : x + 3 > 0 }"));
        [Fact] public void TestCSet2() => Assert.Equal(new ConditionalSet(x, "x + 3 > 0 and x / 2 < y2"), FromString("{ x : x + 3 > 0 and x / 2 < y2 }"));
        [Fact] public void TestUnion1() => Assert.Equal(x.Unite(y), FromString(@"x \/ y"));
        [Fact] public void TestUnion2() => Assert.Equal(x.Unite(y), FromString(@"x unite y"));
        [Fact] public void TestIntersection1() => Assert.Equal(x.Intersect(y), FromString(@"x /\ y"));
        [Fact] public void TestIntersection2() => Assert.Equal(x.Intersect(y), FromString(@"x intersect y"));
        [Fact] public void TestSetSubtraction1() => Assert.Equal(x.SetSubtract(y), FromString(@"x \ y"));
        [Fact] public void TestSetSubtraction2() => Assert.Equal(x.SetSubtract(y), FromString(@"x setsubtract y"));
        [Fact] public void TestPlusInfinity1() => Assert.Equal(Real.PositiveInfinity, FromString("+oo"));
        [Fact] public void TestPlusInfinity2() => Assert.Equal(Real.PositiveInfinity + (Entity)2, FromString("+oo + 2"));
        [Fact] public void TestMinusInfinity1() => Assert.Equal(Real.NegativeInfinity, FromString("-oo"));
        [Fact] public void TestMinusInfinity2() => Assert.Equal(Real.NegativeInfinity + (Entity)2, FromString("-oo + 2"));
        [Fact] public void TestEquality1() => Assert.Equal(x.Equalizes(y) & y.Equalizes(x), FromString("x = y = x"));
        [Fact] public void TestEquality2() => Assert.Equal(x.Equalizes(y).Equalizes(x), FromString("(x = y) = x"));
        [Fact] public void TestDerivative2Args1() => Assert.Equal(MathS.Derivative("x + 2", "x"), FromString("derivative(x + 2, x)"));
        [Fact] public void TestIntegral2Args1() => Assert.Equal(MathS.Integral("x + 2", "x"), FromString("integral(x + 2, x)"));
        [Fact] public void TestDerivative2Args2() => Assert.Equal(2 * MathS.Derivative("x + 2", "x"), FromString("2 derivative(x + 2, x)"));
        [Fact] public void TestIntegral2Args2() => Assert.Equal(2 * MathS.Integral("x + 2", "x"), FromString("2 integral(x + 2, x)"));
        [Fact] public void TestSec() => Assert.Equal(MathS.Sec("x"), FromString("sec(x)"));
        [Fact] public void TestCosec() => Assert.Equal(MathS.Cosec("x"), FromString("csc(x)"));
        [Fact] public void TestArcsec() => Assert.Equal(MathS.Arcsec("x"), FromString("arcsec(x)"));
        [Fact] public void TestArccosec() => Assert.Equal(MathS.Arccosec("x"), FromString("arccsc(x)"));
        [Fact] public void TestProvided1() => Assert.Equal(MathS.Provided("a", "b"), FromString("a provided b"));
        [Fact] public void TestProvided2() => Assert.Equal(MathS.Provided(MathS.Provided("a", "b"), "c"), FromString("a provided b provided c"));

        [Theory]
        [InlineData("sinh", "Sinh")]
        [InlineData("cosh", "Cosh")]
        [InlineData("tanh", "Tanh")]
        [InlineData("coth", "Cotanh")]
        [InlineData("sech", "Sech")]
        [InlineData("csch", "Cosech")]
        [InlineData("asinh", "Arcsinh")]
        [InlineData("acosh", "Arccosh")]
        [InlineData("atanh", "Arctanh")]
        [InlineData("acoth", "Arccotanh")]
        [InlineData("asech", "Arcsech")]
        [InlineData("acsch", "Arccosech")]
        public void TestHyperbolic(string parsedName, string methodName)
        {
            var methods = typeof(MathS.Hyperbolic).GetMethods();
            var withMethods = methods.Where(c => c.Name == methodName);
            if (withMethods.Count() != 1)
                throw new InvalidOperationException($"Incorrect method's name: {methodName}");
            var method = withMethods.First();
            Entity x = "x";
            var runned = method.Invoke(null, new []{ x });
            if (runned is not Entity expected)
                throw new InvalidOperationException("Method returned something weird -___-");
            var actual = MathS.FromString(parsedName + "(x)");
            Assert.Equal(expected, actual);
        }

        [Fact] public void TestInvalidArg1() => Assert.Throws<FunctionArgumentCountException>(() => FromString("integral(x)"));
        [Fact] public void TestInvalidArg2() => Assert.Throws<FunctionArgumentCountException>(() => FromString("integral(24)"));
        [Fact] public void TestInvalidArg3() => Assert.Throws<FunctionArgumentCountException>(() => FromString("integral(x, x, 4, x)"));
        [Fact] public void TestInvalidArg4() => Assert.Throws<FunctionArgumentCountException>(() => FromString("integral(x, x, x, x)"));
        [Fact] public void TestInvalidArg5() => Assert.Throws<InvalidArgumentParseException>(() => FromString("integral(x, x, a)"));
        [Fact] public void TestInvalidArg6() => Assert.Throws<FunctionArgumentCountException>(() => FromString("derivative(x)"));
        [Fact] public void TestInvalidArg7() => Assert.Throws<FunctionArgumentCountException>(() => FromString("derivative(24)"));
        [Fact] public void TestInvalidArg8() => Assert.Throws<FunctionArgumentCountException>(() => FromString("derivative(x, x, 4, x)"));
        [Fact] public void TestInvalidArg9() => Assert.Throws<FunctionArgumentCountException>(() => FromString("derivative(x, x, x, x)"));
        [Fact] public void TestInvalidArg10() => Assert.Throws<InvalidArgumentParseException>(() => FromString("derivative(x, x, a)"));
        [Fact] public void TestPowerUnary1() => Assert.Equal(Pow(x, "-5"), FromString("x^-5"));
        [Fact] public void TestPowerUnary2() => Assert.Equal(Pow(x, -x), FromString("x^-x"));
        [Fact] public void TestPowerUnary3() => Assert.Equal(Pow(x, -Pow(x, x)), FromString("x^ -x^x"));
        [Fact] public void TestPiecewise1() => Assert.Equal(Piecewise((x, y), (y, x), (x + 2, y + 2)), FromString("piecewise(x provided y, y provided x, x + 2 provided y + 2)"));
        [Fact] public void TestPiecewise2() => Assert.Equal(Piecewise(new[] { new Providedf(x, y), new Providedf(y, x), new Providedf(x + 2, y + 2) }, 3), FromString("piecewise(x provided y, y provided x, x + 2 provided y + 2, 3)"));
        [Fact] public void TestNegativeNumberParsing1() => Assert.Equal(-1, FromString("-1"));
        [Fact] public void TestNegativeNumberParsing2() => Assert.Equal(-234.2m, FromString("-234.2"));
        [Fact] public void TestNegativeNumberParsing3() => Assert.Equal(234.2m, FromString("+234.2"));
        [Fact] public void TestNegativeNumberParsing4() => Assert.Equal((Entity)(-1).ToNumber() * (Entity)(-2).ToNumber(), FromString("--2"));

        private (Entity xy, Entity xyz, Entity yz, string str) Extract(string signLeft, string signRight)
        {
            var s = $"x {signLeft} y {signRight} z";
            var nodeXY = signLeft switch
            {
                "<" => x < y,
                ">" => x > y,
                "<=" => x <= y,
                ">=" => x >= y,
                _ => throw new Exception("What the hell???")
            };
            var nodeXYZ = signRight switch
            {
                "<" => nodeXY < z,
                ">" => nodeXY > z,
                "<=" => nodeXY <= z,
                ">=" => nodeXY >= z,
                _ => throw new Exception("What the hell???")
            };
            var nodeYZ = signRight switch
            {
                "<" => y < z,
                ">" => y > z,
                "<=" => y <= z,
                ">=" => y >= z,
                _ => throw new Exception("What the hell???")
            };
            return (nodeXY, nodeXYZ, nodeYZ, s);
        }

        [Theory, CombinatorialData]
        public void TestInequalityCompositionCircle(
        [CombinatorialValues("<", ">", "<=", ">=")] string signLeft,
        [CombinatorialValues("<", ">", "<=", ">=")] string signRight)
        {
            var (_, nodeXYZ, _, str) = Extract(signLeft, signRight);
            Assert.Equal(nodeXYZ, FromString(str));
        }

        [Theory, CombinatorialData]
        public void TestInequalityCompositionPrettySyntax(
        [CombinatorialValues("<", ">", "<=", ">=")] string signLeft,
        [CombinatorialValues("<", ">", "<=", ">=")] string signRight)
        {
            var (nodeXY, _, nodeYZ, str) = Extract(signLeft, signRight);
            Assert.Equal(nodeXY & nodeYZ, FromString(str));
        }
    }
}

