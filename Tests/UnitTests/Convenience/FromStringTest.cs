using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.FromString;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Convenience
{
    public class FromStringTest
    {
        public static readonly Entity.Variable x = MathS.Var(nameof(x));
        public static readonly Entity.Variable y = MathS.Var(nameof(y));

        [Theory]
        [InlineData("limitleft()", "limitleft should have exactly 3 arguments but 0 arguments are provided")]
        [InlineData("derivative(3)", "derivative should have exactly 3 arguments but 1 argument is provided")]
        [InlineData("ln(3, 5)", "ln should have exactly 1 argument but 2 arguments are provided")]
        [InlineData("sin(3, 5, 8)", "sin should have exactly 1 argument but 3 arguments are provided")]
        [InlineData("log()", "log should have 1 argument or 2 arguments but 0 arguments are provided")]
        [InlineData("log(1, 1, 1)", "log should have 1 argument or 2 arguments but 3 arguments are provided")]
        public void WrongNumbersOfArgs0(string input, string message) =>
            Assert.Equal(message, Assert.Throws<FunctionArgumentCountException>(() => (Entity)input).Message);
        [Fact] public void Test1() => Assert.Equal(2, MathS.FromString("1 + 1").Eval());
        [Fact] public void Test2() => Assert.Equal(0, MathS.FromString("sin(0)").Eval());
        [Fact] public void Test3() => Assert.Equal(2, MathS.FromString("log(2, 4)").Eval());
        [Fact] public void Test4() => Assert.Equal(2, MathS.FromString("log(100)").Eval());
        [Fact] public void Test5() => Assert.Equal(MathS.Cos(x), MathS.FromString("sin(x)").Derive(x).Simplify());
        [Fact] public void Test6() => Assert.Equal(7625597484987L, MathS.FromString("3 ^ 3 ^ 3").Eval());
        [Fact] public void Test7() => Assert.Equal(6, MathS.FromString("2 + 2 * 2").Eval());
        [Fact] public void Test8() =>
            // Only needed for Mac
            MathS.Settings.PrecisionErrorZeroRange.As(2e-16m, () =>
                Assert.Equal(MathS.i, MathS.FromString("x^2+1").SolveNt(x).Pieces[0])
            );
        [Fact] public void Test9() => Assert.Equal(1, MathS.FromString("cos(sin(0))").Eval());
        [Fact] public void Test10() => Assert.Equal(ComplexNumber.Create(4, 1), MathS.FromString("2i + 2 * 2 - 1i").Eval());
        [Fact] public void Test11() => Assert.Equal(-1, MathS.FromString("i^2").Eval());
        [Fact] public void Test12() => Assert.Equal(3, MathS.FromString("x^2-1").Substitute(x, 2).Eval());
        [Fact] public void Test13() => Assert.Equal(MathS.DecimalConst.pi / 2, MathS.FromString("arcsin(x)").Substitute(x, 1).Eval());
        [Fact] public void Test14() => Assert.Equal(MathS.Sqr(x), MathS.FromString("x2"));
        [Fact] public void Test15() =>  Assert.Equal(2 * x, MathS.FromString("2x"));
        [Fact] public void Test16() => Assert.Equal(9, MathS.FromString("3 2").Eval());
        [Fact] public void Test17() => Assert.Equal(5 * x, MathS.FromString("x(2 + 3)").Simplify());
        [Fact] public void Test18() => Assert.Equal(x * y, MathS.FromString("x y"));
        [Fact] public void Test19() => Assert.Equal(x * MathS.Sqrt(3), MathS.FromString("x sqrt(3)"));
        [Fact] public void Test20() => Assert.Equal(MathS.Factorial(x), MathS.FromString("x!"));
        [Fact] public void Test21() => Assert.Throws<ParseException>(() => MathS.FromString("x!!"));
        [Fact] public void Test22() => Assert.Equal(MathS.Factorial(MathS.Sin(x)), MathS.FromString("sin(x)!"));
        [Fact] public void Test23() => Assert.Equal(MathS.Pow(2, MathS.Factorial(3)), MathS.FromString("2^3!"));
        [Fact] public void Test24() => Assert.Equal(MathS.Pow(MathS.Factorial(2), MathS.Factorial(3)), MathS.FromString("2!^3!"));
        [Fact] public void Test25() => Assert.Equal(MathS.Pow(MathS.Factorial(2), MathS.Factorial(x + 2)), MathS.FromString("2!^(x+2)!"));
        [Fact] public void Test26() => Assert.Equal(-MathS.Factorial(1), MathS.FromString("-1!"));
        // TODO: "-" inverts an expression instead of parsing it as a number
        [Fact] public void Test27() => Assert.Equal(MathS.Factorial(-1).ToString(), MathS.FromString("(-1)!"));
        [Fact] public void TestSys() => Assert.Equal(MathS.Sqr(x), MathS.FromString("x2"));
        [Fact] public void Test28() 
            => Assert.Equal(MathS.Derivative("x + 1", x), MathS.FromString("derivative(x + 1, x, 1)"));
        [Fact] public void Test29() 
            => Assert.Equal(MathS.Derivative("x + 1", x, 5), MathS.FromString("derivative(x + 1, x, 5)"));
        [Fact] public void Test30()
            => Assert.Equal(MathS.Integral("x + 1", x), MathS.FromString("integral(x + 1, x, 1)"));
        [Fact] public void Test31()
            => Assert.Equal(MathS.Integral("x + y", x, 3), MathS.FromString("integral(x + y, x, 3)"));
        [Fact] public void Test32()
            => Assert.Equal(MathS.Limit("x + y", x, 3), MathS.FromString("limit(x + y, x, 3)"));
        [Fact] public void Test33()
            => Assert.Equal(MathS.Limit("x + y", x, 3, AngouriMath.Limits.ApproachFrom.Left), MathS.FromString("limitleft(x + y, x, 3)"));
    }
}
