
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

using System.Linq;
using PeterO.Numbers;

namespace AngouriMath
{
    using Core;
    using Core.Exceptions;
    partial record Entity
    {
        public abstract partial record Number
        {
            /// <summary>
            /// Extension for <see cref="Real"/>
            /// <a href="https://en.wikipedia.org/wiki/Complex_number"/>
            /// </summary>
            public record Complex : Number
            {
                /// <summary>
                /// Constructor does not downcast automatically. Use <see cref="Create(Real, Real)"/> for automatic downcasting
                /// </summary>
                private protected Complex(Real? real, Real? imaginary) =>
                    (this.real, this.imaginary) = (real, imaginary);
                private readonly Real? real;
                private readonly Real? imaginary;
                public virtual Real RealPart => real ?? Integer.Zero;
                public Real ImaginaryPart => imaginary ?? Integer.Zero;
                public override Priority Priority =>
                    (RealPart, ImaginaryPart) switch
                    {
                        ({ IsZero: false }, { IsZero: false }) => Priority.Sum,
                        ({ IsZero: true }, Integer(1)) => Priority.Leaf,
                        _ => Priority.Mul
                    };
                public static readonly Complex ImaginaryOne = new Complex(0, 1);
                public static readonly Complex MinusImaginaryOne = new Complex(0, -1);

                protected override bool ThisIsFinite => RealPart.EDecimal.IsFinite && ImaginaryPart.EDecimal.IsFinite;
                public override bool IsExact => RealPart.IsExact && ImaginaryPart.IsExact;
                public new bool IsZero => RealPart.EDecimal.IsZero && ImaginaryPart.EDecimal.IsZero;
                public bool IsNaN => this == Real.NaN;

                public static Complex Create(Real real, Real imaginary) =>
                    Create(real.EDecimal, imaginary.EDecimal);
                public static Complex Create(EDecimal real, EDecimal imaginary)
                {
                    if (!MathS.Settings.DowncastingEnabled)
                        return new Complex(real, imaginary);
                    if (real.IsNaN() || imaginary.IsNaN())
                        return Real.NaN;
                    if (imaginary.IsFinite && imaginary.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange))
                        return Real.Create(real);
                    else
                        return new Complex(Real.Create(real), Real.Create(imaginary));
                }
                public void Deconstruct(out Real realPart, out Real imaginaryPart) =>
                    (realPart, imaginaryPart) = (RealPart, ImaginaryPart);

                public override string Stringize()
                {
                    static string RenderNum(Real number)
                    {
                        if (number == Integer.MinusOne)
                            return "-";
                        else if (number == Integer.One)
                            return "";
                        else
                            return number.ToString();
                    }
                    if (ImaginaryPart is Integer(0))
                        return RealPart.ToString();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + "i";
                    var (l, r) = ImaginaryPart is Rational and not Integer ? ("(", ")") : ("", "");
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (-ImaginaryPart, "-");
                    return RealPart.ToString() + " " + sign + " " + l + RenderNum(im) + r + "i";
                }

                public override string Latexise()
                {
                    static string RenderNum(Real number)
                    {
                        if (number == Integer.MinusOne)
                            return "-";
                        else if (number == Integer.One)
                            return "";
                        else
                            return number.Latexise();
                    }
                    if (ImaginaryPart is Integer(0))
                        return RealPart.Latexise();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + "i";
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (-ImaginaryPart, "-");
                    return RealPart.Latexise() + " " + sign + " " +
                        (im == 1 ? "" : im.Latexise(ImaginaryPart is Rational and not Integer)) + "i";
                }

                /// <summary>Returns conjugate of a complex number. Given this = a + ib, Conjugate() -> a - ib</summary>
                /// <returns>Conjugate of the number</returns>
                public Complex Conjugate() => Create(RealPart, -ImaginaryPart);

                /// <summary>The magnitude of this <see cref="Complex"/>.</summary>
                /// <returns>
                /// Returns the absolute value of this complex number, to be precise,
                /// if this = a + ib, this.Abs() -> sqrt(a^2 + b^2)
                /// </returns>
                public new virtual Real Abs() =>
                    (Real)Sqrt(RealPart.EDecimal * RealPart.EDecimal + ImaginaryPart.EDecimal * ImaginaryPart.EDecimal);

                public Real Phase() => ImaginaryPart.EDecimal.Atan2(RealPart.EDecimal, MathS.Settings.DecimalPrecisionContext);

                public static Complex CreatePolar(EDecimal magnitude, EDecimal phase)
                {
                    var context = MathS.Settings.DecimalPrecisionContext;
                    return Create(magnitude.Multiply(phase.Cos(context), context), magnitude.Multiply(phase.Sin(context), context));
                }

