//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    public class BuiltinFunctionsAppliedTest
    {
        public static readonly Entity.Variable x = MathS.Var(nameof(x));
        public static readonly Entity.Variable y = MathS.Var(nameof(y));

        [Theory, CombinatorialData]
        public void TestOneArg(
            [CombinatorialValues(
                "Sin:sin",
                "Cos:cos",
                "Tan:tan",
                "Cotan:cot",
                "Cotan:cotan",
                "Sec:sec",
                "Cosec:cosec",
                "Cosec:csc",
                "Arcsin:arcsin",
                "Arcsin:asin",
                "Arccos:arccos",
                "Arccos:acos",
                "Arctan:arctan",
                "Arctan:atan",
                "Arccotan:arccotan",
                "Arccotan:arccot",
                "Arccotan:acot",
                "Arccotan:acotan",
                "Arcsec:arcsec",
                "Arcsec:asec",
                "Arccosec:arccosec",
                "Arccosec:arccsc",
                "Arccosec:acsc",
                "Signum:signum",
                "Signum:sgn",
                "Signum:sign",
                "Abs:abs",
                "PhiFunction:phi"
            )]
            string combined)
        {
            var (entityMethodName, nameToTest) = combined.Split(':').Into2();
            var entityMethod = typeof(Entity).GetMethod(entityMethodName) ?? throw new System.Exception("No method was found");
            var origin = nameToTest.ToEntity().Apply(x);
            var actual = origin.InnerSimplified;
            var expected = entityMethod.Invoke(x, new object?[0]);
            Assert.Equal(expected, actual);
        }

        private void TestOneArgIntoExpression(string name, string entityMethodName, System.Type type)
        {
            var mi = type.GetMethod(entityMethodName) ?? throw new System.Exception("No method was found");

            var actual = name.ToEntity().Apply(x).InnerSimplified;
            var expected = mi.Invoke(null, new[] { x });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("sinh", "Sinh")]
        [InlineData("sh", "Sinh")]

        [InlineData("cosh", "Cosh")]
        [InlineData("ch", "Cosh")]

        [InlineData("tanh", "Tanh")]
        [InlineData("th", "Tanh")]

        [InlineData("cotanh", "Cotanh")]
        [InlineData("coth", "Cotanh")]
        [InlineData("cth", "Cotanh")]

        [InlineData("sech", "Sech")]
        [InlineData("sch", "Sech")]

        [InlineData("cosech", "Cosech")]
        [InlineData("csch", "Cosech")]

        [InlineData("arsinh", "Arsinh")]
        [InlineData("asinh", "Arsinh")]
        [InlineData("arsh", "Arsinh")]

        [InlineData("arcosh", "Arcosh")]
        [InlineData("acosh", "Arcosh")]
        [InlineData("arch", "Arcosh")]

        [InlineData("artanh", "Artanh")]
        [InlineData("atanh", "Artanh")]
        [InlineData("arth", "Artanh")]

        [InlineData("arcotanh", "Arcotanh")]
        [InlineData("acotanh", "Arcotanh")]
        [InlineData("arcoth", "Arcotanh")]
        [InlineData("acoth", "Arcotanh")]
        [InlineData("arcth", "Arcotanh")]

        [InlineData("arsech", "Arsech")]
        [InlineData("arsch", "Arsech")]
        [InlineData("asech", "Arsech")]

        [InlineData("arcosech", "Arcosech")]
        [InlineData("acosech", "Arcosech")]
        [InlineData("arcsch", "Arcosech")]
        [InlineData("acsch", "Arcosech")]
        public void TestHyperbolicOneArgIntoExpression(string name, string entityMethodName)
            => TestOneArgIntoExpression(name, entityMethodName, typeof(MathS.Hyperbolic));

        [Theory]
        [InlineData("gamma", "Gamma")]
        [InlineData("ln", "Ln")]
        [InlineData("sqrt", "Sqrt")]
        [InlineData("cbrt", "Cbrt")]
        [InlineData("sqr", "Sqr")]
        public void TestMathSOneArgIntoExpression(string name, string entityMethodName)
            => TestOneArgIntoExpression(name, entityMethodName, typeof(MathS));

        [Fact] public void TestLogFull() => "log".ToEntity().Apply(2).Apply(8).InnerSimplified.ShouldBe(3);
        [Fact] public void TestLogCurried() => "log".ToEntity().Apply(2).InnerSimplified.ShouldBe("log".ToEntity().Apply(2));

        [Fact] public void TestDerivativeFull() => "derivative".ToEntity().Apply("x ^ 2").Apply("x").InnerSimplified.ShouldBe("2 * x");
        [Fact] public void TestDerivativeCurried() => "derivative".ToEntity().Apply("x ^ 2").InnerSimplified.ShouldBe("derivative".ToEntity().Apply("x ^ 2"));
        [Fact] public void TestIntegralFull() => "integral".ToEntity().Apply("1").Apply("x").InnerSimplified.ShouldBe("x");
        [Fact] public void TestIntegralCurried() => "integral".ToEntity().Apply("x ^ 2").InnerSimplified.ShouldBe("integral".ToEntity().Apply("x ^ 2"));
        [Fact] public void TestLimitFull() => "limit".ToEntity().Apply("sin(x)").Apply("x").Apply("0").InnerSimplified.ShouldBe(0);
        [Fact] public void TestLimitCurried1() => "limit".ToEntity().Apply("x").Apply("y").InnerSimplified.ShouldBe("limit".ToEntity().Apply("x", "y"));
        [Fact] public void TestLimitCurried2() => "limit".ToEntity().Apply("x").InnerSimplified.ShouldBe("limit".ToEntity().Apply("x"));
        [Fact] public void TestLimitLeftFull() => "limitleft".ToEntity().Apply("sin(x)").Apply("x").Apply("0").InnerSimplified.ShouldBe(0);
        [Fact] public void TestLimitLeftCurried1() => "limitleft".ToEntity().Apply("sin(x)").Apply("x").InnerSimplified.ShouldBe("limitleft".ToEntity().Apply("sin(x)", "x"));
        [Fact] public void TestLimitLeftCurried2() => "limitleft".ToEntity().Apply("sin(x)").InnerSimplified.ShouldBe("limitleft".ToEntity().Apply("sin(x)"));
        [Fact] public void TestLimitRightFull() => "limitright".ToEntity().Apply("sin(x)").Apply("x").Apply("0").InnerSimplified.ShouldBe(0);
        [Fact] public void TestLimitRightCurried1() => "limitright".ToEntity().Apply("sin(x)").Apply("x").InnerSimplified.ShouldBe("limitright".ToEntity().Apply("sin(x)", "x"));
        [Fact] public void TestLimitRightCurried2() => "limitright".ToEntity().Apply("sin(x)").InnerSimplified.ShouldBe("limitright".ToEntity().Apply("sin(x)"));
    }
}
