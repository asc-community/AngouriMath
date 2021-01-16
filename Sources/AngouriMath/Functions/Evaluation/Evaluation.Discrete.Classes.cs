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
            {
                if (Argument.Unpack1Eval() is Boolean b)
                    return !(bool)b; // there's no cost in casting
                return New(Argument.Unpack1Eval());
            }
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
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
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
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
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
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
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
            {
                if (GoodResult(Assumption, Conclusion, out var res))
                    return res;
                return New(Assumption.Unpack1Eval(), Conclusion.Unpack1Eval());
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                if (GoodResult(Assumption, Conclusion, out var res))
                    return res;
                return New(Assumption.Unpack1Simplify(), Conclusion.Unpack1Simplify());
            }
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                if (!Left.IsConstant || !Right.IsConstant)
                    return New(Left.Unpack1Eval(), Right.Unpack1Eval());
                return Left.Unpack1Eval() == Right.Unpack1Eval();
            }
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
               if (Left == Right)
                    return true;
               if (Left.Unpack1Eval() is Number && Right.Unpack1Eval() is Number)
                    return InnerEvalWithCheck();
               else
                    return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                if (Left.Unpack1Eval() is Number numLeft && Right.Unpack1Eval() is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft > reRight;
                    else
                        return MathS.NaN;
                }
                else 
                    return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                if (Left.Unpack1Eval() is Number numLeft && Right.Unpack1Eval() is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft >= reRight;
                    else
                        return MathS.NaN;
                }
                else
                    return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                if (Left.Unpack1Eval() is Number numLeft && Right.Unpack1Eval() is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft < reRight;
                    else
                        return MathS.NaN;
                }
                else
                    return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                if (Left.Unpack1Eval() is Number numLeft && Right.Unpack1Eval() is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft <= reRight;
                    else
                        return MathS.NaN;
                }
                else
                    return New(Left.Unpack1Eval(), Right.Unpack1Eval());
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.Unpack1Simplify(), Right.Unpack1Simplify());
            }
        }

        partial record Set
        {
            partial record Inf
            {
                /// <inheritdoc/>
            protected override Entity InnerEval()
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
