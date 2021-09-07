using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AngouriMath
{
    #if NET6_0
    partial record Entity
    {
        partial record Number
        {
            partial record Real : INumber<Real>
            {
                public static Real Abs(Real value) => value.Abs();

                public static Real Clamp(Real value, Real min, Real max)
                    => value > max ? max : (value < min ? min : value);

                public static Real Create<TOther>(TOther value) where TOther : INumber<TOther>
                {
                    throw new NotImplementedException();
                }

                public static Real CreateSaturating<TOther>(TOther value) where TOther : INumber<TOther>
                {
                    throw new NotImplementedException();
                }

                public static Real CreateTruncating<TOther>(TOther value) where TOther : INumber<TOther>
                {
                    throw new NotImplementedException();
                }

                public static (Real Quotient, Real Remainder) DivRem(Real left, Real right)
                {
                    throw new NotImplementedException();
                }

                public static Real Max(Real x, Real y)
                {
                    throw new NotImplementedException();
                }

                public static Real Min(Real x, Real y)
                {
                    throw new NotImplementedException();
                }

                public static Real Parse(string s, NumberStyles style, IFormatProvider? provider)
                {
                    throw new NotImplementedException();
                }

                public static Real Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
                {
                    throw new NotImplementedException();
                }

                public static Real Sign(Real value)
                {
                    throw new NotImplementedException();
                }

                public static bool TryCreate<TOther>(TOther value, out Real result) where TOther : INumber<TOther>
                {
                    throw new NotImplementedException();
                }

                public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out Real result)
                {
                    throw new NotImplementedException();
                }

                public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Real result)
                {
                    throw new NotImplementedException();
                }

                public static Real One => throw new NotImplementedException();

                public static Real Zero => throw new NotImplementedException();

                Real IAdditiveIdentity<Real, Real>.AdditiveIdentity => throw new NotImplementedException();

                public int CompareTo(object? obj)
                {
                    throw new NotImplementedException();
                }

                public static Real operator --(Real value)
                {
                    throw new NotImplementedException();
                }

                public static Real operator ++(Real value)
                {
                    throw new NotImplementedException();
                }

                Real IMultiplicativeIdentity<Real, Real>.MultiplicativeIdentity => throw new NotImplementedException();

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    throw new NotImplementedException();
                }

                public string ToString(string? format, IFormatProvider? formatProvider)
                {
                    throw new NotImplementedException();
                }

                public static Real Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
                {
                    throw new NotImplementedException();
                }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Real result)
                {
                    throw new NotImplementedException();
                }

                static Real IParseable<Real>.Parse(string s, IFormatProvider? provider)
                {
                    throw new NotImplementedException();
                }

                public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Real result)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
    #endif
}
