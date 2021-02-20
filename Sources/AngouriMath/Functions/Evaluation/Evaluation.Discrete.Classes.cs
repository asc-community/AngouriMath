/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using static AngouriMath.Entity.Boolean;
using AngouriMath.Functions;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        partial record Boolean
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => this;

            /// <inheritdoc/>
            protected override Entity InnerSimplify() => this;
        }

        partial record Notf
        {

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Boolean b => !(bool)b,
                        _ => null
                    },
                    a => MathS.Negation(a)
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() => InnerEvalWithCheck();
        }

        partial record Andf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Unpack1Eval() is Boolean leftBool && right.Unpack1Eval() is Boolean rightBool)
                {
                    res = (bool)leftBool && (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else if (left.Unpack1Eval() == False || right.Unpack1Eval() == False)
                {
                    res = False;
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
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.Conjunction(a.Unpack1Eval(), b.Unpack1Eval())
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.Conjunction(a.Unpack1Simplify(), b.Unpack1Simplify())
                    );
        }

        partial record Orf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Unpack1Eval() is Boolean leftBool && right.Unpack1Eval() is Boolean rightBool)
                {
                    res = (bool)leftBool || (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else if (left.Unpack1Eval() == True || right.Unpack1Eval() == True)
                {
                    res = True;
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
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.Disjunction(a.Unpack1Eval(), b.Unpack1Eval())
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.Disjunction(a.Unpack1Simplify(), b.Unpack1Simplify())
                    );
        }

        partial record Xorf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Unpack1Eval() is Boolean leftBool && right.Unpack1Eval() is Boolean rightBool)
                {
                    res = (bool)leftBool ^ (bool)rightBool; // there's no cost in casting
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
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.ExclusiveDisjunction(a.Unpack1Eval(), b.Unpack1Eval())
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.ExclusiveDisjunction(a.Unpack1Simplify(), b.Unpack1Simplify())
                    );
        }

        partial record Impliesf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Unpack1Eval() is Boolean leftBool && right.Unpack1Eval() is Boolean rightBool)
                {
                    res = !(bool)leftBool || (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else if (left.Unpack1Eval() == False || right.Unpack1Eval() == True)
                {
                    res = True;
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
                => ExpandOnTwoArguments(Assumption, Conclusion,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.Implication(a.Unpack1Eval(), b.Unpack1Eval())
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Assumption, Conclusion,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (a, b) => MathS.Implication(a.Unpack1Simplify(), b.Unpack1Simplify())
                    );
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when left == right => true,
                        (var left, var right) when left.IsConstant && right.IsConstant => left == right,
                        _ => null
                    },
                    (a, b) => a.Xor(b)
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b : 
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when left == right => true,
                        (var left, var right) when left.IsConstant && right.IsConstant => left == right,
                        _ => null
                    },
                    (a, b) => a.Xor(b)
                    );
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft > reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (a, b) => MathS.GreaterThan(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b : 
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (a, b) => MathS.GreaterThan(a, b)
                    );
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft >= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (a, b) => MathS.GreaterThan(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() 
                => Evaled is Boolean b ? b :
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (a, b) => MathS.GreaterOrEqualThan(a, b)
                    );
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft < reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (a, b) => MathS.GreaterThan(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b :
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (a, b) => MathS.LessThan(a, b)
                    );
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft <= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (a, b) => MathS.GreaterThan(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b :
                ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        _ => null
                    },
                    (a, b) => MathS.LessOrEqualThan(a, b)
                    );
        }

        partial record Set
        {
            partial record Inf
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => ExpandOnTwoArguments(Element.Evaled, SupSet.Evaled,
                        (a, b) => (a, b) switch
                        {

                        }
                {
                    if (SupSet.Unpack1Eval() is not Set set)
                        return New(Element.Unpack1Eval(), SupSet.Unpack1Eval());
                    if (!set.TryContains(Element, out var contains))
                        return New(Element.Unpack1Eval(), SupSet.Unpack1Eval());
                    return contains;
                }

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                {
                    if (SupSet.Unpack1Simplify() is not Set set)
                        return New(Element.Unpack1Simplify(), SupSet.Unpack1Simplify());
                    if (!set.TryContains(Element, out var contains))
                        return New(Element.Unpack1Simplify(), SupSet.Unpack1Simplify());
                    return contains;
                }
            }
        }

        partial record Phif
        {               
            private Entity InnerCompute(Entity entity)
            {
                if (entity is not Integer integer)
                    return this;
                else
                    return integer.Phi();
            }

            /// <inheritdoc/>
            protected override Entity InnerEval() => InnerCompute(Argument.Unpack1Eval());

            /// <inheritdoc/>
            protected override Entity InnerSimplify() => InnerCompute(Argument.Unpack1Simplify());
        }
    }
}
