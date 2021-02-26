/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
            /// <summary>
            /// Extension for <see cref="Real"/>
            /// <a href="https://en.wikipedia.org/wiki/Complex_number"/>
            /// </summary>
#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Complex
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                public override string Stringize()
                {
                    static string RenderNum(Real number)
                    {
                        if (number == Integer.MinusOne)
                            return "-";
                        else if (number == Integer.One)
                            return "";
                        else
                            return number.Stringize();
                    }
                    if (ImaginaryPart is Integer(0))
                        return RealPart.Stringize();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + "i";
                    var (l, r) = ImaginaryPart is Rational and not Integer ? ("(", ")") : ("", "");
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (-ImaginaryPart, "-");
                    return RealPart.Stringize() + " " + sign + " " + l + RenderNum(im) + r + "i";
                }
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Real
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                public override string Stringize() => this switch
                {
                    { IsFinite: true } => EDecimal.ToString(),
                    { IsNaN: true } => "NaN",
                    { IsNegative: true } => "-oo",
                    _ => "+oo",
                };
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Rational
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                public override string Stringize() => ERational.ToString();
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Integer
            {
                /// <inheritdoc/>
                public override string Stringize() => EInteger.ToString();
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }
        }
    }
}
