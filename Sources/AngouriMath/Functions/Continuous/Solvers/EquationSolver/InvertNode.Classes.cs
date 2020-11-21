/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using static AngouriMath.Entity.Number;
using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        partial record Number
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw new AngouriBugException("This function must contain " + nameof(x));
        }

        partial record Variable
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) => new[] { value };
        }

        partial record Tensor : Entity
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) => new[] { this };
        }

        // Each function and operator processing
        partial record Sumf
        {
            // x + a = value => x = value - a
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Augend.ContainsNode(x) ? Augend.Invert(value - Addend, x) : Addend.Invert(value - Augend, x);
        }

        partial record Minusf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Subtrahend.ContainsNode(x)
                // x - a = value => x = value + a
                ? Subtrahend.Invert(value + Minuend, x)
                // a - x = value => x = a - value
                : Minuend.Invert(value - Subtrahend, x);
        }

        partial record Mulf
        {
            // x * a = value => x = value / a
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Multiplier.ContainsNode(x)
                ? Multiplier.Invert(value / Multiplicand, x)
                : Multiplicand.Invert(value / Multiplier, x);
        }

        partial record Divf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Dividend.ContainsNode(x)
                // x / a = value => x = a * value
                ? Dividend.Invert(value * Divisor, x)
                // a / x = value => x = a / value
                : Divisor.Invert(Dividend / value, x);
        }

        partial record Powf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
            {
                if (Base.ContainsNode(x))
                {
                    if (Exponent is Integer { EInteger: var pow })
                        return Number.GetAllRootsOf1(pow).SelectMany(root => Base.Invert(root * MathS.Pow(value, 1 / Exponent), x));
                    else
                        return Base.Invert(MathS.Pow(value, 1 / Exponent), x);
                }  
                // a ^ x = value => x = log(a, value)
                // TODO: determine how we should return periodic roots as
                // (e ^ x) ^ a != (e ^ a) ^ x for logs
                return Exponent.Invert(MathS.Log(Base, value), x);
            }
        }

        // TODO: Consider case when sin(sin(x)) where double-mention of n occures
        partial record Sinf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                // sin(x) = value => x = arcsin(value) + 2pi * n
                Argument.Invert(MathS.Arcsin(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x)
                // sin(x) = value => x = pi - arcsin(value) + 2pi * n
                .Concat(Argument.Invert(MathS.pi - MathS.Arcsin(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x));
        }

        partial record Cosf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                // cos(x) = value => x = arccos(value) + 2pi * n
                Argument.Invert(MathS.Arccos(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x)
                // cos(x) = value => x = -arccos(value) + 2pi * n
                .Concat(Argument.Invert(-MathS.Arccos(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x));
        }

        partial record Secantf
        {
            // sec(f(x)) = value
            // 1 / cos(f(x)) = value
            // 1 / value = cos(f(x))
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Argument.Cos().Invert(1 / value, x);
        }

        partial record Cosecantf
        {
            // csc(f(x)) = value
            // 1 / sin(f(x)) = value
            // 1 / value = sin(f(x))
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Argument.Sin().Invert(1 / value, x);
        }

        partial record Tanf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                // tan(x) = value => x = arctan(value) + pi * n
                Argument.Invert(MathS.Arctan(value) + MathS.pi * Variable.CreateUnique(this + value, "n"), x);
        }

        partial record Cotanf
        {
            // cotan(x) = value => x = arccotan(value) + pi * n
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Argument.Invert(MathS.Arccotan(value) + MathS.pi * Variable.CreateUnique(this + value, "n"), x);
        }

        partial record Logf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Base.ContainsNode(x)
                // log_x(a) = value => a = x ^ value => x = a ^ (1 / value)
                ? Base.Invert(MathS.Pow(Antilogarithm, 1 / value), x)
                // log_a(x) = value => x = a ^ value
                : Antilogarithm.Invert(MathS.Pow(Base, value), x);
        }

        partial record Arcsinf
        {
            [ConstantField] private static readonly Complex From = Complex.Create(-MathS.DecimalConst.pi / 2, Real.NegativeInfinity.EDecimal);
            [ConstantField] private static readonly Complex To = Complex.Create(MathS.DecimalConst.pi / 2, Real.PositiveInfinity.EDecimal);
            // arcsin(x) = value => x = sin(value)
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Sin(value), x) : Enumerable.Empty<Entity>();
        }

        partial record Arccosf
        {
            [ConstantField] private static readonly Complex From = Complex.Create(0, Real.NegativeInfinity.EDecimal);
            [ConstantField] private static readonly Complex To = Complex.Create(MathS.DecimalConst.pi, Real.PositiveInfinity.EDecimal);
            // arccos(x) = value => x = cos(value)
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Cos(value), x) : Enumerable.Empty<Entity>();
        }
        
        partial record Arctanf
        {
            [ConstantField] private static readonly Complex From = Complex.Create(-MathS.DecimalConst.pi / 2, Real.NegativeInfinity.EDecimal);
            [ConstantField] private static readonly Complex To = Complex.Create(MathS.DecimalConst.pi / 2, Real.PositiveInfinity.EDecimal);
            // arctan(x) = value => x = tan(value)
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Tan(value), x) : Enumerable.Empty<Entity>();
        }

        partial record Arccotanf
        {
            // TODO: Range should exclude Re(z) = 0
            [ConstantField] private static readonly Complex From = Complex.Create(-MathS.DecimalConst.pi / 2, Real.NegativeInfinity.EDecimal);
            [ConstantField] private static readonly Complex To = Complex.Create(MathS.DecimalConst.pi / 2, Real.PositiveInfinity.EDecimal);
            // arccotan(x) = value => x = cotan(value)
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Cotan(value), x) : Enumerable.Empty<Entity>();
        }

        partial record Arcsecantf
        {
            // arcsec(f(x)) = value
            // f(x) = sec(value)
            // x = f(x).Invert(sec(value))
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Argument.Invert(value.Sec(), x);
        }

        partial record Arccosecantf
        {
            // arccsc(f(x)) = value
            // f(x) = csc(value)
            // x = f(x).Invert(csc(value))
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Argument.Invert(value.Cosec(), x);
        }

        partial record Factorialf
        {
            // TODO: Inverse of factorial not implemented yet
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Enumerable.Empty<Entity>();
        }

        partial record Derivativef
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Expression.ContainsNode(x)
                ? Expression.Invert(MathS.Integral(value, Var, Iterations), x)
                : Enumerable.Empty<Entity>();
        }

        partial record Integralf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Expression.ContainsNode(x)
                ? Expression.Invert(MathS.Derivative(value, Var, Iterations), x)
                : Enumerable.Empty<Entity>();
        }

        partial record Limitf
        {
            // TODO: We can't just do a limit on the inverse function: https://math.stackexchange.com/q/3397326/627798
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x) =>
                Enumerable.Empty<Entity>();
        }

        partial record Signumf
        {
            // signum(f(x)) = value
            // f(x) = value * pr
            // x = f(x).InvertNode(value * pr, x)
            // TODO: we need to make a piecewise for the case when signum(x) = n and |n| != 1
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => Argument.Invert(value * Variable.CreateUnique(Argument + value, "r"), x);
        }

        partial record Absf
        {
            // abs(f(x)) = value
            // f(x) = value * e ^ (i * n)
            // x = f(x).InvertNode(value * e ^ (i * n), x)
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => Argument.Invert(value * MathS.e.Pow(MathS.i * Variable.CreateUnique(value + Argument, "r")), x);
        }

        partial record Boolean
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw new AngouriBugException("This function must contain " + nameof(x));
        }

        partial record Notf
        {
            // !f(x) = value
            // f(x) = !value
            // x = f(x).InvertNode(!value, x)
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => Argument.Invert(!value, x);
        }

        partial record Andf
        {
            // f(x) & b = value
            // f(x) = Piecewise...
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Orf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Xorf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Impliesf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Equalsf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Greaterf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record GreaterOrEqualf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Lessf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record LessOrEqualf
        {
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => throw FutureReleaseException.Raised("Requires Intervals to be Entities", "1.2");
        }

        partial record Set
        {
            partial record FiniteSet
            {
                // set{,,,} = value
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                    => this == x ? new[] { value } : Enumerable.Empty<Entity>();
            }

            partial record Interval
            {
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                {
                    if (this == x)
                        return new[] { value };
                    if (x is FiniteSet fs && fs.Count == 1)
                        throw FutureReleaseException.Raised("Piecewise required", "1.2");
                    return Enumerable.Empty<Entity>();
                }
            }

            partial record ConditionalSet
            {
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                    => this == x ? new[] { value } : Enumerable.Empty<Entity>();
            }

            partial record SpecialSet
            {
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                    => new[] { this };
            }

            partial record Unionf
            {
                // f(x) \/ A = value
                // f(x) = (value \ A) \/ PowerSet(A)
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                {
                    var (withX, withoutX) = Left.ContainsNode(x) ? (Left, Right) : (Right, Left);
                    if (value is FiniteSet valueFiniteSet && withoutX is FiniteSet A)
                    {
                        if (!A.TryIsSubsetOf(valueFiniteSet, out var isSub))
                            return withX.InvertNode(value.SetSubtract(withoutX), x);
                        if (!isSub)
                            return Empty;
                        var sub = FiniteSet.Subtract(valueFiniteSet, A);
                        var answers = new List<Entity>();
                        foreach (var ans in A.GetPowerSet())
                        {
                            if (ans is not FiniteSet finiteSet)
                                throw new AngouriBugException("PowerSet must return a set of sets");
                            answers.AddRange(withX.InvertNode(FiniteSet.Unite(sub, finiteSet), x));
                        }
                        return answers;
                    }
                    return withX.InvertNode(value.SetSubtract(withoutX), x);
                }
            }

            partial record Intersectionf
            {
                // f(x) /\ A = value
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                    => throw FutureReleaseException.Raised("Piecewise to check whether value is subset of A", "1.2");
            }

            partial record SetMinusf
            {
                // f(x) \ A = value
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                    => throw FutureReleaseException.Raised("Piecewise to check whether value is subset of A", "1.2");
            }

            partial record Inf
            {
                // TODO: CSet is needed here => InvertNode to return a Set, not an IEnumerable
                // f(x) in A = value
                private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                    => throw FutureReleaseException.Raised("InvertNode should return set", "1.2");
            }
        }

        partial record Phif
        {
            // We can't easily calculate (compute) all solutions there are for this function
            // There is an algorithm to find exactly one solution but we can do no more.
            // TODO: Mess with that...
            private protected override IEnumerable<Entity> InvertNode(Entity value, Entity x)
                => Enumerable.Empty<Entity>();
        }
    }
}
