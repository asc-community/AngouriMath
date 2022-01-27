//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using static AngouriMath.Entity.Number;
using Xunit;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Convenience
{
    public sealed class LatexTests
    {
        private static readonly Entity x = MathS.Var(nameof(x));
        private static readonly Entity frac34 = Rational.Create(3, 4);
        private static readonly Entity m1 = -1;
        private static readonly Entity im1 = MathS.i - 1;
        private static readonly Entity m2i = -2 * MathS.i;
        private static readonly Entity numPi = MathS.DecimalConst.pi;
        private static readonly Entity mNumPi = -MathS.DecimalConst.pi;
        void Test(string expected, ILatexiseable actual) =>
            Assert.Equal(expected, actual.Latexise());
        void TestSimplify(string expected, Entity actual) =>
            Test(expected, actual.Simplify());
        [Fact] public void Num0() => TestSimplify("0", 0);
        [Fact] public void Num123() => TestSimplify("123", 123);
        [Fact] public void Float() => Test(@"\frac{15432}{125}", (Entity)123.456m);
        [Fact] public void FloatSimplify() => TestSimplify(@"\frac{15432}{125}", 123.456m);
        [Fact] public void FloatParse() => Test(@"3", Complex.Parse("3.000"));
        [Fact] public void FloatZero() => Test(@"123.4561234567890", Complex.Parse("123.4561234567890"));
        [Fact] public void Pi() => Test(@"\pi", MathS.pi);
        [Fact] public void E() => Test(@"e", MathS.e);
        [Fact] public void I() => TestSimplify("i", MathS.Sqrt(-1));
        [Fact] public void Variable() => Test("x", x);
        [Fact] public void Add() => Test("x+x", x + x);
        [Fact] public void AddAdd() => Test("x+x+x", x + x + x);
        [Fact] public void AddSimplify() => TestSimplify(@"2 x", x + x);
        [Fact] public void Subtract() => Test("x-x", x - x);
        [Fact] public void SubtractSubtract() => Test("x-x-x", x - x - x);
        [Fact] public void SubtractSimplify() => TestSimplify("0", x - x);
        [Fact] public void Multiply() => Test(@"x x", x * x);
        [Fact] public void MultiplyMultiply() => Test(@"x x x", x * x * x);
        [Fact] public void MultiplySimplify() => TestSimplify("{x}^{2}", x * x);
        [Fact] public void Divide() => Test(@"\frac{x}{x}", x / x);
        [Fact] public void DivideDivide() => Test(@"\frac{\frac{x}{x}}{x}", x / x / x);
        [Fact] public void DivideSimplify() => TestSimplify(@"1", x / x);
        [Fact] public void Greek1() => Test(@"\alpha", MathS.Var("alpha"));
        [Fact] public void Greek2() => Test(@"\beta", MathS.Var("beta"));
        [Fact] public void Greek3() => Test(@"a_{\beta}", MathS.Var("a_beta"));
        [Fact] public void Greek4() => Test(@"\alpha_{3}", MathS.Var("alpha_3"));
        [Fact] public void Greek5() => Test(@"\alpha_{\beta}", MathS.Var("alpha_beta"));
        [Fact] public void Greek6() => Test(@"\beta_{\alpha}", MathS.Var("beta_alpha"));
        [Fact] public void SubscriptSingle() => Test(@"c_{e}", MathS.Var("c_e"));
        [Fact] public void SubscriptDuo() => Test(@"\Delta_{a2}", MathS.Var("Delta_a2"));
        [Fact] public void SubscriptDuoPlus() => TestSimplify(@"6+\Delta_{a2}", "Delta_a2+6");
        [Fact] public void Square() => Test(@"{x}^{2}", MathS.Sqr(x));
        [Fact] public void SquareSquare() => Test(@"{\left({x}^{2}\right)}^{2}", MathS.Sqr(MathS.Sqr(x)));
        [Fact] public void SquareRoot() => Test(@"\sqrt{x}", MathS.Sqrt(x));
        [Fact] public void SquareRootAsPow() => Test(@"\sqrt{x}", MathS.Pow(x, 1m/2));
        [Fact] public void Cube() => Test(@"{x}^{3}", MathS.Pow(x, 3));
        [Fact] public void CubeRoot() => Test(@"\sqrt[3]{x}", MathS.Pow(x, 1m/3));
        [Fact] public void FourthRoot() => Test(@"\sqrt[4]{x}", MathS.Pow(x, 1m/4));
        [Fact] public void FourthRootCube() => Test(@"\sqrt[4]{x}^{3}", MathS.Pow(x, 3m/4));
        [Fact] public void M3Root() => Test(@"\frac{1}{\sqrt[3]{x}}", MathS.Pow(x, 1m/-3));
        [Fact] public void M4RootCube() => Test(@"\frac{1}{\sqrt[4]{x}^{3}}", MathS.Pow(x, 3m/-4));
        [Fact] public void PowBase2() => Test("{2}^{x}", MathS.Pow(2, x));
        [Fact] public void PowBase10() => Test("{10}^{x}", MathS.Pow(10, x));
        [Fact] public void Pow() => Test("{x}^{x}", MathS.Pow(x, x));
        [Fact] public void PowPow() => Test("{x}^{{x}^{x}}", MathS.Pow(x, MathS.Pow(x, x)));
        [Fact] public void PowPowNumber() => Test(@"{\left({12}^{23}\right)}^{34}", MathS.Pow(MathS.Pow(12, 23), 34));
        [Fact] public void MPow() => Test(@"{\left(-x\right)}^{x}", MathS.Pow(-x, x));
        [Fact] public void AddMultiply() =>
            Test(@"\left(1+2\right) \left(3+4\right)", ((Entity)1 + 2) * ((Entity)3 + 4));
        [Fact] public void SubtractMultiply() =>
            Test(@"\left(1-2\right) \left(3-4\right)", ((Entity)1 - 2) * ((Entity)3 - 4));
        [Fact] public void AddPow() => Test(@"{\left(3+4\right)}^{x}", MathS.Pow("3+4", x));
        [Fact] public void SubtractPow() => Test(@"{\left(3-4\right)}^{x}", MathS.Pow("3-4", x));
        [Fact] public void MultiplyPow() => Test(@"{\left(3 \cdot 4\right)}^{x}", MathS.Pow("3*4", x));
        [Fact] public void DividePow() => Test(@"{\left(\frac{3}{4}\right)}^{x}", MathS.Pow("3/4", x));
        [Fact] public void XSquaredMinusX() => TestSimplify(@"{x}^{2}-x", "x^2-x");
        [Fact] public void XSquaredMinusXAlternate() => TestSimplify(@"{x}^{2}-x", "-x+x^2");
        [Fact] public void XSquaredMinusXAlternate2() => TestSimplify(@"{x}^{2}-x", "x^2+(-1)*x");
        [Fact] public void M1() => Test("-1", (Complex)(-1));
        [Fact] public void M1Entity() => Test("-1", m1);
        [Fact] public void M1Add() => Test(@"-1-1", m1 + m1);
        [Fact] public void M1Subtract() => Test(@"-1-\left(-1\right)", m1 - m1);
        [Fact] public void M1Multiply() => Test(@"1", m1 * m1);
        [Fact] public void M1Divide() => Test(@"\frac{-1}{-1}", m1 / m1);
        [Fact] public void PowM1() => Test(@"{x}^{-1}", MathS.Pow(x, m1));
        [Fact] public void M1Pow() => Test(@"{\left(-1\right)}^{x}", MathS.Pow(m1, x));
        [Fact] public void M2Pow() => Test(@"{\left(-2\right)}^{x}", MathS.Pow(-2, x));
        [Fact] public void PiPow() => Test(@"{\pi}^{x}", MathS.Pow(MathS.pi, x));
        [Fact] public void MPiPow() => Test(@"{\left(-\pi\right)}^{x}", MathS.Pow(-MathS.pi, x));
        [Fact] public void NumPiAdd() => Test(@"3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068+3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068", numPi + numPi);
        [Fact] public void NumPiSubtract() => Test(@"3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068", numPi - numPi);
        [Fact] public void NumPiMultiply() => Test(@"3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068 \cdot 3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068", numPi * numPi);
        [Fact] public void NumPiDivide() => Test(@"\frac{3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}{3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}", numPi / numPi);
        [Fact] public void NumPiPow() => Test(@"{3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}^{x}", MathS.Pow(numPi, x));
        [Fact] public void PowNumPi() => Test(@"{x}^{3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}", MathS.Pow(x, numPi));
        [Fact] public void MNumPiAdd() => Test(@"-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068", mNumPi + mNumPi);
        [Fact] public void MNumPiSubtract() => Test(@"-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068-\left(-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068\right)", mNumPi - mNumPi);
        [Fact] public void MNumPiMultiply() => Test(@"\left(-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068\right) \cdot \left(-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068\right)", mNumPi * mNumPi);
        [Fact] public void MNumPiDivide() => Test(@"\frac{-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}{-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}", mNumPi / mNumPi);
        [Fact] public void MNumPiPow() => Test(@"{\left(-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068\right)}^{x}", MathS.Pow(mNumPi, x));
        [Fact] public void PowMNumPi() => Test(@"{x}^{-3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117068}", MathS.Pow(x, mNumPi));
        [Fact] public void MI() => TestSimplify("-i", MathS.Sqrt(-1) * -1);
        [Fact] public void M2I() => TestSimplify("-2i", m2i);
        [Fact] public void IM1() => Test("-1 + i", im1);
        [Fact] public void MIM1() => Test("-1 - i", -MathS.i - 1);
        [Fact] public void M2IM1() => Test("-1 - 2i", -2 * MathS.i - 1);
        [Fact] public void IP1() => Test("1 + i", MathS.i + 1);
        [Fact] public void MIP1() => Test("1 - i", -1 * MathS.i + 1);
        [Fact] public void M2IP1() => Test("1 - 2i", -2 * MathS.i + 1);
        [Fact] public void M0IP1() => Test("1", 0 * -MathS.i + 1);
        [Fact] public void M0IM1() => Test("-1", 0 * -MathS.i - 1);
        [Fact] public void ISquare() => Test("{i}^{2}", MathS.Sqr(MathS.i));
        [Fact] public void ISquareSimplified() => TestSimplify("-1", MathS.Sqr(MathS.i));
        [Fact] public void Frac34Add() => Test(@"\frac{3}{4}+\frac{3}{4}", frac34 + frac34);
        [Fact] public void Frac34Subtract() => Test(@"\frac{3}{4}-\frac{3}{4}", frac34 - frac34);
        [Fact] public void Frac34Multiply() => Test(@"\frac{3}{4} \cdot \frac{3}{4}", frac34 * frac34);
        [Fact] public void Frac34Divide() => Test(@"\frac{\frac{3}{4}}{\frac{3}{4}}", frac34 / frac34);
        [Fact] public void Frac34Square() => Test(@"{\left(\frac{3}{4}\right)}^{2}", MathS.Sqr(frac34));
        [Fact] public void Frac34SquareRoot() => Test(@"\sqrt{\frac{3}{4}}", MathS.Sqrt(frac34));
        [Fact] public void Frac34CubeRoot() => Test(@"\sqrt[3]{\frac{3}{4}}", MathS.Pow(frac34, 1m/3));
        [Fact] public void Frac34Pow() => Test(@"{\left(\frac{3}{4}\right)}^{x}", MathS.Pow(frac34, x));
        [Fact] public void PowFrac34() => Test(@"\sqrt[4]{2}^{3}", MathS.Pow(2, frac34));
        [Fact] public void M2IAdd() => Test(@"-2i-2i", m2i + m2i);

        // Which is better,
        // -2i-\left(-2i\right)
        // or
        // -2i--2i
        // ? TODO
        [Fact] public void M2ISubtract() => Test(@"-2i--2i", m2i - m2i);
        [Fact] public void M2IMultiply() => Test(@"-2i \cdot -2i", m2i * m2i);
        [Fact] public void M2IDivide() => Test(@"\frac{-2i}{-2i}", m2i / m2i);
        [Fact] public void M2ISquare() => Test(@"{\left(-2i\right)}^{2}", MathS.Sqr(m2i));
        [Fact] public void M2ISquareRoot() => Test(@"\sqrt{-2i}", MathS.Sqrt(m2i));
        [Fact] public void M2ICubeRoot() => Test(@"\sqrt[3]{-2i}", MathS.Pow(m2i, 1m/3));
        [Fact] public void M2IPow() => Test(@"{\left(-2i\right)}^{x}", MathS.Pow(m2i, x));
        [Fact] public void PowM2I() => Test(@"{2}^{-2i}", MathS.Pow(2, m2i));
        [Fact] public void IM1Add() => Test(@"-1 + i-1 + i", im1 + im1);
        [Fact] public void IM1Subtract() => Test(@"-1 + i-\left(-1 + i\right)", im1 - im1);
        [Fact] public void IM1Multiply() => Test(@"\left(-1 + i\right) \cdot \left(-1 + i\right)", im1 * im1);
        [Fact] public void IM1Divide() => Test(@"\frac{-1 + i}{-1 + i}", im1 / im1);
        [Fact] public void IM1Square() => Test(@"{\left(-1 + i\right)}^{2}", MathS.Sqr(im1));
        [Fact] public void IM1SquareRoot() => Test(@"\sqrt{-1 + i}", MathS.Sqrt(im1));
        [Fact] public void IM1CubeRoot() => Test(@"\sqrt[3]{-1 + i}", MathS.Pow(im1, 1m/3));
        [Fact] public void IM1Pow() => Test(@"{\left(-1 + i\right)}^{x}", MathS.Pow(im1, x));
        [Fact] public void PowIM1() => Test(@"{2}^{-1 + i}", MathS.Pow(2, im1));
        [Fact] public void Mi() => TestSimplify("-i", -MathS.Sqrt(-1));
        [Fact] public void Trig() =>
            TestSimplify(@"\sin\left(\cos\left(\tan\left(\cot\left(x\right)\right)\right)\right)", MathS.Sin(MathS.Cos(MathS.Tan(MathS.Cotan(x)))));
        [Fact] public void SecCosec() =>
            TestSimplify(@"\sec\left(\csc\left(x\right)\right)", MathS.Sec(MathS.Cosec(x)));
        [Fact] public void ArcTrig() =>
            TestSimplify(@"\arcsin\left(\arccos\left(\arctan\left(\arccot\left(x\right)\right)\right)\right)", MathS.Arcsin(MathS.Arccos(MathS.Arctan(MathS.Arccotan(x)))));
        [Fact] public void ArcSecCosec() =>
            TestSimplify(@"\arcsec\left(\arccsc\left(x\right)\right)", MathS.Arcsec(MathS.Arccosec(x)));
        [Fact] public void Log10() => Test(@"\log\left(10\right)", MathS.Log(10, 10));
        [Fact] public void Ln() => Test(@"\ln\left(10\right)", MathS.Ln(10));
        [Fact] public void LnAlternate() => Test(@"\ln\left(10\right)", MathS.Log(MathS.e, 10));
        [Fact] public void Log() => Test(@"\log_{2}\left(10\right)", MathS.Log(2, 10));
        [Fact] public void Factorial1() => Test(@"1!", MathS.Factorial(1));
        [Fact] public void Factorial23() => Test(@"23!", MathS.Factorial(23));
        [Fact] public void FactorialM23() => Test(@"\left(-23\right)!", MathS.Factorial(-23));
        [Fact] public void FactorialI() => Test(@"i!", MathS.Factorial(MathS.i));
        [Fact] public void FactorialMI() => Test(@"\left(-i\right)!", MathS.Factorial(-MathS.i));
        [Fact] public void Factorial1MI() => Test(@"\left(1 - i\right)!", MathS.Factorial(1 - MathS.i));
        [Fact] public void Factorial1PI() => Test(@"\left(1 + i\right)!", MathS.Factorial(1 + MathS.i));
        [Fact] public void FactorialX() => Test(@"x!", MathS.Factorial(x));
        [Fact] public void FactorialSinX() => Test(@"\sin\left(x\right)!", MathS.Factorial(MathS.Sin(x)));
        // x!! is the double factorial, (x!)! is factorial applied twice which is different
        [Fact] public void FactorialFactorialX() => Test(@"\left(x!\right)!", MathS.Factorial(MathS.Factorial(x)));
        [Fact] public void OO() => Test(@"\infty ", Real.PositiveInfinity);
        [Fact] public void MOO() => Test(@"-\infty ", Real.NegativeInfinity);
        [Fact] public void MOOAlternate() => Test(@"-\infty ", -Real.PositiveInfinity);
        [Fact] public void OOPI() => Test(@"\infty  + i", Real.PositiveInfinity + MathS.i);
        [Fact] public void OOMI() => Test(@"\infty  - i", Real.PositiveInfinity - MathS.i);
        [Fact] public void OOPOOI() => Test(@"\infty  + \infty i", Real.PositiveInfinity * (1 + MathS.i));
        [Fact] public void OOMOOI() => Test(@"\infty  - \infty i", Real.PositiveInfinity * (1 - MathS.i));
        [Fact] public void MOOPOOI() => Test(@"-\infty  + \infty i", Real.PositiveInfinity * (-1 + MathS.i));
        [Fact] public void MOOMOOI() => Test(@"-\infty  - \infty i", Real.PositiveInfinity * (-1 - MathS.i));
        [Fact] public void Undefined() => TestSimplify(@"\mathrm{undefined}", Real.PositiveInfinity / Real.PositiveInfinity);
        [Fact] public void Set0() => Test(@"\emptyset", MathS.Sets.Empty);
        [Fact] public void Set0Alternate() => Test(@"\emptyset", MathS.Sets.Finite());
        [Fact] public void Matrix() =>
            Test(@"\begin{bmatrix}11 & 12 \\ 21 & 22 \\ 31 & 32\end{bmatrix}", MathS.Matrix(new Entity[,]
                {
                    { 11, 12 },
                    { 21, 22 },
                    { 31, 32 }
                }));
        [Fact] public void MatrixRow() =>
            Test(@"\begin{bmatrix}11 & 12 & 21 & 22 & 31 & 32\end{bmatrix}", MathS.Vector(11, 12, 21, 22, 31, 32).T);
        [Fact] public void MatrixColumn() =>
            Test(@"\begin{bmatrix}11 \\ 12 \\ 21 \\ 22 \\ 31 \\ 32\end{bmatrix}", MathS.Vector(11, 12, 21, 22, 31, 32));
        [Fact] public void MatrixSingle() =>
            Test(@"\begin{bmatrix}x\end{bmatrix}", MathS.Vector(x));
        [Fact] public void Vector() =>
            Test(@"\begin{bmatrix}11 & 12 & 21 & 22 & 31 & 32\end{bmatrix}", MathS.Vector(11, 12, 21, 22, 31, 32).T);
        [Fact] public void VectorSingle() =>
            Test(@"\begin{bmatrix}x\end{bmatrix}", MathS.Vector(x));
        [Fact] public void Derivative1() =>
            Test(@"\frac{d\left[x+1\right]}{dx}", MathS.Derivative("x + 1", x));
        [Fact] public void Derivative2() =>
            Test(@"\frac{d\left[x+y\right]}{d\left[quack\right]}", MathS.Derivative("x + y", "quack"));
        [Fact] public void Derivative3() =>
            Test(@"\frac{d^{3}\left[x+1\right]}{dx^{3}}", MathS.Derivative("x + 1", x, 3));
        [Fact] public void Derivative4() =>
            Test(@"\frac{d\left[\frac{1}{x}\right]}{d\left[xf\right]}", MathS.Derivative("1/x", "xf"));
        [Fact] public void Derivative5() =>
            Test(@"\frac{d\left[{x}^{23}-x_{16}\right]}{d\left[xf\right]}", MathS.Derivative("x^23-x_16", "xf"));
        [Fact] public void Integral1() =>
            Test(@"\int \left[x+1\right] dx", MathS.Integral("x + 1", x));
        [Fact] public void Integral2() =>
            Test(@"\int \int \left[x+1\right] dx dx", MathS.Integral("x + 1", x, 2));
        [Fact] public void Integral3() =>
            Test(@"\int \left[x+1\right] d\left[xf\right]", MathS.Integral("x + 1", "xf"));
        [Fact] public void Integral4() =>
            Test(@"\int \left[\frac{1}{x}\right] d\left[xf\right]", MathS.Integral("1/x", "xf"));
        [Fact] public void Integral5() =>
            Test(@"\int \left[{x}^{23}-x_{16}\right] d\left[xf\right]", MathS.Integral("x^23-x_16", "xf"));
        [Fact] public void Limit1() =>
            Test(@"\lim_{x\to 0^-} \left[x+y\right]", (Entity)"limitleft(x + y, x, 0)");
        [Fact] public void Limit2() =>
            Test(@"\lim_{x\to 0^+} {a}^{5}", (Entity)"limitright(a^5, x, 0)");
        [Fact] public void Limit3() =>
            Test(@"\lim_{x\to \infty } \left[x+y\right]", MathS.Limit("x + y", x, Real.PositiveInfinity));
        [Fact] public void Limit4() =>
            Test(@"\lim_{xf\to 1+x} \left[\frac{1}{x}\right]", MathS.Limit("1/x", "xf", "1+x"));
        [Fact] public void Limit5() =>
            Test(@"\lim_{xf\to {x_{2}}^{3}} \left[{x}^{23}-x_{16}\right]", MathS.Limit("x^23-x_16", "xf", "x_2^3"));
        [Fact] public void Interval1() =>
            Test(@"\left[3; 4\right]", MathS.Sets.Interval(3, true, 4, true));
        [Fact] public void Interval2() =>
            Test(@"\left(3; 4\right]", MathS.Sets.Interval(3, false, 4, true));
        [Fact] public void Interval3() =>
            Test(@"\left(3; 4\right)", MathS.Sets.Interval(3, false, 4, false));
        [Fact] public void Interval4() =>
            Test(@"\left[3; 4\right)", MathS.Sets.Interval(3, true, 4, false));
        [Fact] public void FiniteSet1() =>
            Test(@"\left\{ 1, 2, 3 \right\}", MathS.Sets.Finite(1, 2, 3));
        [Fact] public void FiniteSet2() =>
            Test(@"\left\{ 1 \right\}", MathS.Sets.Finite(1));
        [Fact] public void CSet1() =>
            Test(@"\left\{ x : x > 0 \right\}", new ConditionalSet("x", "x > 0"));
        [Fact] public void CSet2() =>
            Test(@"\left\{ x : \sqrt{2} \right\}", new ConditionalSet("x", "sqrt(2)"));
        [Fact] public void RR() =>
            Test(@"\mathbb{R}", (Entity)Domain.Real);
        [Fact] public void ZZ() =>
            Test(@"\mathbb{Z}", (Entity)Domain.Integer);
        [Fact] public void Signum()
            => Test(@"\operatorname{sgn}\left(x\right)", MathS.Signum(x));
        [Fact] public void Abs()
            => Test(@"\left|x\right|", MathS.Abs(x));
        [Fact] public void Phi1()
            => Test(@"\varphi(x)", MathS.NumberTheory.Phi(x));
        [Fact] public void Phi2()
            => Test(@"\varphi({x}^{2})", MathS.NumberTheory.Phi(MathS.Pow(x, 2)));
        [Fact] public void Phi3()
            => Test(@"{\varphi(x)}^{\varphi({x}^{2})}", MathS.Pow(MathS.NumberTheory.Phi(x), MathS.NumberTheory.Phi(MathS.Pow(x, 2))));
        [Fact] public void Piecewise1()
            => Test(@"\begin{cases}a \: \text{for} \: b\\c \: \text{for} \: e\end{cases}", MathS.Piecewise(("a", "b"), ("c", "e")));
        [Fact] public void Piecewise2()
            => Test(@"\begin{cases}a \: \text{for} \: b\\c \: \text{for} \: e\\g \: \text{otherwise}\end{cases}", MathS.Piecewise(("a", "b"), ("c", "e"), ("g", true)));
        [Fact] public void M1InTheMiddle()
            => Test(@"x \cdot \left(-1\right) \cdot x", (x * (-1)) * x);
    }
}

