
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

using AngouriMath.Functions;
using System;
using static AngouriMath.Entity.Boolean;
using AngouriMath.Core;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        partial record Boolean
        {
            protected override Entity InnerEval() => this;
            internal override Entity InnerSimplify() => this;
        }

        partial record Notf
        {
            protected override Entity InnerEval()
            {
                if (Argument.Evaled is Boolean b)
                    return !(bool)b; // there's no cost in casting
                return New(Argument.Evaled);
            }
            internal override Entity InnerSimplify() => InnerEvalWithCheck();
        }

        partial record Andf
        {
            private bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Evaled is Boolean leftBool && right.Evaled is Boolean rightBool)
                {
                    res = (bool)leftBool && (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            protected override Entity InnerEval()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Evaled, Right.Evaled);
            }
            internal override Entity InnerSimplify()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record Orf
        {
            private bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Evaled is Boolean leftBool && right.Evaled is Boolean rightBool)
                {
                    res = (bool)leftBool || (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            protected override Entity InnerEval()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Evaled, Right.Evaled);
            }
            internal override Entity InnerSimplify()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record Xorf
        {
            private bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Evaled is Boolean leftBool && right.Evaled is Boolean rightBool)
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

            protected override Entity InnerEval()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.Evaled, Right.Evaled);
            }
            internal override Entity InnerSimplify()
            {
                if (GoodResult(Left, Right, out var res))
                    return res;
                return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record Impliesf
        {
            private bool GoodResult(Entity left, Entity right, out Entity res)
            {
                if (left.Evaled is Boolean leftBool && right.Evaled is Boolean rightBool)
                {
                    res = !(bool)leftBool || (bool)rightBool; // there's no cost in casting
                    return true;
                }
                else
                {
                    res = False;
                    return false;
                }
            }

            protected override Entity InnerEval()
            {
                if (GoodResult(Assumption, Conclusion, out var res))
                    return res;
                return New(Assumption.Evaled, Conclusion.Evaled);
            }

            internal override Entity InnerSimplify()
            {
                if (GoodResult(Assumption, Conclusion, out var res))
                    return res;
                return New(Assumption.InnerSimplifyWithCheck(), Conclusion.InnerSimplifyWithCheck());
            }
        }

        partial record Equalsf
        {
            protected override Entity InnerEval()
            {
                // TODO: Is it that?
                return Left.Evaled == Right.Evaled;
            }
            internal override Entity InnerSimplify()
            {
               if (Left.Evaled is Number && Right.Evaled is Number)
                    return InnerEvalWithCheck();
               else
                    return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record Greaterf
        {
            protected override Entity InnerEval()
            {
                if (Left is Number numLeft && Right is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft > reRight;
                    else
                        return MathS.NaN;
                }
                else 
                    return New(Left.Evaled, Right.Evaled);
            }

            internal override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record GreaterOrEqualf
        {
            protected override Entity InnerEval()
            {
                if (Left is Number numLeft && Right is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft >= reRight;
                    else
                        return MathS.NaN;
                }
                else
                    return New(Left.Evaled, Right.Evaled);
            }

            internal override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record Lessf
        {
            protected override Entity InnerEval()
            {
                if (Left is Number numLeft && Right is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft < reRight;
                    else
                        return MathS.NaN;
                }
                else
                    return New(Left.Evaled, Right.Evaled);
            }

            internal override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }

        partial record LessOrEqualf
        {
            protected override Entity InnerEval()
            {
                if (Left is Number numLeft && Right is Number numRight)
                {
                    if (numLeft is Real reLeft && numRight is Real reRight)
                        return reLeft <= reRight;
                    else
                        return MathS.NaN;
                }
                else
                    return New(Left.Evaled, Right.Evaled);
            }

            internal override Entity InnerSimplify()
            {
                var res = InnerEval();
                if (res is Boolean)
                    return res;
                else
                    return New(Left.InnerSimplifyWithCheck(), Right.InnerSimplifyWithCheck());
            }
        }
    }
}
