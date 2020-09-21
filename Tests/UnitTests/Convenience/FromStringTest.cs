using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System.Linq;
using Xunit;
using static AngouriMath.MathS;

namespace UnitTests.Convenience
{
    public class FromStringTest
    {
        public static readonly Entity.Variable x = Var(nameof(x));
        public static readonly Entity.Variable y = Var(nameof(y));

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
                Assert.Throws<ParseException>(() => (Entity)input).Message);
        [Theory]
        [InlineData("limitleft()", "limitleft should have exactly 3 arguments but 0 arguments are provided")]
        [InlineData("derivative(3)", "derivative should have exactly 3 arguments but 1 argument is provided")]
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
            Assert.Throws<System.InvalidCastException>(() => (Entity.Variable)input);
        [Fact] public void Test1() => Assert.Equal(2, FromString("1 + 1").EvalNumerical());
        [Fact] public void Test2() => Assert.Equal(0, FromString("sin(0)").EvalNumerical());
        [Fact] public void Test3() => Assert.Equal(2, FromString("log(2, 4)").EvalNumerical());
        [Fact] public void Test4() => Assert.Equal(2, FromString("log(100)").EvalNumerical());
        [Fact] public void Test5() => Assert.Equal(Cos(x), FromString("sin(x)").Derive(x).Simplify());
        [Fact] public void Test6() => Assert.Equal(7625597484987L, FromString("3 ^ 3 ^ 3").EvalNumerical());
        [Fact] public void Test7() => Assert.Equal(6, FromString("2 + 2 * 2").EvalNumerical());
        [Fact] public void Test8() =>
            // Only needed for Mac
            Settings.PrecisionErrorZeroRange.As(2e-16m, () =>
                Assert.Equal(i, FromString("x^2+1").SolveNt(x).First())
            );
        [Fact] public void Test9() => Assert.Equal(1, FromString("cos(sin(0))").EvalNumerical());
        [Fact] public void Test10() => Assert.Equal(Entity.Number.Complex.Create(4, 1), FromString("2i + 2 * 2 - 1i").EvalNumerical());
        [Fact] public void Test11() => Assert.Equal(-1, FromString("i^2").EvalNumerical());
        [Fact] public void Test12() => Assert.Equal(3, FromString("x^2-1").Substitute(x, 2).EvalNumerical());
        [Fact] public void Test13() => Assert.Equal(DecimalConst.pi / 2, FromString("arcsin(x)").Substitute(x, 1).EvalNumerical());
        [Fact] public void Test14() => Assert.Equal(Sqr(x), FromString("x2"));
        [Fact] public void Test15() =>  Assert.Equal(2 * x, FromString("2x"));
        [Fact] public void Test16() => Assert.Equal(9, FromString("3 2").EvalNumerical());
        [Fact] public void Test17() => Assert.Equal(5 * x, FromString("x(2 + 3)").Simplify());
        [Fact] public void Test18() => Assert.Equal(x * y, FromString("x y"));
        [Fact] public void Test19() => Assert.Equal(x * Sqrt(3), FromString("x sqrt(3)"));
        [Fact] public void Test20() => Assert.Equal(Factorial(x), FromString("x!"));
        [Fact] public void Test21() => Assert.Throws<ParseException>(() => FromString("x!!"));
        [Fact] public void Test22() => Assert.Equal(Factorial(Sin(x)), FromString("sin(x)!"));
        [Fact] public void Test23() => Assert.Equal(Pow(2, Factorial(3)), FromString("2^3!"));
        [Fact] public void Test24() => Assert.Equal(Pow(Factorial(2), Factorial(3)), FromString("2!^3!"));
        [Fact] public void Test25() => Assert.Equal(Pow(Factorial(2), Factorial(x + 2)), FromString("2!^(x+2)!"));
        [Fact] public void Test26() => Assert.Equal(-Factorial(1), FromString("-1!"));
        [Fact] public void Test27() => Assert.Equal(Factorial(-1).ToString(), FromString("(-1)!"));
        [Fact] public void TestSys() => Assert.Equal(Sqr(x), FromString("x2"));
        [Fact] public void Test28() 
            => Assert.Equal(Derivative("x + 1", x), FromString("derivative(x + 1, x, 1)"));
        [Fact] public void Test29() 
            => Assert.Equal(Derivative("x + 1", x, 5), FromString("derivative(x + 1, x, 5)"));
        [Fact] public void Test30()
            => Assert.Equal(Integral("x + 1", x), FromString("integral(x + 1, x, 1)"));
        [Fact] public void Test31()
            => Assert.Equal(Integral("x + y", x, 3), FromString("integral(x + y, x, 3)"));
        [Fact] public void Test32()
            => Assert.Equal(Limit("x + y", x, 3), FromString("limit(x + y, x, 3)"));
        [Fact] public void Test33()
            => Assert.Equal(Limit("x + y", x, 3, ApproachFrom.Left), FromString("limitleft(x + y, x, 3)"));
        [Fact] public void Test34()
            => Assert.Equal(Signum("x"), FromString("signum(x)"));
        [Fact] public void Test35()
            => Assert.Equal(Abs("x"), FromString("abs(x)"));
        [Fact] public void Test36()
            => Assert.Equal(-1 * (-1 * (-1 * x)), FromString("---x"));
        [Fact] public void Test37()
            => Assert.Equal(-1 * (-1 * (-1 * x)), FromString("-++-+-+x"));
        [Fact] public void Test38()
            => Assert.Equal(!!!x, FromString("not not not x"));
        [Fact] public void Test39()
            => Assert.Equal(x & x | x.Implies(x), FromString("x and x or (x -> x)"));
        [Fact] public void Test40()
            => Assert.Equal(x & x & x & x, FromString("x and x and x and x"));
        [Fact] public void Test41()
            => Assert.Equal(x | x | x | x, FromString("x or x or x or x"));
        [Fact] public void Test42()
            => Assert.Equal(Abs(x), FromString("(|x|)"));
        [Fact] public void Test43()
            => Assert.Equal(Abs(Abs(x) + 2).Pow(2), FromString("(|(|x|) + 2|) ^ 2"));
        [Fact] public void Test44()
            => Assert.Equal(Sqrt(x).WithCodomain(Domain.Real), FromString("domain(sqrt(x), RR)"));
        [Fact] public void Test45()
            => Assert.Equal(Sqrt(x).WithCodomain(Domain.Real), FromString("domain(domain(sqrt(x), RR), RR)"));
        [Fact] public void Test46()
            => Assert.Equal(Sqrt(x).WithCodomain(Domain.Complex), FromString("domain(domain(sqrt(x), RR), CC)"));
    }
}
