//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core.Exceptions;
using PeterO.Numbers;

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
            internal static T Min<T>(T a, T b) where T : Real => a < b ? a : b;
            internal static T Max<T>(T a, T b) where T : Real => a > b ? a : b;

            /// <summary>
            /// This function serves not only convenience but also protects from unexpected cases, for example,
            /// if a new type added
            /// </summary>
            protected static T SuperSwitch<T>(
                Number num1, Number num2,
                Func<Integer, Integer, T> ifInt,
                Func<Rational, Rational, T> ifRat,
                Func<Real, Real, T> ifReal,
                Func<Complex, Complex, T> ifCom
            ) => (num1, num2) switch
            {
                (Integer n1, Integer n2) => ifInt(n1, n2),
                (Rational r1, Rational r2) => ifRat(r1, r2),
                (Real r1, Real r2) => ifReal(r1, r2),
                (Complex c1, Complex c2) => ifCom(c1, c2),
                _ => throw new AngouriBugException($"({num1.GetType()}, {num2.GetType()}) is not supported.")
            };

            /// <summary>
            /// This function serves not only convenience but also protects from unexpected cases, for example,
            /// if a new type added
            /// </summary>
            protected static T SuperSwitch<T>(
                T num1, T num2,
                Func<Integer, Integer, Integer> ifInt,
                Func<Rational, Rational, Rational> ifRat,
                Func<Real, Real, Real> ifReal,
                Func<Complex, Complex, Complex> ifCom
            ) where T : Number
                => (T)(Number)((num1, num2) switch
                {
                    (Integer n1, Integer n2) => ifInt(n1, n2),
                    (Rational r1, Rational r2) => ifRat(r1, r2),
                    (Real r1, Real r2) => ifReal(r1, r2),
                    (Complex c1, Complex c2) => ifCom(c1, c2),
                    _ => throw new AngouriBugException($"({num1.GetType()}, {num2.GetType()}) is not supported.")
                });

            internal static T OpSum<T>(T a, T b) where T : Number =>
                SuperSwitch<T>(a, b,
                    (a, b) => Integer.Create(CtxAdd(a.EInteger, b.EInteger)),
                    (a, b) => Rational.Create(CtxAdd(a.ERational, b.ERational)),
                    (a, b) => Real.Create(CtxAdd(a.EDecimal, b.EDecimal)),
                    (a, b) => Complex.Create(CtxAdd(a.RealPart.EDecimal, b.RealPart.EDecimal),
                                             CtxAdd(a.ImaginaryPart.EDecimal, b.ImaginaryPart.EDecimal))
                    );

            internal static T OpSub<T>(T a, T b) where T : Number =>
                SuperSwitch<T>(a, b,
                    (a, b) => Integer.Create(CtxSubtract(a.EInteger, b.EInteger)),
                    (a, b) => Rational.Create(CtxSubtract(a.ERational, b.ERational)),
                    (a, b) => Real.Create(CtxSubtract(a.EDecimal, b.EDecimal)),
                    (a, b) => Complex.Create(CtxSubtract(a.RealPart.EDecimal, b.RealPart.EDecimal),
                                             CtxSubtract(a.ImaginaryPart.EDecimal, b.ImaginaryPart.EDecimal))
                    );

            internal static T OpMul<T>(T a, T b) where T : Number =>
                SuperSwitch<T>(a, b,
                    (a, b) => Integer.Create(CtxMultiply(a.EInteger, b.EInteger)),
                    (a, b) => Rational.Create(CtxMultiply(a.ERational, b.ERational)),
                    (a, b) => Real.Create(CtxMultiply(a.EDecimal, b.EDecimal)),
                    (a, b) =>
                    {
                    // Define both (oo * i) and (i * oo) to be (oo i) which is (0 + oo i) instead of (NaN + oo i)
                    static EDecimal ModifiedMultiply(EDecimal a, EDecimal b) =>
                            a.IsInfinity() && b.IsZero || b.IsInfinity() && a.IsZero ? EDecimal.Zero : CtxMultiply(a, b);
                        return Complex.Create(
                            CtxSubtract(ModifiedMultiply(a.RealPart.EDecimal, b.RealPart.EDecimal), ModifiedMultiply(a.ImaginaryPart.EDecimal, b.ImaginaryPart.EDecimal)),
                            CtxAdd(ModifiedMultiply(a.RealPart.EDecimal, b.ImaginaryPart.EDecimal), ModifiedMultiply(a.ImaginaryPart.EDecimal, b.RealPart.EDecimal)));
                    }
                    );

            internal static Complex OpDiv<T>(T a, T b) where T : Number =>
                SuperSwitch<Complex>(a, b,
                    (a, b) => CtxDivide(a.ERational, b.ERational) is { } n ? Rational.Create(n) : Real.NaN,
                    (a, b) => CtxDivide(a.ERational, b.ERational) is { } n ? Rational.Create(n) : Real.NaN,
                    (a, b) => Real.Create(CtxDivide(a.EDecimal, b.EDecimal)),
                    (a, b) =>
                    {
                        /*
                        * (a + ib) / (c + id) = (a + ib) * (1 / (c + id))
                        * 1 / (c + id) = (c2 + d2) / (c + id) / (c2 + d2) = (c - id) / (c2 + d2)
                        * => ans = (a + ib) * (c - id) / (c2 + d2)
                        */
                        var conj = b.Conjugate;
                        var bAbs = b.Abs().EDecimal;
                        var abs2 = CtxMultiply(bAbs, bAbs);
                        var Re = CtxDivide(conj.RealPart.EDecimal, abs2);
                        var Im = CtxDivide(conj.ImaginaryPart.EDecimal, abs2);
                        var c = Complex.Create(Re, Im);
                        return a * c;
                    }
                    );

            /// <summary>
            /// Checks whether a number is zero
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool IsZero(Real num)
            {
                if (!num.IsFinite)
                    return false;
                return IsZero(num.EDecimal);
            }

            /// <summary>
            /// Checks whether a number is zero
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool IsZero(EDecimal num)
            {
                return num.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange);
            }

            /// <summary>
            /// Checks whether a number is zero
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool IsZero(Complex num) => IsZero(num.RealPart) && IsZero(num.ImaginaryPart);

            internal static bool AreEqual<T>(T a, T b) where T : Number =>
                SuperSwitch<bool>(a, b,
                    (a, b) => a.EInteger.Equals(b.EInteger),
                    (a, b) => a.ERational.Equals(b.ERational),
                    (a, b) => a.IsFinite && b.IsFinite
                                && CtxSubtract(a.EDecimal, b.EDecimal).Abs().LessThan(MathS.Settings.PrecisionErrorCommon)
                                || !a.IsFinite && !b.IsFinite && a.EDecimal.Equals(b.EDecimal),
                    (a, b) => AreEqual<Real>(a.RealPart, b.RealPart) && AreEqual<Real>(a.ImaginaryPart, b.ImaginaryPart)
                    );

