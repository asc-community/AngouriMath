using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class LatexTests
    {
        private static readonly VariableEntity x = MathS.Var("x");
        private static readonly NumberEntity frac34 = Number.CreateRational(3, 4);
        private static readonly NumberEntity m1 = -1;
        private static readonly NumberEntity im1 = MathS.i - 1;
        private static readonly NumberEntity m2i = -2 * MathS.i;
        void Test(string expected, AngouriMath.Core.Sys.Interfaces.ILatexiseable actual) =>
            Assert.AreEqual(expected, actual.Latexise());
        void TestSimplify(string expected, Entity actual) =>
            Test(expected, actual.Simplify());
        [TestMethod] public void Num() => TestSimplify("123", 123);
        [TestMethod] public void Float() => Test("123.456", (Entity)123.456);
        [TestMethod] public void FloatSimplify() => TestSimplify(@"\frac{15432}{125}", 123.456);
        [TestMethod] public void FracParse() => Test(@"3", ComplexNumber.Parse("3.000"));
        [TestMethod] public void FloatZero() => Test(@"123.4561234567890", ComplexNumber.Parse("123.4561234567890"));
        [TestMethod] public void Pi() => Test(@"\pi", MathS.pi);
        [TestMethod] public void E() => Test(@"e", MathS.e);
        [TestMethod] public void I() => TestSimplify("i", MathS.Sqrt(-1));
        [TestMethod] public void Var() => Test("x", x);
        [TestMethod] public void Add() => Test("x+x", x + x);
        [TestMethod] public void AddAdd() => Test("x+x+x", x + x + x);
        [TestMethod] public void AddSimplify() => TestSimplify(@"2\times x", x + x);
        [TestMethod] public void Subtract() => Test("x-x", x - x);
        [TestMethod] public void SubtractSubtract() => Test("x-x-x", x - x - x);
        [TestMethod] public void SubtractSimplify() => TestSimplify("0", x - x);
        [TestMethod] public void Multiply() => Test(@"x\times x", x * x);
        [TestMethod] public void MultiplyMultiply() => Test(@"x\times x\times x", x * x * x);
        [TestMethod] public void MultiplySimplify() => TestSimplify("{x}^{2}", x * x);
        [TestMethod] public void Divide() => Test(@"\frac{x}{x}", x / x);
        [TestMethod] public void DivideDivide() => Test(@"\frac{\frac{x}{x}}{x}", x / x / x);
        [TestMethod] public void DivideSimplify() => TestSimplify(@"1", x / x);
        [TestMethod] public void Greek() => Test(@"\alpha", MathS.Var("alpha"));
        [TestMethod] public void SubscriptSingle() => Test(@"c_{e}", MathS.Var("c_e"));
        [TestMethod] public void SubscriptDuo() => Test(@"\Delta_{a2}", MathS.Var("Delta_a2"));
        [TestMethod] public void SubscriptDuoPlus() => TestSimplify(@"6+\Delta_{a2}", "Delta_a2+6");
        [TestMethod] public void Square() => Test(@"{x}^{2}", MathS.Sqr(x));
        [TestMethod] public void SquareSquare() => Test(@"{\left({x}^{2}\right)}^{2}", MathS.Sqr(MathS.Sqr(x)));
        [TestMethod] public void SquareRoot() => Test(@"\sqrt{x}", MathS.Sqrt(x));
        [TestMethod] public void SquareRootAsPow() => Test(@"\sqrt{x}", MathS.Pow(x, 1m/2));
        [TestMethod] public void Cube() => Test(@"{x}^{3}", MathS.Pow(x, 3));
        [TestMethod] public void CubeRoot() => Test(@"\sqrt[3]{x}", MathS.Pow(x, 1m/3));
        [TestMethod] public void FourthRoot() => Test(@"\sqrt[4]{x}", MathS.Pow(x, 1m/4));
        [TestMethod] public void FourthRootCube() => Test(@"\sqrt[4]{x}^{3}", MathS.Pow(x, 3m/4));
        [TestMethod] public void M3Root() => Test(@"\frac{1}{\sqrt[3]{x}}", MathS.Pow(x, 1m/-3));
        [TestMethod] public void M4RootCube() => Test(@"\frac{1}{\sqrt[4]{x}^{3}}", MathS.Pow(x, 3m/-4));
        [TestMethod] public void PowBase2() => Test("{2}^{x}", MathS.Pow(2, x));
        [TestMethod] public void PowBase10() => Test("{10}^{x}", MathS.Pow(10, x));
        [TestMethod] public void Pow() => Test("{x}^{x}", MathS.Pow(x, x));
        [TestMethod] public void PowPow() => Test("{x}^{{x}^{x}}", MathS.Pow(x, MathS.Pow(x, x)));
        [TestMethod] public void PowPowNumber() => Test(@"{\left({12}^{23}\right)}^{34}", MathS.Pow(MathS.Pow(12, 23), 34));
        [TestMethod] public void MPow() => Test(@"{\left(-x\right)}^{x}", MathS.Pow(-x, x));
        [TestMethod] public void AddMultiply() =>
            Test(@"\left(1+2\right)\times \left(3+4\right)", ((Entity)1 + 2) * ((Entity)3 + 4));
        [TestMethod] public void SubtractMultiply() =>
            Test(@"\left(1-2\right)\times \left(3-4\right)", ((Entity)1 - 2) * ((Entity)3 - 4));
        [TestMethod] public void AddPow() => Test(@"{\left(3+4\right)}^{x}", MathS.Pow("3+4", x));
        [TestMethod] public void SubtractPow() => Test(@"{\left(3-4\right)}^{x}", MathS.Pow("3-4", x));
        [TestMethod] public void MultiplyPow() => Test(@"{\left(3\times 4\right)}^{x}", MathS.Pow("3*4", x));
        [TestMethod] public void DividePow() => Test(@"{\left(\frac{3}{4}\right)}^{x}", MathS.Pow("3/4", x));
        [TestMethod] public void XSquaredMinusX() => TestSimplify(@"{x}^{2}-x", "x^2-x");
        [TestMethod] public void XSquaredMinusXAlternate() => TestSimplify(@"{x}^{2}-x", "-x+x^2");
        [TestMethod] public void XSquaredMinusXAlternate2() => TestSimplify(@"{x}^{2}-x", "x^2+(-1)*x");
        [TestMethod] public void M1() => Test("-1", (Number)(-1));
        [TestMethod] public void M1Entity() => Test("-1", m1);
        [TestMethod] public void M1Add() => Test(@"-1-1", m1 + m1);
        [TestMethod] public void M1Subtract() => Test(@"-1--1", m1 - m1);
        [TestMethod] public void M1Multiply() => Test(@"--1", m1 * m1);
        [TestMethod] public void M1Divide() => Test(@"\frac{-1}{-1}", m1 / m1);
        [TestMethod] public void PowM1() => Test(@"{x}^{-1}", MathS.Pow(x, m1));
#warning Needs fixing! Should be {\left(-1\right)}^{x}
        [TestMethod] public void M1Pow() => Test(@"{-1}^{x}", MathS.Pow(m1, x));
        [TestMethod] public void M2Pow() => Test(@"{-2}^{x}", MathS.Pow(-2, x));
        [TestMethod] public void MI() => TestSimplify("-i", MathS.Sqrt(-1) * -1);
        [TestMethod] public void M2I() => TestSimplify("-2i", m2i);
        [TestMethod] public void IM1() => Test("-1 + i", im1);
        [TestMethod] public void MIM1() => Test("-1 - i", -MathS.i - 1);
        [TestMethod] public void M2IM1() => Test("-1 - 2i", -2 * MathS.i - 1);
        [TestMethod] public void IP1() => Test("1 + i", MathS.i + 1);
        [TestMethod] public void MIP1() => Test("1 - i", -1 * MathS.i + 1);
        [TestMethod] public void M2IP1() => Test("1 - 2i", -2 * MathS.i + 1);
        [TestMethod] public void M0IP1() => Test("1", 0 * -MathS.i + 1);
        [TestMethod] public void M0IM1() => Test("-1", 0 * -MathS.i - 1);
        [TestMethod] public void ISquare() => Test("{i}^{2}", MathS.Sqr(MathS.i));
        [TestMethod] public void ISquareSimplified() => TestSimplify("-1", MathS.Sqr(MathS.i));
        [TestMethod] public void Frac34Add() => Test(@"\frac{3}{4}+\frac{3}{4}", frac34 + frac34);
        [TestMethod] public void Frac34Subtract() => Test(@"\frac{3}{4}-\left(\frac{3}{4}\right)", frac34 - frac34);
        [TestMethod] public void Frac34Multiply() => Test(@"\left(\frac{3}{4}\right)\times \left(\frac{3}{4}\right)", frac34 * frac34);
        [TestMethod] public void Frac34Divide() => Test(@"\frac{\frac{3}{4}}{\frac{3}{4}}", frac34 / frac34);
        [TestMethod] public void Frac34Square() => Test(@"{\left(\frac{3}{4}\right)}^{2}", MathS.Sqr(frac34));
        [TestMethod] public void Frac34SquareRoot() => Test(@"\sqrt{\frac{3}{4}}", MathS.Sqrt(frac34));
        [TestMethod] public void Frac34CubeRoot() => Test(@"\sqrt[3]{\frac{3}{4}}", MathS.Pow(frac34, 1m/3));
        [TestMethod] public void Frac34Pow() => Test(@"{\left(\frac{3}{4}\right)}^{x}", MathS.Pow(frac34, x));
        [TestMethod] public void PowFrac34() => Test(@"\sqrt[4]{2}^{3}", MathS.Pow(2, frac34));
        [TestMethod] public void M2IAdd() => Test(@"-2i-2i", m2i + m2i);
        [TestMethod] public void M2ISubtract() => Test(@"-2i-\left(-2i\right)", m2i - m2i);
        [TestMethod] public void M2IMultiply() => Test(@"\left(-2i\right)\times \left(-2i\right)", m2i * m2i);
        [TestMethod] public void M2IDivide() => Test(@"\frac{-2i}{-2i}", m2i / m2i);
        [TestMethod] public void M2ISquare() => Test(@"{\left(-2i\right)}^{2}", MathS.Sqr(m2i));
        [TestMethod] public void M2ISquareRoot() => Test(@"\sqrt{-2i}", MathS.Sqrt(m2i));
        [TestMethod] public void M2ICubeRoot() => Test(@"\sqrt[3]{-2i}", MathS.Pow(m2i, 1m/3));
        [TestMethod] public void M2IPow() => Test(@"{\left(-2i\right)}^{x}", MathS.Pow(m2i, x));
        [TestMethod] public void PowM2I() => Test(@"{2}^{-2i}", MathS.Pow(2, m2i));
        [TestMethod] public void IM1Add() => Test(@"-1 + i-1 + i", im1 + im1);
        [TestMethod] public void IM1Subtract() => Test(@"-1 + i-\left(-1 + i\right)", im1 - im1);
        [TestMethod] public void IM1Multiply() => Test(@"\left(-1 + i\right)\times \left(-1 + i\right)", im1 * im1);
        [TestMethod] public void IM1Divide() => Test(@"\frac{-1 + i}{-1 + i}", im1 / im1);
        [TestMethod] public void IM1Square() => Test(@"{\left(-1 + i\right)}^{2}", MathS.Sqr(im1));
        [TestMethod] public void IM1SquareRoot() => Test(@"\sqrt{-1 + i}", MathS.Sqrt(im1));
        [TestMethod] public void IM1CubeRoot() => Test(@"\sqrt[3]{-1 + i}", MathS.Pow(im1, 1m/3));
        [TestMethod] public void IM1Pow() => Test(@"{\left(-1 + i\right)}^{x}", MathS.Pow(im1, x));
        [TestMethod] public void PowIM1() => Test(@"{2}^{-1 + i}", MathS.Pow(2, im1));
        [TestMethod] public void Mi() => TestSimplify("-i", -MathS.Sqrt(-1));
        [TestMethod] public void Trig() =>
            TestSimplify(@"\sin\left(\cos\left(\tan\left(\cot\left(x\right)\right)\right)\right)", MathS.Sin(MathS.Cos(MathS.Tan(MathS.Cotan(x)))));
        [TestMethod] public void SecCosec() =>
            TestSimplify(@"{\cos\left({\sin\left(x\right)}^{-1}\right)}^{-1}", MathS.Sec(MathS.Cosec(x)));
        [TestMethod] public void ArcTrig() =>
            TestSimplify(@"\arcsin\left(\arccos\left(\arctan\left(\arccot\left(x\right)\right)\right)\right)", MathS.Arcsin(MathS.Arccos(MathS.Arctan(MathS.Arccotan(x)))));
        [TestMethod] public void ArcSecCosec() =>
            TestSimplify(@"{\cos\left({\sin\left(x\right)}^{-1}\right)}^{-1}", MathS.Sec(MathS.Cosec(x)));
        [TestMethod] public void Log10() => Test(@"\log\left(10\right)", MathS.Log(10, 10));
        [TestMethod] public void Ln() => Test(@"\ln\left(10\right)", MathS.Ln(10));
        [TestMethod] public void LnAlternate() => Test(@"\ln\left(10\right)", MathS.Log(MathS.e, 10));
        [TestMethod] public void Log() => Test(@"\log_{2}\left(10\right)", MathS.Log(2, 10));
        [TestMethod] public void OO() => Test(@"\infty ", RealNumber.PositiveInfinity());
        [TestMethod] public void MOO() => Test(@"-\infty ", RealNumber.NegativeInfinity());
        [TestMethod] public void MOOAlternate() => Test(@"-\infty ", -RealNumber.PositiveInfinity());
        [TestMethod] public void OOPI() => Test(@"\infty  + i", RealNumber.PositiveInfinity() + MathS.i);
        [TestMethod] public void OOMI() => Test(@"\infty  - i", RealNumber.PositiveInfinity() - MathS.i);
        [TestMethod] public void OOPOOI() => Test(@"\infty  + \infty i", RealNumber.PositiveInfinity() * (1 + MathS.i));
        [TestMethod] public void OOMOOI() => Test(@"\infty  - \infty i", RealNumber.PositiveInfinity() * (1 - MathS.i));
        [TestMethod] public void MOOPOOI() => Test(@"-\infty  + \infty i", RealNumber.PositiveInfinity() * (-1 + MathS.i));
        [TestMethod] public void MOOMOOI() => Test(@"-\infty  - \infty i", RealNumber.PositiveInfinity() * (-1 - MathS.i));
        [TestMethod] public void Set0() => Test(@"\emptyset", MathS.Sets.Empty());
        [TestMethod] public void Set0Alternate() => Test(@"\emptyset", MathS.Sets.Finite());
        [TestMethod] public void Set1() => Test(@"\left\{1\right\}", MathS.Sets.Finite(1));
        [TestMethod] public void Set2() => Test(@"\left\{1,2\right\}", MathS.Sets.Finite(1, 2));
        [TestMethod] public void Set3() => Test(@"\left\{\sqrt{x},{x}^{2},\sin\left(x\right)\right\}", MathS.Sets.Finite(MathS.Sqrt(x), MathS.Sqr(x), MathS.Sin(x)));
        [TestMethod] public void SetR() => Test(@"\left\{\left(-\infty ,\infty \right)\right\}", MathS.Sets.R());
        [TestMethod] public void SetC() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(-\infty ,\infty \right)\wedge\Im\left(z\right)\in\left(-\infty ,\infty \right)\right\}\right\}", MathS.Sets.C());
        [TestMethod] public void SetIntervalCloseClose() =>
            Test(@"\left\{\left[1,2\right]\right\}", new Set(MathS.Sets.Interval(1, 2)));
        [TestMethod] public void SetIntervalCloseOpen() =>
            Test(@"\left\{\left[1,2\right)\right\}", new Set(MathS.Sets.Interval(1, 2).SetRightClosed(false)));
        [TestMethod] public void SetIntervalOpenClose() =>
            Test(@"\left\{\left(1,2\right]\right\}", new Set(MathS.Sets.Interval(1, 2).SetLeftClosed(false)));
        [TestMethod] public void SetIntervalOpenOpen() =>
            Test(@"\left\{\left(1,2\right)\right\}", new Set(MathS.Sets.Interval(1, 2).SetLeftClosed(false).SetRightClosed(false)));
        [TestMethod] public void SetIntervalCloseCloseCloseClose() =>
            Test(@"\left\{\left[1 - i,2 + 9i\right]\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, true).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalCloseCloseCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[1,2\right]\wedge\Im\left(z\right)\in\left[-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, true).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalCloseCloseOpenClose() =>
            Test(@"\left\{\left[1 - i,2 + 9i\right)\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, true).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalCloseCloseOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[1,2\right)\wedge\Im\left(z\right)\in\left[-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, true).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalCloseOpenCloseClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[1,2\right]\wedge\Im\left(z\right)\in\left(-1,9\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, false).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalCloseOpenCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[1,2\right]\wedge\Im\left(z\right)\in\left(-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, false).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalCloseOpenOpenClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[1,2\right)\wedge\Im\left(z\right)\in\left(-1,9\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, false).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalCloseOpenOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[1,2\right)\wedge\Im\left(z\right)\in\left(-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(true, false).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalOpenCloseCloseClose() =>
            Test(@"\left\{\left(1 - i,2 + 9i\right]\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, true).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalOpenCloseCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(1,2\right]\wedge\Im\left(z\right)\in\left[-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, true).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalOpenCloseOpenClose() =>
            Test(@"\left\{\left(1 - i,2 + 9i\right)\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, true).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalOpenCloseOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(1,2\right)\wedge\Im\left(z\right)\in\left[-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, true).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalOpenOpenCloseClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(1,2\right]\wedge\Im\left(z\right)\in\left(-1,9\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, false).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalOpenOpenCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(1,2\right]\wedge\Im\left(z\right)\in\left(-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, false).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalOpenOpenOpenClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(1,2\right)\wedge\Im\left(z\right)\in\left(-1,9\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, false).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalOpenOpenOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(1,2\right)\wedge\Im\left(z\right)\in\left(-1,9\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(1 - MathS.i, 2 + 9 * MathS.i).SetLeftClosed(false, false).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalVariableCloseClose() =>
            Test(@"\left\{\left[x-i,2-x\times i\right]\right\}", new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i)));
        [TestMethod] public void SetIntervalVariableCloseOpen() =>
            Test(@"\left\{\left[x-i,2-x\times i\right)\right\}", new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetRightClosed(false)));
        [TestMethod] public void SetIntervalVariableOpenClose() =>
            Test(@"\left\{\left(x-i,2-x\times i\right]\right\}", new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false)));
        [TestMethod] public void SetIntervalVariableOpenOpen() =>
            Test(@"\left\{\left(x-i,2-x\times i\right)\right\}", new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false).SetRightClosed(false)));
        [TestMethod] public void SetIntervalVariableCloseCloseCloseClose() =>
            Test(@"\left\{\left[x-i,2-x\times i\right]\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, true).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalVariableCloseCloseCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[\Re\left(x-i\right),\Re\left(2-x\times i\right)\right]\wedge\Im\left(z\right)\in\left[\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, true).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalVariableCloseCloseOpenClose() =>
            Test(@"\left\{\left[x-i,2-x\times i\right)\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, true).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalVariableCloseCloseOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[\Re\left(x-i\right),\Re\left(2-x\times i\right)\right)\wedge\Im\left(z\right)\in\left[\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, true).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalVariableCloseOpenCloseClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[\Re\left(x-i\right),\Re\left(2-x\times i\right)\right]\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, false).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalVariableCloseOpenCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[\Re\left(x-i\right),\Re\left(2-x\times i\right)\right]\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, false).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalVariableCloseOpenOpenClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[\Re\left(x-i\right),\Re\left(2-x\times i\right)\right)\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, false).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalVariableCloseOpenOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left[\Re\left(x-i\right),\Re\left(2-x\times i\right)\right)\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(true, false).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalVariableOpenCloseCloseClose() =>
            Test(@"\left\{\left(x-i,2-x\times i\right]\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, true).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalVariableOpenCloseCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(\Re\left(x-i\right),\Re\left(2-x\times i\right)\right]\wedge\Im\left(z\right)\in\left[\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, true).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalVariableOpenCloseOpenClose() =>
            Test(@"\left\{\left(x-i,2-x\times i\right)\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, true).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalVariableOpenCloseOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(\Re\left(x-i\right),\Re\left(2-x\times i\right)\right)\wedge\Im\left(z\right)\in\left[\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, true).SetRightClosed(false, false)));
        [TestMethod] public void SetIntervalVariableOpenOpenCloseClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(\Re\left(x-i\right),\Re\left(2-x\times i\right)\right]\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, false).SetRightClosed(true, true)));
        [TestMethod] public void SetIntervalVariableOpenOpenCloseOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(\Re\left(x-i\right),\Re\left(2-x\times i\right)\right]\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, false).SetRightClosed(true, false)));
        [TestMethod] public void SetIntervalVariableOpenOpenOpenClose() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(\Re\left(x-i\right),\Re\left(2-x\times i\right)\right)\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right]\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, false).SetRightClosed(false, true)));
        [TestMethod] public void SetIntervalVariableOpenOpenOpenOpen() =>
            Test(@"\left\{\left\{z\in\mathbb C:\Re\left(z\right)\in\left(\Re\left(x-i\right),\Re\left(2-x\times i\right)\right)\wedge\Im\left(z\right)\in\left(\Im\left(x-i\right),\Im\left(2-x\times i\right)\right)\right\}\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i).SetLeftClosed(false, false).SetRightClosed(false, false)));
        [TestMethod] public void SetItemsAndIntervals() =>
            Test(@"\left\{\left[x-i,2-x\times i\right],2,\begin{pmatrix}a & i\\\pi & e\end{pmatrix},\left[x-i,3-x\times i\right]\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i), 2, MathS.Matrices.Matrix(2, 2, "a", "i", "pi", "e"), MathS.Sets.Interval(x - MathS.i, 3 - x * MathS.i)));
        [TestMethod] public void SetSimplify() => Test(@"\left\{\log_{2}\left(4\right)\right\}", new Set(MathS.Log(2, 4), MathS.Sqrt(MathS.Sqrt(16))));
        [TestMethod] public void SetDuplicates() =>
            Test(@"\left\{\left[x-i,2-x\times i\right],2,\begin{pmatrix}a & i\\\pi & e\end{pmatrix},4,\left[x-i,3-x\times i\right],i\right\}",
                new Set(MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i), 2, MathS.Matrices.Matrix(2, 2, "a", "i", "pi", "e"), 2+2, MathS.Sets.Interval(x - MathS.i, 2 - x * MathS.i), MathS.Sqrt(16), MathS.Sets.Interval(x - MathS.i, 3 - x * MathS.i), MathS.Matrices.Matrix(2, 2, "a", "i", "pi", "e"), MathS.i));
        [TestMethod] public void Matrix() =>
            Test(@"\begin{pmatrix}11 & 12\\21 & 22\\31 & 32\end{pmatrix}", MathS.Matrices.Matrix(3, 2, 11, 12, 21, 22, 31, 32));
        [TestMethod] public void MatrixRow() =>
            Test(@"\begin{pmatrix}11 & 12 & 21 & 22 & 31 & 32\end{pmatrix}", MathS.Matrices.Matrix(1, 6, 11, 12, 21, 22, 31, 32));
        [TestMethod] public void MatrixColumn() =>
            Test(@"\begin{pmatrix}11\\12\\21\\22\\31\\32\end{pmatrix}", MathS.Matrices.Matrix(6, 1, 11, 12, 21, 22, 31, 32));
        [TestMethod] public void MatrixSingle() =>
            Test(@"\begin{pmatrix}x\end{pmatrix}", MathS.Matrices.Matrix(1, 1, x));
        [TestMethod] public void MatrixEmpty() =>
            Test(@"\begin{pmatrix}\end{pmatrix}", MathS.Matrices.Matrix(0, 0));
        [TestMethod] public void Vector() =>
            Test(@"\begin{bmatrix}11 & 12 & 21 & 22 & 31 & 32\end{bmatrix}", MathS.Matrices.Vector(11, 12, 21, 22, 31, 32));
        [TestMethod] public void VectorSingle() =>
            Test(@"\begin{bmatrix}x\end{bmatrix}", MathS.Matrices.Vector(x));
        [TestMethod] public void VectorEmpty() =>
            Test(@"\begin{bmatrix}\end{bmatrix}", MathS.Matrices.Vector());
    }
}