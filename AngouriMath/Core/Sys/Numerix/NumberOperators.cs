
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PeterO.Numbers;
using AngouriMath.Core.Numerix;

namespace AngouriMath
{
    partial record Entity
    {
        public abstract partial record Number
        {
            internal static EInteger CtxAdd(EInteger a, EInteger b) => a.Add(b);
            internal static EInteger CtxSubtract(EInteger a, EInteger b) => a.Subtract(b);
            internal static EInteger CtxMultiply(EInteger a, EInteger b) => a.Multiply(b);
            internal static ERational? CtxDivide(EInteger a, EInteger b) =>
                b.IsZero ? null : ERational.Create(a, b);
            internal static EInteger CtxMod(EInteger a, EInteger b) => a.Remainder(b);
            internal static EInteger CtxPow(EInteger a, EInteger b) => a.Pow(b);
            internal static ERational CtxAdd(ERational a, ERational b) => a.Add(b);
            internal static ERational CtxSubtract(ERational a, ERational b) => a.Subtract(b);
            internal static ERational CtxMultiply(ERational a, ERational b) => a.Multiply(b);
            internal static ERational? CtxDivide(ERational a, ERational b) =>
                b.IsZero ? null : a.Divide(b);
            internal static ERational CtxMod(ERational a, ERational b) => a.Remainder(b);
            internal static EDecimal CtxAdd(EDecimal a, EDecimal b)
                => a.Add(b, MathS.Settings.DecimalPrecisionContext);
            internal static EDecimal CtxSubtract(EDecimal a, EDecimal b)
                => a.Subtract(b, MathS.Settings.DecimalPrecisionContext);
            internal static EDecimal CtxMultiply(EDecimal a, EDecimal b)
                => a.Multiply(b, MathS.Settings.DecimalPrecisionContext);
            internal static EDecimal CtxDivide(EDecimal a, EDecimal b)
                => b.IsZero ? EDecimal.NaN : a.DivideToExponent(b, -MathS.Settings.DecimalPrecisionContext.Value.Precision);
            internal static EDecimal CtxMod(EDecimal a, EDecimal b)
                => a.Remainder(b, MathS.Settings.DecimalPrecisionContext);
            internal static EDecimal CtxPow(EDecimal a, EDecimal b)
                => a.Pow(b, MathS.Settings.DecimalPrecisionContext);
            internal static T Min<T>(T a, T b) where T : RealNumber => a < b ? a : b;
            internal static T Max<T>(T a, T b) where T : RealNumber => a > b ? a : b;
            internal static T OpSum<T>(T a, T b) where T : Number =>
                SuperSwitch(a, b,
                    (a, b) => IntegerNumber.Create(CtxAdd(a.Integer, b.Integer)),
                    (a, b) => RationalNumber.Create(CtxAdd(a.Rational, b.Rational)),
                    (a, b) => RealNumber.Create(CtxAdd(a.Decimal, b.Decimal)),
                    (a, b) => ComplexNumber.Create(CtxAdd(a.Real.Decimal, b.Real.Decimal), CtxAdd(a.Imaginary.Decimal, b.Imaginary.Decimal))
                 );
            internal static T OpSub<T>(T a, T b) where T : Number =>
                SuperSwitch(a, b,
                    (a, b) => IntegerNumber.Create(CtxSubtract(a.Integer, b.Integer)),
                    (a, b) => RationalNumber.Create(CtxSubtract(a.Rational, b.Rational)),
                    (a, b) => RealNumber.Create(CtxSubtract(a.Decimal, b.Decimal)),
                    (a, b) => ComplexNumber.Create(CtxSubtract(a.Real.Decimal, b.Real.Decimal), CtxSubtract(a.Imaginary.Decimal, b.Imaginary.Decimal))
                 );
            internal static T OpMul<T>(T a, T b) where T : Number =>
                SuperSwitch(a, b,
                    (a, b) => IntegerNumber.Create(CtxMultiply(a.Integer, b.Integer)),
                    (a, b) => RationalNumber.Create(CtxMultiply(a.Rational, b.Rational)),
                    (a, b) => RealNumber.Create(CtxMultiply(a.Decimal, b.Decimal)),
                    (a, b) =>
                    {
                    // Define both (oo * i) and (i * oo) to be (oo i) which is (0 + oo i) instead of (NaN + oo i)
                    static EDecimal ModifiedMultiply(EDecimal a, EDecimal b) =>
                            a.IsInfinity() && b.IsZero || b.IsInfinity() && a.IsZero ? EDecimal.Zero : CtxMultiply(a, b);
                        return ComplexNumber.Create(
                            CtxSubtract(ModifiedMultiply(a.Real.Decimal, b.Real.Decimal), ModifiedMultiply(a.Imaginary.Decimal, b.Imaginary.Decimal)),
                            CtxAdd(ModifiedMultiply(a.Real.Decimal, b.Imaginary.Decimal), ModifiedMultiply(a.Imaginary.Decimal, b.Real.Decimal)));
                    }
                 );
            internal static ComplexNumber OpDiv<T>(T a, T b) where T : Number =>
                SuperSwitch(a, b,
                    (a, b) => CtxDivide(a.Rational, b.Rational) is { } n ? RationalNumber.Create(n) : RealNumber.NaN,
                    (a, b) => CtxDivide(a.Rational, b.Rational) is { } n ? RationalNumber.Create(n) : RealNumber.NaN,
                    (a, b) => RealNumber.Create(CtxDivide(a.Decimal, b.Decimal)),
                    (a, b) =>
                    {
                    /*
                     * (a + ib) / (c + id) = (a + ib) * (1 / (c + id))
                     * 1 / (c + id) = (c2 + d2) / (c + id) / (c2 + d2) = (c - id) / (c2 + d2)
                     * => ans = (a + ib) * (c - id) / (c2 + d2)
                     */
                        var conj = b.Conjugate();
                        var bAbs = b.Abs().Decimal;
                        var abs2 = CtxMultiply(bAbs, bAbs);
                        var Re = CtxDivide(conj.Real.Decimal, abs2);
                        var Im = CtxDivide(conj.Imaginary.Decimal, abs2);
                        var c = ComplexNumber.Create(Re, Im);
                        return a * c;
                    }
                 );
            internal static bool AreEqual<T>(T a, T b) where T : Number =>
                SuperSwitch(a, b,
                    (a, b) => a.Integer.Equals(b.Integer),
                    (a, b) => a.Rational.Equals(b.Rational),
                    (a, b) => a.IsFinite && b.IsFinite && IsZero(CtxSubtract(a.Decimal, b.Decimal)) ||
                             !a.IsFinite && !b.IsFinite && a.Decimal.Equals(b.Decimal),
                    (a, b) => AreEqual(a.Real, b.Real) && AreEqual(a.Imaginary, b.Imaginary)
                 );

