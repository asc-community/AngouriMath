
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

using AngouriMath.Core.Sets;
using AngouriMath.Functions;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        partial record Set
        {
            partial record FiniteSet
            {
                protected override Entity InnerEval()
                    => Apply(el => el.Evaled);

                internal override Entity InnerSimplify()
                    => Apply(el => el.InnerSimplified);
            }

            partial record Interval
            {
                private Entity IfEqualEndsThenCollapse()
                    => Left.Evaled == Right.Evaled ? new FiniteSet(Simplificator.PickSimplest(Left, Right)) : this;

                protected override Entity InnerEval()
                    => New(Left.Evaled, Right.Evaled).IfEqualEndsThenCollapse();

                internal override Entity InnerSimplify()
                    => New(Left.InnerSimplified, Right.InnerSimplified);
            }

            partial record ConditionalSet
            {
                protected override Entity InnerEval()
                    => Simplificator.PickSimplest(New(Var, Predicate.Evaled), InnerSimplifyWithCheck());

                internal override Entity InnerSimplify()
                {
                    if (!Predicate.EvaluableBoolean)
                        return New(Var, Predicate.InnerSimplified);
                    // so it's either U or {} if the statement is always true or false respectively
                    return Predicate.EvalBoolean() ? Codomain : Set.Empty;
                }
            }

            partial record SpecialSet
            {
                protected override Entity InnerEval()
                    => this;

                internal override Entity InnerSimplify()
                    => this;
            }

            partial record Unionf
            {
                protected override Entity InnerEval()
                    => InnerSimplified;

                internal override Entity InnerSimplify()
                    => (Left.InnerSimplified, Right.InnerSimplified) switch
                    {
                        (FiniteSet setLeft, Set setRight) => SetOperators.UniteFiniteSetAndSet(setLeft, setRight),
                        (Interval intLeft, Interval intRight) => SetOperators.UniteIntervalAndInterval(intLeft, intRight),
                        // TODO
                        (var left, var right) => New(left, right)
                    };
            }

            partial record Intersectionf
            {
                protected override Entity InnerEval()
                    => InnerSimplified;

                internal override Entity InnerSimplify()
                    => (Left.InnerSimplified, Right.InnerSimplified) switch
                    {
                        (FiniteSet setLeft, Set setRight) => SetOperators.IntersectFiniteSetAndSet(setLeft, setRight),
                        (Interval intLeft, Interval intRight) => SetOperators.IntersectIntervalAndInterval(intLeft, intRight),
                        // TODO
                        (var left, var right) => New(left, right)
                    };
            }

            partial record SetMinusf
            {
                protected override Entity InnerEval()
                    => InnerSimplified;

                internal override Entity InnerSimplify()
                    => (Left.InnerSimplified, Right.InnerSimplified) switch
                    {
                        (FiniteSet setLeft, FiniteSet setRight) => FiniteSet.Subtract(setLeft, setRight),
                        // TODO
                        (var left, var right) => New(left, right)
                    };
            }
        }
    }
}
