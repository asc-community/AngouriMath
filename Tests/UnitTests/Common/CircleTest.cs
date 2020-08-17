using AngouriMath;
using Xunit;
using System.Linq;

namespace UnitTests.Common
{
    public class CircleTest
    {
        public static readonly Entity.Variable x = MathS.Var(nameof(x));

        [Theory]
        [InlineData("1 + 2 * log(2, 3)")]
        [InlineData("2 ^ 3 + sin(3)")]
        [InlineData("x!")]
        [InlineData("(-1)!")]
        [InlineData("-1!")]
        [InlineData("(3i)!")]
        public void Circle(string inputIsOutput) =>
            Assert.Equal(inputIsOutput, MathS.FromString(inputIsOutput).ToString());

        [Fact]
        public void Test3()
        {
            const string expr = "23.3 + 3 / 3 + i";
            var exprActual = MathS.FromString(expr);
            Assert.Equal("233/10 + 3 / 3 + i", exprActual.ToString());
        }

        [Fact]
        public void Test4()
        {
            MathS.FromString((MathS.Sin(x) / MathS.Cos(x)).Derive(x).ToString());
        }

        [Fact]
        public void Test5()
        {
            Assert.Equal("1", MathS.Numbers.Create(1).ToString());
            Assert.Equal("-1", MathS.Numbers.Create(-1).ToString());
        }

        [Fact]
        public void Test6()
        {
            Assert.Equal("i", MathS.i.ToString());
            Assert.Equal("-i", (-1 * MathS.i).ToString());
        }

        [Fact]
        public void Test7() => Assert.Equal(3 * x, MathS.Sin(MathS.Arcsin(x * 3)).Simplify());

        [Fact]
        public void Test8() => Assert.Equal(3 * x, MathS.Arccotan(MathS.Cotan(x * 3)).Simplify());
        public bool FunctionsAreEqualHack(Entity eq1, Entity eq2)
        {
            var vars1 = eq1.Vars;
            var vars2 = eq2.Vars;
            if (!vars1.SequenceEqual(vars2))
                return false;
            for (int i = 1; i < 10; i++)
            {
                var replacements = vars1.ToDictionary(var => (Entity)var, _ => (Entity)i);
                var a = eq1.Substitute(replacements);
                var b = eq2.Substitute(replacements);

                if (a.Eval() != b.Eval())
                    return false;
            }

            return true;
        }

        [Fact]
        public void TestLinch()
        {
            Entity expr = "x / y + x * x * y";
            Entity exprOptimized = MathS.Utils.OptimizeTree(expr);
            Assert.True(FunctionsAreEqualHack(expr, exprOptimized), "Expressions " + expr.ToString() + " and " + exprOptimized.ToString() + " are not equal");
        }

        [Fact]
        public void TestLinch1()
        {
            Entity expr = "x / 1 + 2";
            Entity exprOptimized = MathS.Utils.OptimizeTree(expr);
            Assert.True(FunctionsAreEqualHack(expr, exprOptimized), "Expressions " + expr.ToString() + " and " + exprOptimized.ToString() + " are not equal");
        }

        [Fact]
        public void TestLinch2()
        {
            Entity expr = "(x + y + x + 1 / (x + 4 + 4 + sin(x))) / (x + x + 3 / y) + 3";
            Entity exprOptimized = MathS.Utils.OptimizeTree(expr);
            Assert.True(FunctionsAreEqualHack(expr, exprOptimized), "Expressions " + expr.ToString() + " and " + exprOptimized.ToString() + " are not equal");
        }
    }
}