#pragma warning disable CS1591
            public static Number operator +(Number a, Number b) => OpSum(a, b);
            public static Number operator -(Number a, Number b) => OpSub(a, b);
            public static Number operator *(Number a, Number b) => OpMul(a, b);
            public static Number operator /(Number a, Number b) => OpDiv(a, b);
            public static Number operator +(Number a) => a;
            public static Number operator -(Number a) => OpMul(-1, a);
#pragma warning restore CS1591

            /// <summary>
            /// Gets all n-th roots of a number,
            /// that is, all numbers whose n-th power equals 1
            /// </summary>
            public static IEnumerable<Entity> GetAllRootsOf1(EInteger rootPower)
            {
                for (int i = 0; i < rootPower; i++)
                {
                    var angle = Rational.Create(i * 2, rootPower) * MathS.pi;
                    yield return (MathS.Cos(angle) + MathS.i * MathS.Sin(angle)).InnerSimplified;
                }
            }

            /// <summary>
            /// Finds all complex roots of a number
            /// e. g. sqrt(1) = { -1, 1 }
            /// root(1, 4) = { -i, i, -1, 1 }
            /// </summary>
            public static HashSet<Complex> GetAllRoots(Complex value, EInteger rootPower)
            {
                // Avoid infinite recursion from Abs to GetAllRoots again
                using var _ = MathS.Settings.FloatToRationalIterCount.Set(0);
                var res = new HashSet<Complex>();
                EDecimal phi = (Ln(value / value.Abs()) / MathS.i).RealPart.EDecimal;
                if (phi.IsNaN()) // (value / value.Abs()) is NaN when value is zero
                    phi = EDecimal.Zero;

                EDecimal newMod = Pow(value.Abs(), CtxDivide(EDecimal.One, rootPower)).RealPart.EDecimal;

                var i = Complex.ImaginaryOne;
                for (int n = 0; n < rootPower; n++)
                {
                    EDecimal newPow = CtxAdd(CtxDivide(phi, rootPower),
                        CtxDivide(CtxMultiply(CtxMultiply(2, MathS.DecimalConst.pi), n), rootPower));
                    var root = newMod * Exp(i * newPow);
                    res.Add(root);
                }
                return res;
            }

            internal static Real? FindGoodRoot(Complex @base, Integer power)
            {
                Real? positive = null, real = null;
                foreach (var root in GetAllRoots(@base, power.EInteger))
                {
                    using var _ = MathS.Settings.FloatToRationalIterCount.Set(15);
                    using var __ = MathS.Settings.PrecisionErrorZeroRange.Set(1e-6m);
                    var toCheck = Complex.Create(root.RealPart, root.ImaginaryPart);
                    switch (toCheck)
                    {
                        case Rational rational when IsZero(Pow(rational, power) - @base):  // To keep user's desired precision
                            return rational;
                        case Real r when r > 0:
                            positive ??= r;
                            break;
                        case Real r:
                            real ??= r;
                            break;
                    }
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
            public static Complex Sqrt(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;

                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,7dc9c2ee4f99814a
                if (num is Real { EDecimal: var real })
                    if (real.IsNegative)
                        return Complex.Create(0, real.Negate().Sqrt(context));
                    else return Real.Create(real.Sqrt(context));
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

                    var (re, im) = (num.RealPart.EDecimal, num.ImaginaryPart.EDecimal);

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
                    return Complex.Create(x, y);
                }
            }

            /// <summary>exp(x) = e^x</summary>
            public static Complex Exp(Complex x)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                var expReal = x.RealPart.EDecimal.Exp(context);
                var imaginary = x.ImaginaryPart.EDecimal;
                if (imaginary.IsZero)
                    return expReal;
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,7dc9c2ee4f99814a
                var cosImaginary = expReal.Multiply(imaginary.Cos(context), context);
                var sinImaginary = expReal.Multiply(imaginary.Sin(context), context);
                return Complex.Create(cosImaginary, sinImaginary);
            }

            /// <summary>e.g. Pow(2, 5) = 32</summary>
            /// <param name="base">The base of the exponential, base^power</param>
            /// <param name="power">The power of the exponential, base^power</param>
            public static Complex Pow(Complex @base, Complex power)
            {
                static Complex BinaryIntPow(Complex num, EInteger val)
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
                if (@base.IsFinite && power is Integer { EInteger: var pow })
                    return BinaryIntPow(@base, pow);

                if (@base.IsFinite && power is Rational r && r.ERational.Denominator.Abs() < 10 // there should be a minimal threshold to avoid long searches 
                    && FindGoodRoot(@base, r.ERational.Denominator) is { } goodRoot)
                    return Pow(goodRoot, r.ERational.Numerator);

                var context = MathS.Settings.DecimalPrecisionContext;
                if (@base is Real { EDecimal: { IsNegative: false } realBase } && power is Real { EDecimal: var realPower })
                    return realBase.Pow(realPower, context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,7dc9c2ee4f99814a
                var baseReal = @base.RealPart.EDecimal;
                var baseImaginary = @base.ImaginaryPart.EDecimal;
                var powerReal = power.RealPart.EDecimal;
                var powerImaginary = power.ImaginaryPart.EDecimal;

                if (powerReal.IsZero && powerImaginary.IsZero)
                    return Integer.One;
                if (baseReal.IsZero && baseImaginary.IsZero)
                    return Integer.Zero;

                var rho = @base.Abs().EDecimal;
                var theta = baseImaginary.Arctan2(baseReal, context);
                var newRho = powerReal.MultiplyAndAdd(theta, powerImaginary.Multiply(rho.Log(context), context), context);
                var t = rho.Pow(powerReal, context).Multiply(powerImaginary.Multiply(-theta, context).Exp(context), context);

                return Complex.Create(t.Multiply(newRho.Cos(context), context), t.Multiply(newRho.Sin(context), context));
            }

            /// <summary>e.g. Log(2, 32) = 5</summary>
            /// <param name="base">Log's base, log(base, x) is a number y such that base^y = x</param>
            /// <param name="x">The number of which we want to get its base power</param>
            public static Complex Log(Complex @base, Complex x)
            {
                if (x is Real real && real.EDecimal.CompareTo(EDecimal.Zero) > 0 && @base is Real realBase && realBase.EDecimal.CompareTo(EDecimal.Zero) > 0)
                    return real.EDecimal.LogN(realBase.EDecimal, MathS.Settings.DecimalPrecisionContext);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                return Ln(x) / Ln(@base);
            }

            /// <summary>ln(x) = log(e, x)</summary>
            public static Complex Ln(Complex x)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (x is Real { EDecimal: { IsNegative: false } real })
                    return real.Log(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                return Complex.Create(x.Abs().EDecimal.Log(context), x.ImaginaryPart.EDecimal.Arctan2(x.RealPart.EDecimal, context));
            }

            /// <summary>Calculates the exact value of sine of num</summary>
            public static Complex Sin(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is Real { EDecimal: var real })
                    return real.Sin(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                var (re, im) = (num.RealPart.EDecimal, num.ImaginaryPart.EDecimal);
                // We need both sinh and cosh of imaginary part.
                // To avoid multiple calls to Exp with the same value,
                // we compute them both here from a single call to Exp.
                var p = im.Exp(context);
                var q = EDecimal.One.Divide(p, context);
                var sinh = p.Subtract(q, context).Divide(2, context);
                var cosh = p.Add(q, context).Divide(2, context);
                return Complex.Create(
                    re.Sin(context).Multiply(cosh, context),
                    re.Cos(context).Multiply(sinh, context));
            }

            /// <summary>Calculates the exact value of secant of num</summary>
            public static Complex Secant(Complex num)
                => 1 / Cos(num);

            /// <summary>Calculates the exact value of cosecant of num</summary>
            public static Complex Cosecant(Complex num)
                => 1 / Sin(num);

            /// <summary>
            /// sec(x) = value
            /// 1 / cos(x) = value
            /// 1 / value = cos(x)
            /// x = arccos(1 / value)
            /// </summary>
            public static Complex Arcsecant(Complex num)
                => Arccos(1 / num);

            /// <summary>
            /// csc(x) = value
            /// 1 / sin(x) = value
            /// 1 / value = sin(x)
            /// x = arcsin(1 / value)
            /// </summary>
            public static Complex Arccosecant(Complex num)
                => Arcsin(1 / num);

            /// <summary>
            /// Defines the Signum function on complex numbers
            /// Which is z / |z|
            /// </summary>
            /// <param name="num">Number to find Signum of</param>
            /// <returns>
            /// A complex signum value for a non-zero argument,
            /// 0 otherwise
            /// </returns>
            public static Complex Signum(Complex num)
                => num.IsZero ? Integer.Zero : num / num.Abs();

            /// <summary>
            /// Complex absolute value
            /// </summary>
            public static Real Abs(Complex num)
                => num.Abs();

            /// <summary>Calculates the exact value of cosine of num</summary>
            public static Complex Cos(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is Real { EDecimal: var real })
                    return real.Cos(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                var (re, im) = (num.RealPart.EDecimal, num.ImaginaryPart.EDecimal);
                var p = im.Exp(context);
                var q = EDecimal.One.Divide(p, context);
                var sinh = p.Subtract(q, context).Divide(2, context);
                var cosh = p.Add(q, context).Divide(2, context);
                return Complex.Create(
                    re.Cos(context).Multiply(cosh, context),
                    -re.Sin(context).Multiply(sinh, context));
            }

            /// <summary>Calculates the exact value of tangent of num</summary>
            public static Complex Tan(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is Real { EDecimal: var real })
                    return real.Tan(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1

                // tan z = sin z / cos z, but to avoid unnecessary repeated trig computations, use
                //   tan z = (sin(2x) + i sinh(2y)) / (cos(2x) + cosh(2y))
                // (see Abramowitz & Stegun 4.3.57 or derive by hand), and compute trig functions here.

                // This approach does not work for |y| > ~355, because sinh(2y) and cosh(2y) overflow,
                // even though their ratio does not. In that case, divide through by cosh to get:
                //   tan z = (sin(2x) / cosh(2y) + i \tanh(2y)) / (1 + cos(2x) / cosh(2y))
                // which correctly computes the (tiny) real part and the (normal-sized) imaginary part.

                var x2 = num.RealPart.EDecimal.Multiply(2, context);
                var y2 = num.ImaginaryPart.EDecimal.Multiply(2, context);
                var p = y2.Exp(context);
                var q = EDecimal.One.Divide(p, context);
                var cosh = p.Add(q, context).Divide(2, context);
                if (num.ImaginaryPart.EDecimal.Abs().LessThanOrEquals(4))
                {
                    var sinh = p.Subtract(q, context).Divide(2, context);
                    var D = x2.Cos(context).Add(cosh, context);
                    return Complex.Create(x2.Sin(context).Divide(D, context), sinh.Divide(D, context));
                }
                else
                {
                    var D = x2.Cos(context).Divide(cosh, context).Increment();
                    return Complex.Create(
                        x2.Sin(context).Divide(cosh, context).Divide(D, context),
                        y2.Tanh(context).Divide(D, context));
                }
            }

            /// <summary>Calculates the exact value of cotangent of num</summary>
            public static Complex Cotan(Complex num)
            {
                var cotan = Integer.One / Tan(num);
                if (cotan.RealPart.EDecimal.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange)
                    && cotan.ImaginaryPart.EDecimal.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange))
                    return Integer.Zero;
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

            static (EDecimal beta, EDecimal v) ArcSinCosInner(Complex num, EContext context)
            {
                var (x, y) = (num.RealPart.EDecimal, num.ImaginaryPart.EDecimal);
                var xp1 = x.Increment();
                var xm1 = x.Decrement();
                var rho = xp1.MultiplyAndAdd(xp1, y.Multiply(y, context), context).Sqrt(context);
                var sigma = xm1.MultiplyAndAdd(xm1, y.Multiply(y, context), context).Sqrt(context);
                var alpha = rho.Add(sigma, context).Divide(2, context);
                return (rho.Subtract(sigma, context).Divide(2, context),
                    alpha.MultiplyAndSubtract(alpha, EDecimal.One, context).Sqrt(context).Add(alpha, context).Log(context).Multiply((y.IsNegative || y.IsZero) ? -1 : 1, context));
            }

            /// <summary>Calculates the exact value of arcsine of num</summary>
            public static Complex Arcsin(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is Real { EDecimal: var real } && !(real.GreaterThan(EDecimal.One) || real.LessThan(-EDecimal.One)))
                    return real.Arcsin(context);
                var (beta, v) = ArcSinCosInner(num, context);
                return Complex.Create(beta.Arcsin(context), v);
            }

            /// <summary>Calculates the exact value of arccosine of num</summary>
            public static Complex Arccos(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is Real { EDecimal: var real } && !(real.GreaterThan(EDecimal.One) || real.LessThan(-EDecimal.One)))
                    return real.Acos(context);
                var (beta, v) = ArcSinCosInner(num, context);
                return Complex.Create(beta.Acos(context), -v);
            }

            /// <summary>Calculates the exact value of arctangent of num</summary>
            public static Complex Arctan(Complex num)
            {
                var context = MathS.Settings.DecimalPrecisionContext;
                if (num is Real { EDecimal: var real })
                    return real.Arctan(context);
                // From https://source.dot.net/#System.Runtime.Numerics/System/Numerics/Complex.cs,cf15f2e5cc49cef1
                var one = Integer.One;
                var two = Integer.Create(2);
                var i = Complex.ImaginaryOne;
                return i / two * (Ln(one - i * num) - Ln(one + i * num));
            }

            /// <summary>Calculates the exact value of arccotangent of num</summary>
            public static Complex Arccotan(Complex num) => num == 0 ? MathS.DecimalConst.pi / 2 : Arctan(1 / num);

            // From https://github.com/eobermuhlner/big-math/blob/ba75e9a80f040224cfeef3c2ac06390179712443/ch.obermuhlner.math.big/src/main/java/ch/obermuhlner/math/big/BigComplexMath.java
            /// 
            /// <summary>
            /// Calculates the factorial of the specified <see cref="Complex"/>.
            /// 
            /// <para>This implementation uses
            /// <a href="https://en.wikipedia.org/wiki/Spouge%27s_approximation">Spouge's approximation</a>
            /// to calculate the factorial for non-integer values.</para>
            /// 
            /// <para>This involves calculating a series of constants that depend on the desired precision.
            /// Since this constant calculation is quite expensive (especially for higher precisions),
            /// the constants for a specific precision will be cached
            /// and subsequent calls to this method with the same precision will be much faster.</para>
            /// 
            /// <para>It is therefore recommended to do one call to this method with the standard precision of your application during the startup phase
            /// and to avoid calling it with many different precisions.</para>
            /// 
            /// <para>See: <a href="https://en.wikipedia.org/wiki/Factorial#Extension_of_factorial_to_non-integer_values_of_argument">Wikipedia: Factorial - Extension of factorial to non-integer values of argument</a></para>
            /// </summary>
            /// <param name="x">The <see cref="Complex"/></param>
            /// <returns>The factorial <see cref="Complex"/></returns>
            /// <seealso cref="InternalAMExtensions.Factorial(EDecimal, EContext)"/>
            /// <seealso cref="Gamma(Complex)"/>
            /// 
            public static Complex Factorial(Complex x)
            {
                var mathContext = MathS.Settings.DecimalPrecisionContext.Value;
                switch (x)
                {
                    case Integer { EInteger: var value } when value.Sign >= 0:
                        return value.Factorial();
                    case Real { EDecimal: var value }:
                        return value.Factorial(mathContext);
                }

                if (!mathContext.Precision.CanFitInInt32())
                    throw new CannotEvalException($"The precision of the {nameof(mathContext)} is outside the int32 range");

                using var _ = MathS.Settings.DowncastingEnabled.Set(false);
                using var __ = MathS.Settings.DecimalPrecisionContext.Set(mathContext.WithBigPrecision(mathContext.Precision << 1));
                // https://en.wikipedia.org/wiki/Spouge%27s_approximation
                int a = mathContext.Precision.ToInt32Checked() * 13 / 10;

                var constants = InternalAMExtensions.GetSpougeFactorialConstants(a);

                var negative = false;
                var factor = Complex.Create(constants[0], 0);
                for (int k = 1; k < a; k++)
                {
                    factor += constants[k] / (x + k);
                    negative = !negative;
                }

                var result = Pow(x + a, x + 0.5m) * Exp(-x - a) * factor;

                return Complex.Create(result.RealPart.EDecimal.RoundToPrecision(mathContext),
                                        result.ImaginaryPart.EDecimal.RoundToPrecision(mathContext));
            }

            /// <summary>
            /// Calculates the gamma function of the specified <see cref="Complex"/>.
            /// 
            /// <para>This implementation uses {@link #factorial(ComplexNumber, MathContext)} internally,
            /// therefore the performance implications described there apply also for this method.</para>
            /// 
            /// <para>See: <a href="https://en.wikipedia.org/wiki/Gamma_function">Wikipedia: Gamma function</a></para>
            /// </summary>
            /// <param name="x">The <see cref="Complex"/></param>
            /// <returns>The gamma <see cref="Complex"/></returns>
            /// <seealso cref="InternalAMExtensions.Gamma(EDecimal, EContext)"/>
            /// <seealso cref="Factorial(Complex)"/>
            public static Complex Gamma(Complex x) => Factorial(x - Integer.One);

            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}