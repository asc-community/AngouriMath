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
        static readonly NumberEntity nan = double.NaN;
        static readonly NumberEntity oo = double.PositiveInfinity;
        static readonly NumberEntity moo = double.NegativeInfinity;
        void AssertSimplify(Entity original, Entity simplified, int? level = null)
        {
            Assert.AreNotEqual(simplified, original);
            Assert.AreEqual(simplified, level is { } l ? original.Simplify(l) : original.Simplify());
        }
        void AssertSimplifyIdentical(Entity original, int? level = null) =>
            Assert.AreEqual(original, level is { } l ? original.Simplify(l) : original.Simplify());
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
        [TestMethod] public void Factorial2() => AssertSimplify(MathS.Factorial(2), 2);
        [TestMethod] public void Factorial1_5() => AssertSimplify(MathS.Factorial(1.5m), 0.75m * MathS.Sqrt(MathS.pi));
        [TestMethod] public void Factorial1() => AssertSimplify(MathS.Factorial(1), 1);
        [TestMethod] public void Factorial0_5() => AssertSimplify(MathS.Factorial(0.5m), 0.5m * MathS.Sqrt(MathS.pi));
        [TestMethod] public void Factorial0() => AssertSimplify(MathS.Factorial(0), 1);
        [TestMethod] public void FactorialM0_5() => AssertSimplify(MathS.Factorial(-0.5m), MathS.Sqrt(MathS.pi));
        [TestMethod] public void FactorialM1() => AssertSimplify(MathS.Factorial(-1), double.NaN);
        [TestMethod] public void FactorialM1_5() => AssertSimplify(MathS.Factorial(-1.5m), -2 * MathS.Sqrt(MathS.pi));
        [TestMethod] public void FactorialM2() => AssertSimplify(MathS.Factorial(-2), double.NaN);
        [TestMethod] public void FactorialXP1OverFactorialXP1() => AssertSimplify(MathS.Factorial(x + 1) / MathS.Factorial(x + 1), 1);
        [TestMethod] public void FactorialXP1OverFactorialX() => AssertSimplify(MathS.Factorial(1 + x) / MathS.Factorial(x), 1 + x);
        [TestMethod] public void FactorialXP1OverFactorialXM1() => AssertSimplify(MathS.Factorial(1 + x) / MathS.Factorial(-1 + x), x * (1 + x));
        [TestMethod] public void FactorialXP1OverFactorialXM2() => AssertSimplifyIdentical(MathS.Factorial(1 + x) / MathS.Factorial(-2 + x));
        [TestMethod] public void FactorialXP1OverFactorialXM3() => AssertSimplifyIdentical(MathS.Factorial(x + 1) / MathS.Factorial(x - 3));
        [TestMethod] public void FactorialXOverFactorialXP1() => AssertSimplify(MathS.Factorial(x) / MathS.Factorial(x + 1), 1 / (1 + x));
        [TestMethod] public void FactorialXOverFactorialX() => AssertSimplify(MathS.Factorial(x) / MathS.Factorial(x), 1);
        [TestMethod] public void FactorialXOverFactorialXM1() => AssertSimplify(MathS.Factorial(0 + x) / MathS.Factorial(x - 1), x);
        [TestMethod] public void FactorialXOverFactorialXM2() => AssertSimplify(MathS.Factorial(x + 0) / MathS.Factorial(x - 2), (-1 + x) * x);
        [TestMethod] public void FactorialXOverFactorialXM3() => AssertSimplifyIdentical(MathS.Factorial(x) / MathS.Factorial(x - 3));
        [TestMethod] public void FactorialXM1OverFactorialXP1() => AssertSimplify(MathS.Factorial(x - 1) / MathS.Factorial(x + 1), 1 / ((x + 1) * x));
        [TestMethod] public void FactorialXM1OverFactorialX() => AssertSimplify(MathS.Factorial(-1 + x) / MathS.Factorial(x), 1 / x);
        [TestMethod] public void FactorialXM1OverFactorialXM1() => AssertSimplify(MathS.Factorial(x - 1) / MathS.Factorial(x - 1), 1);
        [TestMethod] public void FactorialXM1OverFactorialXM2() => AssertSimplify(MathS.Factorial(-1 + x) / MathS.Factorial(-2 + x), -1 + x);
        [TestMethod] public void FactorialXM1OverFactorialXM3() => AssertSimplify(MathS.Factorial(x - 1) / MathS.Factorial(-3 + x), (-2 + x) * (-1 + x));
        [TestMethod] public void XMultiplyFactorialXM1() => AssertSimplify(x * MathS.Factorial(x - 1), MathS.Factorial(x));
        [TestMethod] public void FactorialXM1MultiplyX() => AssertSimplify(MathS.Factorial(-1 + x) * x, MathS.Factorial(x));
        [TestMethod] public void XP1MultiplyFactorialX() => AssertSimplify((1 + x) * MathS.Factorial(x), MathS.Factorial(1 + x));
        [TestMethod] public void FactorialXMultiplyXP1() => AssertSimplify(MathS.Factorial(x) * (x + 1), MathS.Factorial(1 + x));
        [TestMethod] public void FactorialXP1MultiplyXP2() => AssertSimplify(MathS.Factorial(x + 1) * (x + 2), MathS.Factorial(1 + x));
        [TestMethod] public void OOP1() => AssertSimplify(oo + 1, oo);
        [TestMethod] public void OO0() => AssertSimplify(oo * 0, nan);
        [TestMethod] public void OOPowOO() => AssertSimplify(MathS.Pow(oo, oo), oo);
        [TestMethod] public void MOO() => AssertSimplify(-oo, moo);
        [TestMethod] public void MOOP1() => AssertSimplify(-oo + 1, moo);
        [TestMethod] public void MOO0() => AssertSimplify(-oo * 0, nan);
        [TestMethod] public void MOOPowMOO() => AssertSimplify(MathS.Pow(-oo, -oo), nan);
        [TestMethod] public void NaNP1() => AssertSimplify(nan + 1, nan);
        [TestMethod] public void NaN0() => AssertSimplify(nan * 0, nan);
        [TestMethod] public void NaNPow0() => AssertSimplify(MathS.Pow(nan, 0), nan);
    }
}
