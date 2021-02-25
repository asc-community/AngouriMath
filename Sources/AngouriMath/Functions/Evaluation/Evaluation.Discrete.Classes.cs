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
                    (@this, a) => ((Notf)@this).New(a)
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Evaled is Boolean b ? b : New(Argument.InnerSimplified);
        }

        partial record Andf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left is Boolean leftBool && right is Boolean rightBool)
                {
                    res = (bool)leftBool && (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else if (left == False || right == False)
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
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Andf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Andf)@this).New(a, b)
                    );
        }

        partial record Orf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left is Boolean leftBool && right is Boolean rightBool)
                {
                    res = (bool)leftBool || (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else if (left == True || right == True)
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
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Orf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Orf)@this).New(a, b)
                    );
        }

        partial record Xorf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left is Boolean leftBool && right is Boolean rightBool)
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
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Xorf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Xorf)@this).New(a, b)
                    );
        }

        partial record Impliesf
        {
            private static bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left is Boolean leftBool && right is Boolean rightBool)
                {
                    res = !(bool)leftBool || (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else if (left == False || right == True)
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
                => ExpandOnTwoArguments(Assumption.Evaled, Conclusion.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left, right, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Impliesf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnTwoArguments(Assumption.InnerSimplified, Conclusion.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (var left, var right) when GoodResult(left.Evaled, right.Evaled, out var res) => res,
                        _ => null
                    },
                    (@this, a, b) => ((Impliesf)@this).New(a, b)
                    );
        }

        partial record Equalsf
        {
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
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
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
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
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
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
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
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnTwoArguments(Left.Evaled, Right.Evaled,
                    (a, b) => (a, b) switch
                    {
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