                /// <summary>-oo + -ooi</summary>
                public static readonly Complex NegNegInfinity =
                    new Complex(Real.NegativeInfinity, Real.NegativeInfinity);

                /// <summary>-oo + +ooi</summary>
                public static readonly Complex NegPosInfinity =
                    new Complex(Real.NegativeInfinity, Real.PositiveInfinity);

                /// <summary>+oo + -ooi</summary>
                public static readonly Complex PosNegInfinity =
                    new Complex(Real.PositiveInfinity, Real.NegativeInfinity);

                /// <summary>+oo + +ooi</summary>
                public static readonly Complex PosPosInfinity =
                    new Complex(Real.PositiveInfinity, Real.PositiveInfinity);

                /// <summary>Parses a <see cref="string"/> into <see cref="Complex"/></summary>
                /// <returns><see cref="Complex"/></returns>
                /// <exception cref="ParseException">Thrown when <paramref name="source"/> cannot be parsed.</exception>
                public static Complex Parse(string source) =>
                    TryParse(source, out var res) ? res : throw new ParseException("Cannot parse number from " + source);

                /// <summary>Tries to parse a <see cref="Complex"/> from <see cref="string"/></summary>
                /// <param name="source"></param>
                /// <param name="dst">
                /// The result will be written to this variable only if parsing was successful,
                /// if it was not, do not access this variable
                /// </param>
                /// <returns>If parsing was successful - <see langword="true"/>, <see langword="false"/> otherwise</returns>
                // TODO: parse more possible values of complex numbers
                public static bool TryParse(string source,
                    [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Complex? dst)
                {
                    dst = null;
                    if (string.IsNullOrEmpty(source))
                        return false;
                    if (Real.TryParse(source, out var real))
                    {
                        dst = real;
                        return true;
                    }
                    if (source.Last() == 'i')
                    {
                        if (source == "i")
                        {
                            dst = ImaginaryOne;
                            return true;
                        }
                        else if (source == "-i")
                        {
                            dst = MinusImaginaryOne;
                            return true;
                        }
                        else if (Real.TryParse(source.Substring(0, source.Length - 1), out var realPart))
                        {
                            dst = new Complex(0, realPart);
                            return true;
                        }
                        else
                            return false;
                    }
                    return false;
                }

                public System.Numerics.Complex ToNumerics() =>
                    new System.Numerics.Complex(RealPart.EDecimal.ToDouble(), ImaginaryPart.EDecimal.ToDouble());

                public static Complex operator +(Complex a, Complex b) => OpSum(a, b);
                public static Complex operator -(Complex a, Complex b) => OpSub(a, b);
                public static Complex operator *(Complex a, Complex b) => OpMul(a, b);
                public static Complex operator /(Complex a, Complex b) => OpDiv(a, b);
                public static Complex operator +(Complex a) => a;
                public static Complex operator -(Complex a) => OpMul(Integer.MinusOne, a);
                public static implicit operator Complex(sbyte value) => Integer.Create(value);
                public static implicit operator Complex(byte value) => Integer.Create(value);
                public static implicit operator Complex(short value) => Integer.Create(value);
                public static implicit operator Complex(ushort value) => Integer.Create(value);
                public static implicit operator Complex(int value) => Integer.Create(value);
                public static implicit operator Complex(uint value) => Integer.Create(value);
                public static implicit operator Complex(long value) => Integer.Create(value);
                public static implicit operator Complex(ulong value) => Integer.Create(value);
                public static implicit operator Complex(EInteger value) => Integer.Create(value);
                public static implicit operator Complex(ERational value) => Rational.Create(value);
                public static implicit operator Complex(EDecimal value) => Real.Create(value);
                public static implicit operator Complex(float value) => Real.Create(EDecimal.FromSingle(value));
                public static implicit operator Complex(double value) => Real.Create(EDecimal.FromDouble(value));
                public static implicit operator Complex(decimal value) => Real.Create(EDecimal.FromDecimal(value));
                public static implicit operator Complex(System.Numerics.Complex value) =>
                    Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
                public static implicit operator Complex((int re, int im) v) => Complex.Create(v.re, v.im);
                public static implicit operator Complex((float re, float im) v) => Complex.Create(v.re, v.im);
                public static implicit operator Complex((decimal re, decimal im) v) => Complex.Create(v.re, v.im);
                public static implicit operator Complex((double re, double im) v) => Complex.Create(v.re, v.im);

                public override Domain Codomain { get; protected init; } = Domain.Complex;

                public override Entity Substitute(Entity x, Entity value)
                    => this == x ? value : this;
            }
        }
    }
}