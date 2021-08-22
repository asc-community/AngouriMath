using AngouriMath;
using AngouriMath.Extensions;
using FluentAssertions;
using HonkSharp.Fluency;
using System.Linq;
using Xunit;
using static AngouriMath.Entity;

namespace UnitTests.Common
{
    public class LambdaCalculusTest
    {
        private readonly Variable x = "x";
        private readonly Variable y = "y";
        private readonly Variable z = "z";
        private readonly Variable f = "f";
        [Fact] public void ApplicationDoesNothingToVariable()
            => x.Apply(4).Alias(out var expected).InnerSimplified.Should().Be(expected);

        [Fact] public void ApplicationDoesNothingToNumber()
            => 5.ToNumber().Apply(4).Alias(out var expected).InnerSimplified.Should().Be(expected);

        [Fact] public void LambdaIdentity()
            => x.LambdaOver(x).Apply(67).InnerSimplified.ShouldBe(67);

        [Fact] public void LambdaTest1()
            => (2 * x).LambdaOver(x).Apply(4).InnerSimplified.ShouldBe(8);

        [Fact] public void LambdaTest2()
            => (2 * x * y).LambdaOver(x).LambdaOver(y).Apply(4).Apply(3).InnerSimplified.ShouldBe(24);

        [Fact] public void LambdaTest3()
            => (2 * x * y).LambdaOver(x).LambdaOver(y).Apply(4).InnerSimplified.ShouldBe((2 * x * 4).LambdaOver(x));

        [Fact] public void LambdaTest4()
            => x.Pow(y).LambdaOver(x).LambdaOver(y).Apply(3).Apply(2).InnerSimplified.ShouldBe(8);

        [Fact] public void LambdaTest5()
            => f.Apply(MathS.pi / 3).LambdaOver(f).Apply("sin").InnerSimplified.ShouldBe("0.5 * (sqrt(3))");

        [Fact] public void LambdaTest6()
            => f.Apply(x.Pow(2)).Apply(x).LambdaOver(f).Apply("derivative").InnerSimplified.ShouldBe(2 * x);

        [Fact] public void LambdaTest7()
            => f.Apply(x).Apply(y).InnerSimplified.ShouldBe(f.Apply(x, y));

        [Fact] public void LambdaTest8()
            => (z * x).LambdaOver(x)
                .Substitute(z, x)
                .InnerSimplified
                .ShouldBe((x * "y").LambdaOver("y"));

        [Fact] public void LambdaTest9()
            => (z * x).LambdaOver(x)
                .LambdaOver(z)
                .Apply(x)
                .InnerSimplified
                .ShouldBe((x * "y").LambdaOver("y"));

        [Fact]
        public void LambdaTest10()
            => (x * z * f)
                .LambdaOver(x)
                .LambdaOver(z)
                .LambdaOver(f)
                .Apply(z)
                .InnerSimplified
                .ShouldBe((x * "y" * z).LambdaOver(x).LambdaOver("y"));

        [Fact] public void LambdaTest11()
            => (x * z * f)
                .LambdaOver(x)
                .LambdaOver(z)
                .LambdaOver(f)
                .Apply(z)
                .Apply(x)
                .InnerSimplified
                .ShouldBe(("a" * x * z).LambdaOver("a"));


        [Fact] public void LambdaTest12()
            => (x * y * z)
                .LambdaOver(x)
                .LambdaOver(y)
                .Apply(x)
                .InnerSimplified
                .ShouldBe(("a" * x * z).LambdaOver("a"));

        [Fact] public void LambdaTest13()
            => x.LambdaOver(x)
                .Apply(3)
                .LambdaOver(y)
                .InnerSimplified
                .ShouldBe(3.ToNumber().LambdaOver(y));

        [Fact] public void LambdaTest14()
            => x.LambdaOver(x)
                .Apply(y.LambdaOver(y))
                .InnerSimplified
                .ShouldBe(y.LambdaOver(y));

        [Fact] public void LambdaTest15()
            => x.Apply(y.LambdaOver(y).Apply(3))
                .InnerSimplified
                .ShouldBe(x.Apply(3));
    }
}
