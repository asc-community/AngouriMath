//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Xml.Linq;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Boolean
        {
            // Boolean values are always defined
            private protected override Entity IntrinsicCondition => True;
            
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) => this;
        }

        partial record Notf
        {
            // Logical NOT is always defined for any input
            private protected override Entity IntrinsicCondition => True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Boolean(var b) => !b,
                        _ => null
                    },
                    (@this, a) => ((Notf)@this).New(a), isExact);
        }

        partial record Andf
        {
            // Logical AND is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => left == right ? left : (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(false), _) or (_, Boolean(false)) => False,
                        (Boolean(true), _) => right,
                        (_, Boolean(true)) => left,
                        _ => null
                    },
                    (@this, a, b) => ((Andf)@this).New(a, b), isExact);
        }

        partial record Orf
        {
            // Logical OR is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(true), _) or (_, Boolean(true)) => True,
                        (Boolean(false), _) => right,
                        (_, Boolean(false)) => left,
                        _ => null
                    },
                    (@this, a, b) => ((Orf)@this).New(a, b), isExact);
        }

        partial record Xorf
        {
            // Logical XOR is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(var leftBool), Boolean(var rightBool)) => leftBool ^ rightBool,
                        (Boolean(true), _) => !right,
                        (Boolean(false), _) => right,
                        (_, Boolean(true)) => !left,
                        (_, Boolean(false)) => left,
                        _ => null
                    },
                    (@this, a, b) => ((Xorf)@this).New(a, b), isExact);
        }

        partial record Impliesf
        {
            // Logical implication is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Assumption, Conclusion,
                    (left, right) => (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(var leftBool), Boolean(var rightBool)) => !leftBool || rightBool,
                        (Boolean(false), _) => True,
                        (Boolean(true), _) => right,
                        (_, Boolean(true)) => True,
                        (_, Boolean(false)) => !left,
                        _ => null
                    },
                    (@this, a, b) => ((Impliesf)@this).New(a, b), isExact);
        }

        partial record Equalsf
        {
            // Equality comparison is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => left == right ? true
                    : left.IsConstant && right.IsConstant ? left.Evaled == right.Evaled
                    : null,
                    (@this, a, b) => ((Equalsf)@this).New(a, b), isExact);
        }

        partial record Greaterf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft > reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Greaterf)@this).New(a, b), isExact);
        }

        partial record GreaterOrEqualf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft >= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((GreaterOrEqualf)@this).New(a, b), isExact);
        }

        partial record Lessf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft < reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Lessf)@this).New(a, b), isExact);
        }

        partial record LessOrEqualf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft <= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((LessOrEqualf)@this).New(a, b), isExact);
        }

        partial record Set
        {
            partial record Inf
            {
                // Set membership is always defined for any element and set
                private protected override Entity IntrinsicCondition => True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoArguments(Element, SupSet,
                        (a, b) => (a, b) switch
                        {
                            (var el, Set set) when set.TryContains(el, out var contains) => contains,
                            _ => null
                        },
                        (@this, a, b) => ((Inf)@this).New(a, b), isExact, propagateSet: false);
            }
        }

        partial record Phif
        {
            // Euler's totient function is defined for all integers in this library.
            // For positive integers, it returns the standard φ(n) value.
            // For non-positive integers, this implementation extends the definition by returning 0.
            private protected override Entity IntrinsicCondition => Argument.In(MathS.Sets.Z);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Integer integer => integer.Phi(),
                        Number n => MathS.NaN,
                        _ => null
                    },
                    (@this, a) => ((Phif)@this).New(a), isExact);
        }
    }
}