            public static Number operator +(Number a, Number b) => OpSum(a, b);
            public static Number operator -(Number a, Number b) => OpSub(a, b);
            public static Number operator *(Number a, Number b) => OpMul(a, b);
            public static Number operator /(Number a, Number b) => OpDiv(a, b);
            public static Number operator +(Number a) => a;
            public static Number operator -(Number a) => OpMul(-1, a);
            public static bool operator ==(Number a, Number b) => AreEqual(a, b);
            public static bool operator !=(Number a, Number b) => !AreEqual(a, b);

            internal static RealNumber? FindGoodRoot(ComplexNumber @base, IntegerNumber power)
            {
                RealNumber? positive = null, real = null;
                foreach (var root in GetAllRoots(@base, power.Integer))
                    switch (MathS.Settings.FloatToRationalIterCount.As(15, () =>
                        MathS.Settings.PrecisionErrorZeroRange.As(1e-6m, () =>
                            ComplexNumber.Create(root.Real, root.Imaginary))))
                    {
                        case RationalNumber rational when IsZero(Pow(rational, power) - @base):  // To keep user's desired precision
                            return rational;
                        case RealNumber r when r > 0:
                            positive ??= r;
                            break;
                        case RealNumber r:
                            real ??= r;
                            break;
                    }
                return positive ?? real;
            }
            private static EDecimal Hypot(EDecimal a, EDecimal b, EContext context)
            {
                // Using
                //   sqrt(a^2 + b^2) = |a| * sqrt(1 + (b/a)^2)
                // we can factor out the larger component to dodge overflow even when a * a would overflow.

                a = a.Abs();
                b = b.Abs();

                var (small, large) = a.LessThan(b) ? (a, b) : (b, a);

                if (small.IsZero)
                    return large;
                else if (large.IsPositiveInfinity() && !small.IsNaN())
                    // The NaN test is necessary so we don't return +inf when small=NaN and large=+inf.
                    // NaN in any other place returns NaN without any special handling.
                    return EDecimal.PositiveInfinity;
                else
                {
                    var ratio = small.Divide(large, context);
                    return ratio.MultiplyAndAdd(ratio, EDecimal.One, context).Sqrt(context).Multiply(large, context);
                }
            }

