using AngouriMath;
using AngouriMath.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SimplifyTest
    {
        static readonly VariableEntity a = MathS.Var(nameof(a));
        static readonly VariableEntity b = MathS.Var(nameof(b));
        static readonly VariableEntity c = MathS.Var(nameof(c));
        static readonly VariableEntity x = MathS.Var(nameof(x));
        static readonly VariableEntity y = MathS.Var(nameof(y));
        void AssertSimplify(Entity original, Entity simplified, int? level = null)
        {
            Assert.AreNotEqual(simplified, original);
            Assert.AreEqual(simplified, level is { } l ? simplified.Simplify(l) : simplified.Simplify());
        }
        [TestMethod] public void TestMinus() => AssertSimplify(x - x, 0);
        [TestMethod] public void TestMul0() => AssertSimplify(x * 3 * 0, 0);
        [TestMethod] public void TestMul1() => AssertSimplify(x * 3 * 2, 6 * x);
        [TestMethod] public void TestPow1() => AssertSimplify(MathS.Pow(3 * x, 1), 3 * x);
        [TestMethod] public void TestPow0() => AssertSimplify(MathS.Pow(x * 3, 0), 1);
        [TestMethod] public void TestSum0() => AssertSimplify(x + 0, x);
        [TestMethod] public void TestPatt1() => AssertSimplify(MathS.Pow(x * 4, 3), MathS.Pow(4 * x, 3));
        [TestMethod] public void TestPatt2() => AssertSimplify(
            (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))) / (2 * MathS.Sin(x - y) * MathS.Cos(x - y) + 1),
            1 / (MathS.Sin(2 * (x - y)) + 1));
        [TestMethod] public void TestPatt3() => AssertSimplify((x - y) * (x + y), MathS.Sqr(x) - MathS.Sqr(y));
        [TestMethod] public void TestPatt4() => AssertSimplify((x - y) * (x + y) / (x * x - y * y), 1);
        [TestMethod] public void TestPatt5() => AssertSimplify((x + 3) * (3 / (x + 3)), 3);
        [TestMethod] public void TestPatt6() => AssertSimplify((x + 1) * (x + 2) * (x + 3) / ((x + 2) * (x + 3)), 1 + x);
        [TestMethod] public void TestPatt7() => AssertSimplify(MathS.Arcsin(x * 3) + MathS.Arccos(x * 3), 0.5 * MathS.pi);
        [TestMethod] public void TestPatt8() => AssertSimplify(MathS.Arccotan(x * 3) + MathS.Arctan(x * 3), 0.5 * MathS.pi);
        [TestMethod] public void TestPatt9() => AssertSimplify(MathS.Arccotan(x * 3) + MathS.Arctan(x * 6), MathS.Arccotan(3 * x) + MathS.Arctan(6 * x));
        [TestMethod] public void TestPatt10() => AssertSimplify(MathS.Arcsin(x * 3) + MathS.Arccos(x * 1), MathS.Arccos(x) + MathS.Arcsin(3 * x));
        [TestMethod] public void TestPatt11() => AssertSimplify(3 + x + 4 + x, 7 + 2 * x);
        [TestMethod] public void TestPatt12() => AssertSimplify((x * y * a * b * c) / (c * b * a * x * x), y / x, 4);
        [TestMethod] public void TestFrac1() => AssertSimplify("x / (y / z)", "x * z / y");
        [TestMethod] public void TestFrac2() => AssertSimplify("x / y / z", "x / (y * z)");
    }
}
