using AngouriMath;
using Xunit;

namespace UnitTests.PatternsTest
{
    public class SimplifyTest
    {
        static readonly Entity a = MathS.Var(nameof(a));
        static readonly Entity b = MathS.Var(nameof(b));
        static readonly Entity c = MathS.Var(nameof(c));
        static readonly Entity x = MathS.Var(nameof(x));
        static readonly Entity y = MathS.Var(nameof(y));
        static readonly Entity nan = double.NaN;
        static readonly Entity oo = double.PositiveInfinity;
        static readonly Entity moo = double.NegativeInfinity;
        void AssertSimplify(Entity original, Entity simplified, int? level = null)
        {
            Assert.NotEqual(simplified, original);
            Assert.Equal(simplified, level is { } l ? original.Simplify(l) : original.Simplify());
        }
        void AssertSimplifyIdentical(Entity original, int? level = null) =>
            Assert.Equal(original, level is { } l ? original.Simplify(l) : original.Simplify());
        [Fact] public void TestMinus() => AssertSimplify(x - x, 0);
        [Fact] public void TestMul0() => AssertSimplify(x * 3 * 0, 0);
        [Fact] public void TestMul1() => AssertSimplify(x * 3 * 2, 6 * x);
        [Fact] public void TestPow1() => AssertSimplify(MathS.Pow(3 * x, 1), 3 * x);
        [Fact] public void TestPow0() => AssertSimplify(MathS.Pow(x * 3, 0), 1);
        [Fact] public void TestSum0() => AssertSimplify(x + 0, x);
        [Fact] public void TestPatt1() => AssertSimplify(MathS.Pow(x * 4, 3), MathS.Pow(4 * x, 3));
        [Fact] public void TestPatt2() => AssertSimplify(
            (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))) / (2 * MathS.Sin(x - y) * MathS.Cos(x - y) + 1),
            1 / (MathS.Sin(2 * (x - y)) + 1));
        [Fact] public void TestPatt3() => AssertSimplify((x - y) * (x + y), MathS.Sqr(x) - MathS.Sqr(y));
        [Fact] public void TestPatt4() => AssertSimplify((x - y) * (x + y) / (x * x - y * y), 1);
        [Fact] public void TestPatt5() => AssertSimplify((x + 3) * (3 / (x + 3)), 3);
        [Fact] public void TestPatt6() => AssertSimplify((x + 1) * (x + 2) * (x + 3) / ((x + 2) * (x + 3)), 1 + x);
        [Fact] public void TestPatt7() => AssertSimplify(MathS.Arcsin(x * 3) + MathS.Arccos(x * 3), 0.5 * MathS.pi);
        [Fact] public void TestPatt8() => AssertSimplify(MathS.Arccotan(x * 3) + MathS.Arctan(x * 3), 0.5 * MathS.pi);
        [Fact] public void TestPatt9() => AssertSimplify(MathS.Arccotan(x * 3) + MathS.Arctan(x * 6), MathS.Arccotan(3 * x) + MathS.Arctan(6 * x));
        [Fact] public void TestPatt10() => AssertSimplify(MathS.Arcsin(x * 3) + MathS.Arccos(x * 1), MathS.Arccos(x) + MathS.Arcsin(3 * x));
        [Fact] public void TestPatt11() => AssertSimplify(3 + x + 4 + x, 7 + 2 * x);
        [Fact] public void TestPatt12() => AssertSimplify((x * y * a * b * c) / (c * b * a * x * x), y / x, 4);
        [Fact] public void TestFrac1() => AssertSimplify("x / (y / z)", "x * z / y");
        [Fact] public void TestFrac2() => AssertSimplify("x / y / z", "x / (y * z)");
        [Fact] public void Factorial2() => AssertSimplify(MathS.Factorial(2), 2);
        [Fact] public void Factorial1_5() => AssertSimplify(MathS.Factorial(1.5m), 0.75m * MathS.Sqrt(MathS.pi));
        [Fact] public void Factorial1() => AssertSimplify(MathS.Factorial(1), 1);
        [Fact] public void Factorial0_5() => AssertSimplify(MathS.Factorial(0.5m), 0.5m * MathS.Sqrt(MathS.pi));
        [Fact] public void Factorial0() => AssertSimplify(MathS.Factorial(0), 1);
        [Fact] public void FactorialM0_5() => AssertSimplify(MathS.Factorial(-0.5m), MathS.Sqrt(MathS.pi));
        [Fact] public void FactorialM1() => AssertSimplify(MathS.Factorial(-1), double.NaN);
        [Fact] public void FactorialM1_5() => AssertSimplify(MathS.Factorial(-1.5m), -2 * MathS.Sqrt(MathS.pi));
        [Fact] public void FactorialM2() => AssertSimplify(MathS.Factorial(-2), double.NaN);
        [Fact] public void FactorialXP1OverFactorialXP1() => AssertSimplify(MathS.Factorial(x + 1) / MathS.Factorial(x + 1), 1);
        [Fact] public void FactorialXP1OverFactorialX() => AssertSimplify(MathS.Factorial(1 + x) / MathS.Factorial(x), 1 + x);
        [Fact] public void FactorialXP1OverFactorialXM2() => AssertSimplify(MathS.Factorial(1 + x) / MathS.Factorial(-2 + x), "x3 - x");
        [Fact] public void FactorialXP1OverFactorialXM1() => AssertSimplify(MathS.Factorial(1 + x) / MathS.Factorial(-1 + x), (1 + x) * x);
        [Fact] public void FactorialXP1OverFactorialXM3() => AssertSimplifyIdentical(MathS.Factorial(x + 1) / MathS.Factorial(x - 3));
        [Fact] public void FactorialXOverFactorialXP1() => AssertSimplify(MathS.Factorial(x) / MathS.Factorial(x + 1), 1 / (1 + x));
        [Fact] public void FactorialXOverFactorialX() => AssertSimplify(MathS.Factorial(x) / MathS.Factorial(x), 1);
        [Fact] public void FactorialXOverFactorialXM1() => AssertSimplify(MathS.Factorial(0 + x) / MathS.Factorial(x - 1), x);
        [Fact] public void FactorialXOverFactorialXM2() => AssertSimplify(MathS.Factorial(x + 0) / MathS.Factorial(x - 2), (-1 + x) * x);
        [Fact] public void FactorialXOverFactorialXM3() => AssertSimplifyIdentical(MathS.Factorial(x) / MathS.Factorial(x - 3));
        [Fact] public void FactorialXM1OverFactorialXP1() => AssertSimplify(MathS.Factorial(x - 1) / MathS.Factorial(x + 1), 1 / ((x + 1) * x));
        [Fact] public void FactorialXM1OverFactorialX() => AssertSimplify(MathS.Factorial(-1 + x) / MathS.Factorial(x), 1 / x);
        [Fact] public void FactorialXM1OverFactorialXM1() => AssertSimplify(MathS.Factorial(x - 1) / MathS.Factorial(x - 1), 1);
        [Fact] public void FactorialXM1OverFactorialXM2() => AssertSimplify(MathS.Factorial(-1 + x) / MathS.Factorial(-2 + x), -1 + x);
        [Fact] public void FactorialXM1OverFactorialXM3() => AssertSimplify(MathS.Factorial(x - 1) / MathS.Factorial(-3 + x), (-2 + x) * (-1 + x));
        [Fact] public void FactorialXM0D5OverFactorialXP0D5() => AssertSimplify(MathS.Factorial(x - 0.5m) / MathS.Factorial(x + 0.5m), 1 / (0.5m + x));
        [Fact] public void FactorialXPIOverFactorialXPIM1() => AssertSimplify(MathS.Factorial(x + MathS.i) / MathS.Factorial(x + MathS.i - 1), MathS.i + x);
        [Fact] public void FactorialXP0D5IP0D5OverFactorialXP0D5IM0D5() => AssertSimplify(MathS.Factorial(x + MathS.i * 0.5m + 0.5m) / MathS.Factorial(x + MathS.i * 0.5m - 0.5m), 0.5m + MathS.i * 0.5m + x);
        [Fact] public void XMultiplyFactorialXM1() => AssertSimplify(x * MathS.Factorial(x - 1), MathS.Factorial(x));
        [Fact] public void FactorialXM1MultiplyX() => AssertSimplify(MathS.Factorial(-1 + x) * x, MathS.Factorial(x));
        [Fact] public void XP1MultiplyFactorialX() => AssertSimplify((1 + x) * MathS.Factorial(x), MathS.Factorial(1 + x));
        [Fact] public void FactorialXMultiplyXP1() => AssertSimplify(MathS.Factorial(x) * (x + 1), MathS.Factorial(1 + x));
        [Fact] public void FactorialXP1MultiplyXP2() => AssertSimplify(MathS.Factorial(x + 1) * (x + 2), MathS.Factorial(2 + x));
        [Fact] public void FactorialM22D5PXMultiplyM21D5PX() => AssertSimplify(MathS.Factorial(-22.5m + x) * (-21.5m + x), MathS.Factorial(-21.5m + x));
        [Fact] public void M21D5PXP5IMultiplyFactorial5IPXM22D5() => AssertSimplify((-21.5m + x + 5 * MathS.i) * MathS.Factorial(5 * MathS.i + x - 22.5m), MathS.Factorial(-21.5m + 5 * MathS.i + x));
        [Fact] public void OOP1() => AssertSimplify(oo + 1, oo); 
        [Fact] public void OO0() => AssertSimplify(oo * 0, nan);
        [Fact] public void OOPowOO() => AssertSimplify(MathS.Pow(oo, oo), oo);
        [Fact] public void MOO() => AssertSimplify(-oo, moo);
        [Fact] public void MOOP1() => AssertSimplify(-oo + 1, moo);
        [Fact] public void MOO0() => AssertSimplify(-oo * 0, nan);
        [Fact] public void MOOPowMOO() => AssertSimplify(MathS.Pow(-oo, -oo), nan);
        [Fact] public void NaNP1() => AssertSimplify(nan + 1, nan);
        [Fact] public void NaN0() => AssertSimplify(nan * 0, nan);
        [Fact] public void NaNPow0() => AssertSimplify(MathS.Pow(nan, 0), nan);
        [Fact] public void Derive1() => AssertSimplify(MathS.Derivative("x + 2", x), 1);
        [Fact] public void Derive2() => AssertSimplify(MathS.Derivative("7x2 - x + 2", x, 2), 14);
        [Fact] public void Integral1() => AssertSimplify(MathS.Integral("x + y", x, 0), "x + y");
        [Fact] public void Divide1() => AssertSimplify("(x2 + 2 x y + y2) / (x + y)", "x + y");
        // TODO: Smart collapser
        [Fact] public void Divide2() => AssertSimplify("(x3 + 3 x 2 y + 3 x y 2 + y3) / (x + y)", "2 * (x * y) + (x ^ 2 + y ^ 2)");
        [Fact] public void Divide3() => AssertSimplify("(x2 + 2 x y + y2 + 1) / (x + y)", "1 / (x + y) + (x + y)");

        [Fact] public void BigSimple1() => AssertSimplify(
            "1+2x*-1+2x*2+x^2+2x+2x*-4+2x*4+2x*2x*-1+2x*2x*2+2x*x^2+x^2+x^2*-4+x^2*4+x^2*2*x*-1+x^2*2x*2+x^2*x^2",
            "1 + (4 * x + (4 * x ^ 3 + 6 * x ^ 2 + x ^ 4))");
        // NOTE: Simplify should not be called both sides since does not ensure that the simplified result
        // is acceptable - we should maintain an expected result that does not change with the implementation
        // and update it when needed. Test should be more restrictive to actually catch bugs.
        // When both sides have the same string result - check object structure and add parentheses.
        // Using DirectChildren inside the Immediate Window can help.
    }
}

