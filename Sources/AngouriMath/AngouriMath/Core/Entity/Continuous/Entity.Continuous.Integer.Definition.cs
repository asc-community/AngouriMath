//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
            /// <summary>Use <see cref="Create(EInteger)"/> instead of the constructor for consistency with
            /// <see cref="Rational"/>, <see cref="Real"/> and <see cref="Complex"/>.</summary>
            public sealed partial record Integer : Rational, System.IComparable<Integer>
            {
                private Integer(EInteger value) : base(value) => EInteger = value;

                internal override Priority Priority => IsNegative ? Priority.Sum : Priority.Leaf;

                /// <summary>
                /// Represents PeterO number in EInteger
                /// </summary>
                public EInteger EInteger { get; }

                /// <summary>
                /// A zero, you can use it to avoid allocations
                /// </summary>
                [ConstantField] public static readonly Integer Zero = new Integer(EInteger.Zero);

                /// <summary>
                /// A one, you can use it to avoid allocations
                /// </summary>
                [ConstantField] public static readonly Integer One = new Integer(EInteger.One);

                /// <summary>
                /// A minus one, you can use it to avoid allocations
                /// </summary>
                [ConstantField] public static readonly Integer MinusOne = new Integer(-EInteger.One);


                /// <summary>
                /// Creates an instance of Integer
                /// </summary>
                public static Integer Create(int value)
                {
                    if (value == 0)
                        return Zero;
                    if (value == 1)
                        return One;
                    if (value == -1)
                        return MinusOne;
                    return new Integer(value);
                }

                /// <summary>
                /// Creates an instance of Integer
                /// </summary>
                public static Integer Create(EInteger value)
                {
                    if (value.IsZero)
                        return Zero;
                    if (value.Equals(EInteger.One))
                        return One;
                    return new Integer(value);
                }

                /// <summary>
                /// Computes Euler phi function
                /// <a href="https://en.wikipedia.org/wiki/Euler%27s_totient_function"/>
                /// </summary>
                /// If integer x is non-positive, the result will be 0
                public Integer Phi() => EInteger.Phi();

                /// <summary>
                /// Factorization of integer
                /// </summary>
                public IEnumerable<(Integer prime, Integer power)> Factorize() =>
                    EInteger.Factorize().Select(x => ((Integer) x.prime, (Integer) x.power));

                /// <summary>
                /// Count of all divisors of an integer
                /// </summary>
                public Integer CountDivisors() => EInteger.CountDivisors();

                /// <summary>
                /// Detemine whether integer is prime or not.
                /// </summary>
                public bool IsPrime => CountDivisors() == 2;

                /// <summary>
                /// Deconstructs as record
                /// </summary>
                public void Deconstruct(out int? value) =>
                    value = EInteger.CanFitInInt32() ? EInteger.ToInt32Unchecked() : new int?();

                /// <inheritdoc/>
                public override Real Abs() => Create(EInteger.Abs());

                internal static bool TryParse(string s,
                    [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Integer? dst)
                {
                    try
                    {
                        dst = EInteger.FromString(s);
                        return true;
                    }
                    catch
                    {
                        dst = null;
                        return false;
                    }
                }

                /// <summary>
                /// Performs integer division of the
                /// number by the given number
                /// </summary>
                public Integer IntegerDiv(Integer a) => EInteger.Divide(a.EInteger);

#pragma warning disable CS1591
                public static bool operator >(Integer a, Integer b) => a.EInteger.CompareTo(b.EInteger) > 0;
                public static bool operator >=(Integer a, Integer b) => a.EInteger.CompareTo(b.EInteger) >= 0;
                public static bool operator <(Integer a, Integer b) => a.EInteger.CompareTo(b.EInteger) < 0;
                public static bool operator <=(Integer a, Integer b) => a.EInteger.CompareTo(b.EInteger) <= 0;
                public int CompareTo(Integer? other) => other is null ? throw new System.ArgumentNullException() : EInteger.CompareTo(other.EInteger);
                public static Integer operator +(Integer a, Integer b) => OpSum(a, b);
                public static Integer operator -(Integer a, Integer b) => OpSub(a, b);
                public static Integer operator *(Integer a, Integer b) => OpMul(a, b);
                public static Real operator /(Integer a, Integer b) => (Real)OpDiv(a, b);
                public static Integer operator %(Integer a, Integer b) => a.EInteger.Mod(b.EInteger);
                public static Integer operator +(Integer a) => a;
                public static Integer operator -(Integer a) => OpMul(MinusOne, a);
                public static implicit operator Integer(sbyte value) => Create(value);
                public static implicit operator Integer(byte value) => Create(value);
                public static implicit operator Integer(short value) => Create(value);
                public static implicit operator Integer(ushort value) => Create(value);
                public static implicit operator Integer(int value) => Create(value);
                public static implicit operator Integer(uint value) => Create(value);
                public static implicit operator Integer(long value) => Create(value);
                public static implicit operator Integer(ulong value) => Create(value);
                public static implicit operator Integer(EInteger value) => Create(value);
#pragma warning restore CS1591

            }
        }
    }
}
