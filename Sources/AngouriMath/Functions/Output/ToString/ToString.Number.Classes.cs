//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
                private protected override string StringizeNode()
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
                    // A fraction next to the i has to be multiplied by it explicitly. Written
                    // as 1/2i the parser reads 1/(2i), which is the negation of what was
                    // printed, and bracketing it as (1/2)i does not help -- a bracket followed
                    // by a name is read as a power, so 2 + (3/4)i came back as 2 + (3/4)^i.
                    // Both are silent: the misreading is a valid expression, so nothing
                    // complains, and every root of a trigonometric equation prints this way.
                    var times = ImaginaryPart is Rational and not Integer ? " * " : "";
                    if (ImaginaryPart is Integer(0))
                        return RealPart.Stringize();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + times + "i";
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (NegatedAsItIs(ImaginaryPart), "-");
                    return RealPart.Stringize() + " " + sign + " " + RenderNum(im) + times + "i";
                }

                /// <summary>
                /// <paramref name="number"/> negated, and nothing else. Negating makes a new number,
                /// which is downcast under the setting in force when it is made, and a number made
                /// with downcasting off could come back different: <c>0.333...</c> to forty digits
                /// as the rational 1/3, printed without the product sign a fraction needs, so that
                /// <c>1 - 0.333...i</c> read back as <c>1 + i/3</c>; and <c>9.5e-114</c> as 0.
                /// https://github.com/asc-community/AngouriMath/issues/1610
                /// </summary>
                internal static Real NegatedAsItIs(Real number)
                {
                    using var _ = MathS.Settings.DowncastingEnabled.Set(false);
                    return -number;
                }
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Real
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                private protected override string StringizeNode() => this switch
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
                private protected override string StringizeNode() => ERational.ToString();
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Integer
            {
                /// <inheritdoc/>
                private protected override string StringizeNode() => EInteger.ToString();
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }
        }
    }
}
