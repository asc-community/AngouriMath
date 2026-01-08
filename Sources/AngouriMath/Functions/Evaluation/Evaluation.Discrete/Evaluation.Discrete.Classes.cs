//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity.Boolean;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Boolean
        {
            // Boolean values are always defined
            private protected override Entity IntrinsicCondition => True;
            
            /// <inheritdoc/>
            protected override Entity InnerEval() => this;

            /// <inheritdoc/>
            protected override Entity InnerSimplify() => this;
        }

        partial record Notf
        {
            // Logical NOT is always defined for any input
            private protected override Entity IntrinsicCondition => True;

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Boolean b => !(bool)b,
                        _ => null
                    },
                    (@this, a) => ((Notf)@this).New(a)
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b : New(Argument.InnerSimplified);
        }

        partial record Andf
        {
            // Logical AND is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            
            private static bool GoodResult(Entity left, Entity right, Entity leftEvaled, Entity rightEvaled, out Entity res)
            {
                if (leftEvaled is Boolean leftBool && rightEvaled is Boolean rightBool)
                {
                    res = (bool)leftBool && (bool)rightBool;
                    return true;
                }
                else if (leftEvaled == False || rightEvaled == False)
                {
                    res = False;
                    return true;
                }
                else if (leftEvaled == True)
                {
                    res = right;
                    return true;
                }
                else if (rightEvaled == True)
                {
                    res = left;
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Andf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Andf)@this).New(a, b)
                    );
        }

        partial record Orf
        {
            // Logical OR is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            
            private static bool GoodResult(Entity left, Entity right, Entity leftEvaled, Entity rightEvaled, out Entity res)
            {
                if (leftEvaled is Boolean leftBool && rightEvaled is Boolean rightBool)
                {
                    res = (bool)leftBool || (bool)rightBool;
                    return true;
                }
                else if (leftEvaled == True || rightEvaled == True)
                {
                    res = True;
                    return true;
                }
                else if (leftEvaled == False)
                {
                    res = right;
                    return true;
                }
                else if (rightEvaled == False)
                {
                    res = left;
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Orf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Orf)@this).New(a, b)
                    );
        }

        partial record Xorf
        {
            // Logical XOR is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            
            private static bool GoodResult(Entity left, Entity right, Entity leftEvaled, Entity rightEvaled, out Entity res)
            {
                if (leftEvaled is Boolean leftBool && rightEvaled is Boolean rightBool)
                {
                    res = (bool)leftBool ^ (bool)rightBool;
                    return true;
                }
                else if (leftEvaled is Boolean leftBoolOnly)
                {
                    res = leftBoolOnly ? !right : right;
                    return true;
                }
                else if (rightEvaled is Boolean rightBoolOnly)
                {
                    res = rightBoolOnly ? !left : left;
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Xorf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Xorf)@this).New(a, b)
                    );
        }

        partial record Impliesf
        {
            // Logical implication is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            
            private static bool GoodResult(Entity left, Entity right, Entity leftEvaled, Entity rightEvaled, out Entity res)
            {
                if (leftEvaled is Boolean leftBool && rightEvaled is Boolean rightBool)
                {
                    res = !(bool)leftBool || (bool)rightBool;
                    return true;
                }
                else if (leftEvaled == False || rightEvaled == True)
                {
                    res = True;
                    return true;
                }
                else if (leftEvaled == True)
                {
                    res = right;
                    return true;
                }
                else if (rightEvaled == False)
                {
                    res = !left;
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Assumption.Evaled, Conclusion.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Impliesf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Assumption.InnerSimplified, Conclusion.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Impliesf)@this).New(a, b)
                    );
        }

        partial record Equalsf
        {
            // Equality comparison is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => (Left.Evaled, Right.Evaled) switch
                {
                    (var left, var right) when left == right => true,
                    (var left, var right) when left.IsConstant && right.IsConstant => left == right,
                    (var left, var right) => MathS.Equality(left, right)
                };
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b : MathS.Equality(Left.InnerSimplified, Right.InnerSimplified);
        }

        partial record Greaterf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real nan, _) when nan == MathS.NaN => MathS.NaN,
                        (_, Real nan) when nan == MathS.NaN => MathS.NaN,
                        (Real reLeft, Real reRight) => reLeft > reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Greaterf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b : 
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (@this, a, b) => ((Greaterf)@this).New(a, b)
                    );
        }

        partial record GreaterOrEqualf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real nan, _) when nan == MathS.NaN => MathS.NaN,
                        (_, Real nan) when nan == MathS.NaN => MathS.NaN,
                        (Real reLeft, Real reRight) => reLeft >= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((GreaterOrEqualf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() 
                => Evaled is Boolean b ? b :
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (@this, a, b) => ((GreaterOrEqualf)@this).New(a, b)
                    );
        }

        partial record Lessf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real nan, _) when nan == MathS.NaN => MathS.NaN,
                        (_, Real nan) when nan == MathS.NaN => MathS.NaN,
                        (Real reLeft, Real reRight) => reLeft < reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Lessf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b :
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (@this, a, b) => ((Lessf)@this).New(a, b)
                    );
        }

        partial record LessOrEqualf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real nan, _) when nan == MathS.NaN => MathS.NaN,
                        (_, Real nan) when nan == MathS.NaN => MathS.NaN,
                        (Real reLeft, Real reRight) => reLeft <= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((LessOrEqualf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b :
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (@this, a, b) => ((LessOrEqualf)@this).New(a, b)
                    );
        }

        partial record Set
        {
            partial record Inf
            {
                // Set membership is always defined for any element and set
                private protected override Entity IntrinsicCondition => True;
                
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => ExpandOnTwoArguments(Element.Evaled, SupSet.Evaled,
                        (a, b) => (a, b) switch
                        {
                            (var el, Set set) when set.TryContains(el, out var contains) => contains,
                            _ => null
                        },
                        (@this, a, b) => ((Inf)@this).New(a, b)
                        );

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => ExpandOnTwoArguments(Element.InnerSimplified, SupSet.InnerSimplified,
                        (a, b) => (a, b) switch
                        {
                            (var el, Set set) when set.TryContains(el, out var contains) => contains,
                            _ => null
                        },
                        (@this, a, b) => ((Inf)@this).New(a, b)
                        );
            }
        }

        partial record Phif
        {
            // Euler's totient function is defined for all integers in this library.
            // For positive integers, it returns the standard φ(n) value.
            // For non-positive integers, this implementation extends the definition by returning 0.
            private protected override Entity IntrinsicCondition => True;
            
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Integer integer => integer.Phi(),
                        _ => null
                    },
                    (@this, a) => ((Phif)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        Integer integer => integer.Phi(),
                        _ => null
                    },
                    (@this, a) => ((Phif)@this).New(a)
                    );
        }
    }
}