            /// <summary>Calculates the exact value of square root of num</summary>
            public static ComplexNumber Sqrt(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;

                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,7dc9c2ee4f99814a
                if (num is RealNumber { Decimal: var real })
                    if (real.IsNegative)
                        return ComplexNumber.Create(0, real.Negate().Sqrt(context));
                    else return RealNumber.Create(real.Sqrt(context));
                else
                {

                    // One way to compute Sqrt(z) is just to call Pow(z, 0.5), which coverts to polar coordinates
                    // (sqrt + atan), halves the phase, and reconverts to cartesian coordinates (cos + sin).
                    // Not only is this more expensive than necessary, it also fails to preserve certain expected
                    // symmetries, such as that the square root of a pure negative is a pure imaginary, and that the
                    // square root of a pure imaginary has exactly equal real and imaginary parts. This all goes
                    // back to the fact that Math.PI is not stored with infinite precision, so taking half of Math.PI
                    // does not land us on an argument with cosine exactly equal to zero.

                    // To find a fast and symmetry-respecting formula for complex square root,
                    // note x + i y = \sqrt{a + i b} implies x^2 + 2 i x y - y^2 = a + i b,
                    // so x^2 - y^2 = a and 2 x y = b. Cross-substitute and use the quadratic formula to obtain
                    //   x = \sqrt{\frac{\sqrt{a^2 + b^2} + a}{2}}  y = \pm \sqrt{\frac{\sqrt{a^2 + b^2} - a}{2}}
                    // There is just one complication: depending on the sign on a, either x or y suffers from
                    // cancelation when |b| << |a|. We can get aroud this by noting that our formulas imply
                    // x^2 y^2 = b^2 / 4, so |x| |y| = |b| / 2. So after computing the one that doesn't suffer
                    // from cancelation, we can compute the other with just a division. This is basically just
                    // the right way to evaluate the quadratic formula without cancelation.

                    // All this reduces our total cost to two sqrts and a few flops, and it respects the desired
                    // symmetries. Much better than atan + cos + sin!

                    // The signs are a matter of choice of branch cut, which is traditionally taken so x > 0 and sign(y) = sign(b).

                    // [Skipped overflow handling from original .NET Core source - we don't need it]

                    // This is the core of the algorithm. Everything else is special case handling.

                    var (re, im) = (num.Real.Decimal, num.Imaginary.Decimal);

                    EDecimal x, y;
                    if (!re.IsNegative)
                    {
                        x = Hypot(re, im, context).Add(re, context).Divide(2, context).Sqrt(context);
                        y = im.Divide(x.Multiply(2, context), context);
                    }
                    else
                    {
                        y = Hypot(re, im, context).Subtract(re, context).Divide(2, context).Sqrt(context);
                        if (im.IsNegative) y = -y;
                        x = im.Divide(y.Multiply(2, context), context);
                    }
                    return ComplexNumber.Create(x, y);
                }
            }

            /// <summary>exp(x) = e^x</summary>
            public static ComplexNumber Exp(ComplexNumber x)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                var expReal = x.Real.Decimal.Exp(context);
                var imaginary = x.Imaginary.Decimal;
                if (imaginary.IsZero)
                    return expReal;
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,7dc9c2ee4f99814a
                var cosImaginary = expReal.Multiply(imaginary.Cos(context), context);
                var sinImaginary = expReal.Multiply(imaginary.Sin(context), context);
                return ComplexNumber.Create(cosImaginary, sinImaginary);
            }

            /// <summary>e.g. Pow(2, 5) = 32</summary>
            /// <param name="base">The base of the exponential, base^power</param>
            /// <param name="power">The power of the exponential, base^power</param>
            public static ComplexNumber Pow(ComplexNumber @base, ComplexNumber power)
            {
                static ComplexNumber BinaryIntPow(ComplexNumber num, EInteger val)
                {
                    if (val.IsZero)
                        return 1;
                    if (val.Equals(1))
                        return num;
                    if (val.Equals(-1))
                        return 1 / num;
                    var divRem = val.DivRem(2); // divRem[0] == val / 2, divRem[1] == val % 2
                    return BinaryIntPow(num, divRem[0]) * BinaryIntPow(num, divRem[0]) * BinaryIntPow(num, divRem[1]);
                }
                // TODO: make it more detailed (e. g. +oo ^ +oo = +oo)
                if (@base.IsFinite && power is IntegerNumber { Integer: var pow })
                    return BinaryIntPow(@base, pow);

                if (@base.IsFinite && power is RationalNumber r && r.Rational.Denominator.Abs() < 10 // there should be a minimal threshold to avoid long searches 
                    && FindGoodRoot(@base, r.Rational.Denominator) is { } goodRoot)
                    return Pow(goodRoot, r.Rational.Numerator);

                var context = MathS.Settings.DecimalPrecisionContext;
                if (@base is RealNumber { Decimal: { IsNegative: false } realBase } && power is RealNumber { Decimal: var realPower })
                    return realBase.Pow(realPower, context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,7dc9c2ee4f99814a
                var baseReal = @base.Real.Decimal;
                var baseImaginary = @base.Imaginary.Decimal;
                var powerReal = power.Real.Decimal;
                var powerImaginary = power.Imaginary.Decimal;

                if (powerReal.IsZero && powerImaginary.IsZero)
                    return IntegerNumber.One;
                if (baseReal.IsZero && baseImaginary.IsZero)
                    return IntegerNumber.Zero;

                var rho = @base.Abs().Decimal;
                var theta = baseImaginary.Atan2(baseReal, context);
                var newRho = powerReal.MultiplyAndAdd(theta, powerImaginary.Multiply(rho.Log(context), context), context);
                var t = rho.Pow(powerReal, context).Multiply(powerImaginary.Multiply(-theta, context).Exp(context), context);

                return ComplexNumber.Create(t.Multiply(newRho.Cos(context), context), t.Multiply(newRho.Sin(context), context));
            }

            /// <summary>e.g. Log(2, 32) = 5</summary>
            /// <param name="base">Log's base, log(base, x) is a number y such that base^y = x</param>
            /// <param name="x">The number of which we want to get its base power</param>
            public static ComplexNumber Log(ComplexNumber @base, ComplexNumber x)
            {
                if (x is RealNumber real && real.Decimal.CompareTo(EDecimal.Zero) > 0 && @base is RealNumber realBase && realBase.Decimal.CompareTo(EDecimal.Zero) > 0)
                    return real.Decimal.LogN(realBase.Decimal, MathS.Settings.DecimalPrecisionContext);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                return Ln(x) / Ln(@base);
            }

            /// <summary>ln(x) = log(e, x)</summary>
            public static ComplexNumber Ln(ComplexNumber x)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (x is RealNumber { Decimal: { IsNegative: false } real })
                    return real.Log(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                return ComplexNumber.Create(x.Abs().Decimal.Log(context), x.Imaginary.Decimal.Atan2(x.Real.Decimal, context));
            }

            /// <summary>Calculates the exact value of sine of num</summary>
            public static ComplexNumber Sin(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is RealNumber { Decimal: var real })
                    return real.Sin(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                var (re, im) = (num.Real.Decimal, num.Imaginary.Decimal);
                // We need both sinh and cosh of imaginary part.
                // To avoid multiple calls to Exp with the same value,
                // we compute them both here from a single call to Exp.
                var p = im.Exp(context);
                var q = EDecimal.One.Divide(p, context);
                var sinh = p.Subtract(q, context).Divide(2, context);
                var cosh = p.Add(q, context).Divide(2, context);
                return ComplexNumber.Create(
                    re.Sin(context).Multiply(cosh, context),
                    re.Cos(context).Multiply(sinh, context));
            }

            /// <summary>Calculates the exact value of cosine of num</summary>
            public static ComplexNumber Cos(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is RealNumber { Decimal: var real })
                    return real.Cos(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                var (re, im) = (num.Real.Decimal, num.Imaginary.Decimal);
                var p = im.Exp(context);
                var q = EDecimal.One.Divide(p, context);
                var sinh = p.Subtract(q, context).Divide(2, context);
                var cosh = p.Add(q, context).Divide(2, context);
                return ComplexNumber.Create(
                    re.Cos(context).Multiply(cosh, context),
                    -re.Sin(context).Multiply(sinh, context));
            }

            /// <summary>Calculates the exact value of tangent of num</summary>
            public static ComplexNumber Tan(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is RealNumber { Decimal: var real })
                    return real.Tan(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1

                // tan z = sin z / cos z, but to avoid unnecessary repeated trig computations, use
                //   tan z = (sin(2x) + i sinh(2y)) / (cos(2x) + cosh(2y))
                // (see Abramowitz & Stegun 4.3.57 or derive by hand), and compute trig functions here.

                // This approach does not work for |y| > ~355, because sinh(2y) and cosh(2y) overflow,
                // even though their ratio does not. In that case, divide through by cosh to get:
                //   tan z = (sin(2x) / cosh(2y) + i \tanh(2y)) / (1 + cos(2x) / cosh(2y))
                // which correctly computes the (tiny) real part and the (normal-sized) imaginary part.

                var x2 = num.Real.Decimal.Multiply(2, context);
                var y2 = num.Imaginary.Decimal.Multiply(2, context);
                var p = y2.Exp(context);
                var q = EDecimal.One.Divide(p, context);
                var cosh = p.Add(q, context).Divide(2, context);
                if (num.Imaginary.Decimal.Abs().LessThanOrEquals(4))
                {
                    var sinh = p.Subtract(q, context).Divide(2, context);
                    var D = x2.Cos(context).Add(cosh, context);
                    return ComplexNumber.Create(x2.Sin(context).Divide(D, context), sinh.Divide(D, context));
                }
                else
                {
                    var D = x2.Cos(context).Divide(cosh, context).Increment();
                    return ComplexNumber.Create(
                        x2.Sin(context).Divide(cosh, context).Divide(D, context),
                        y2.Tanh(context).Divide(D, context));
                }
            }

            /// <summary>Calculates the exact value of cotangent of num</summary>
            public static ComplexNumber Cotan(ComplexNumber num)
            {
                var cotan = IntegerNumber.One / Tan(num);
                if (cotan.Real.Decimal.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange)
                    && cotan.Imaginary.Decimal.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange))
                    return IntegerNumber.Zero;
                else return cotan;
            }

            // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1

            // This method for the inverse complex sine (and cosine) is described in Hull, Fairgrieve,
            // and Tang, "Implementing the Complex Arcsine and Arccosine Functions Using Exception Handling",
            // ACM Transactions on Mathematical Software (1997)
            // (https://www.researchgate.net/profile/Ping_Tang3/publication/220493330_Implementing_the_Complex_Arcsine_and_Arccosine_Functions_Using_Exception_Handling/links/55b244b208ae9289a085245d.pdf)

            // First, the basics: start with sin(w) = (e^{iw} - e^{-iw}) / (2i) = z. Here z is the input
            // and w is the output. To solve for w, define t = e^{i w} and multiply through by t to
            // get the quadratic equation t^2 - 2 i z t - 1 = 0. The solution is t = i z + sqrt(1 - z^2), so
            //   w = arcsin(z) = - i log( i z + sqrt(1 - z^2) )
            // Decompose z = x + i y, multiply out i z + sqrt(1 - z^2), use log(s) = |s| + i arg(s), and do a
            // bunch of algebra to get the components of w = arcsin(z) = u + i v
            //   u = arcsin(beta)  v = sign(y) log(alpha + sqrt(alpha^2 - 1))
            // where
            //   alpha = (rho + sigma) / 2      beta = (rho - sigma) / 2
            //   rho = sqrt((x + 1)^2 + y^2)    sigma = sqrt((x - 1)^2 + y^2)
            // These formulas appear in DLMF section 4.23. (http://dlmf.nist.gov/4.23), along with the analogous
            //   arccos(w) = arccos(beta) - i sign(y) log(alpha + sqrt(alpha^2 - 1))
            // So alpha and beta together give us arcsin(w) and arccos(w).

            static (EDecimal beta, EDecimal v) ArcSinCosInner(ComplexNumber num, EContext context)
            {
                var (x, y) = (num.Real.Decimal, num.Imaginary.Decimal);
                var xp1 = x.Increment();
                var xm1 = x.Decrement();
                var rho = xp1.MultiplyAndAdd(xp1, y.Multiply(y, context), context).Sqrt(context);
                var sigma = xm1.MultiplyAndAdd(xm1, y.Multiply(y, context), context).Sqrt(context);
                var alpha = rho.Add(sigma, context).Divide(2, context);
                return (rho.Subtract(sigma, context).Divide(2, context),
                    alpha.MultiplyAndSubtract(alpha, EDecimal.One, context).Sqrt(context).Add(alpha, context).Log(context).Multiply((y.IsNegative || y.IsZero) ? -1 : 1, context));
            }

            /// <summary>Calculates the exact value of arcsine of num</summary>
            public static ComplexNumber Arcsin(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is RealNumber { Decimal: var real } && !(real.GreaterThan(EDecimal.One) || real.LessThan(-EDecimal.One)))
                    return real.Asin(context);
                var (beta, v) = ArcSinCosInner(num, context);
                return ComplexNumber.Create(beta.Asin(context), v);
            }

            /// <summary>Calculates the exact value of arccosine of num</summary>
            public static ComplexNumber Arccos(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is RealNumber { Decimal: var real } && !(real.GreaterThan(EDecimal.One) || real.LessThan(-EDecimal.One)))
                    return real.Acos(context);
                var (beta, v) = ArcSinCosInner(num, context);
                return ComplexNumber.Create(beta.Acos(context), -v);
            }

            /// <summary>Calculates the exact value of arctangent of num</summary>
            public static ComplexNumber Arctan(ComplexNumber num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is RealNumber { Decimal: var real })
                    return real.Atan(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                var one = IntegerNumber.One;
                var two = IntegerNumber.Create(2);
                var i = ComplexNumber.ImaginaryOne;
                return i / two * (Ln(one - i * num) - Ln(one + i * num));
            }

            /// <summary>Calculates the exact value of arccotangent of num</summary>
            public static ComplexNumber Arccotan(ComplexNumber num) => Arctan(1 / num);

            // From https://github.com/eobermuhlner/big-math/blob/ba75e9a80f040224cfeef3c2ac06390179712443/ch.obermuhlner.math.big/src/main/java/ch/obermuhlner/math/big/BigComplexMath.java
            /**
             * <summary>
             * Calculates the factorial of the specified <see cref="ComplexNumber"/>.
             *
             * <para>This implementation uses
             * <a href="https://en.wikipedia.org/wiki/Spouge%27s_approximation">Spouge's approximation</a>
             * to calculate the factorial for non-integer values.</para>
             *
             * <para>This involves calculating a series of constants that depend on the desired precision.
             * Since this constant calculation is quite expensive (especially for higher precisions),
             * the constants for a specific precision will be cached
             * and subsequent calls to this method with the same precision will be much faster.</para>
             *
             * <para>It is therefore recommended to do one call to this method with the standard precision of your application during the startup phase
             * and to avoid calling it with many different precisions.</para>
             *
             * <para>See: <a href="https://en.wikipedia.org/wiki/Factorial#Extension_of_factorial_to_non-integer_values_of_argument">Wikipedia: Factorial - Extension of factorial to non-integer values of argument</a></para>
             * </summary>
             * <param name="x">The <see cref="ComplexNumber"/></param>
             * <returns>The factorial <see cref="ComplexNumber"/></returns>
             * <seealso cref="PeterONumbersExtensions.Factorial(EDecimal, EContext)"/>
             * <seealso cref="Gamma(ComplexNumber)"/>
             */
            public static ComplexNumber Factorial(ComplexNumber x)
            {
                var mathContext = MathS.Settings.DecimalPrecisionContext.Value;
                switch (x)
                {
                    case IntegerNumber { Integer: var value } when value.Sign >= 0:
                        return value.Factorial();
                    case RealNumber { Decimal: var value }:
                        return value.Factorial(mathContext);
                }

                if (!mathContext.Precision.CanFitInInt32())
                    throw new ArgumentOutOfRangeException($"The precision of the {nameof(mathContext)} is outside the int32 range");

                ComplexNumber result = RealNumber.NaN;

                MathS.Settings.DowncastingEnabled.As(false, () =>
                MathS.Settings.DecimalPrecisionContext.As(mathContext.WithBigPrecision(mathContext.Precision << 1), () =>
                {
                // https://en.wikipedia.org/wiki/Spouge%27s_approximation
                int a = mathContext.Precision.ToInt32Checked() * 13 / 10;

                    var constants = PeterONumbersExtensions.GetSpougeFactorialConstants(a);

                    var negative = false;
                    var factor = ComplexNumber.Create(constants[0], 0);
                    for (int k = 1; k < a; k++)
                    {
                        factor += constants[k] / (x + k);
                        negative = !negative;
                    }

                    result = Pow(x + a, x + 0.5m) * Exp(-x - a) * factor;
                }));
                return ComplexNumber.Create(result.Real.Decimal.RoundToPrecision(mathContext),
                                            result.Imaginary.Decimal.RoundToPrecision(mathContext));
            }

            /**<summary>
             * Calculates the gamma function of the specified <see cref="ComplexNumber"/>.
             *
             * <para>This implementation uses {@link #factorial(ComplexNumber, MathContext)} internally,
             * therefore the performance implications described there apply also for this method.</para>
             *
             * <para>See: <a href="https://en.wikipedia.org/wiki/Gamma_function">Wikipedia: Gamma function</a></para>
             * </summary>
             * <param name="x">The <see cref="ComplexNumber"/></param>
             * <returns>The gamma <see cref="ComplexNumber"/></returns>
             * <seealso cref="PeterONumbersExtensions.Gamma(EDecimal, EContext)"/>
             * <seealso cref="Factorial(ComplexNumber)"/>
             */
            public static ComplexNumber Gamma(ComplexNumber x) => Factorial(x - IntegerNumber.One);
        }
    }
}