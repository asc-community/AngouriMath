
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

using System;
using System.Collections.Generic;
using System.Linq;
using GenericTensor.Core;
using PeterO.Numbers;
using AngouriMath.Core;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Continuous
        {
            /// <summary>Number node.
            /// This class represents all possible numerical values as a hierarchy,
            /// <list>
            ///   <see cref="Number"/>
            ///   <list type="bullet">
            ///     <see cref="Complex"/>
            ///       <list type="bullet">
            ///         <see cref="Real"/>
            ///         <list type="bullet">
            ///           <see cref="Rational"/>
            ///           <list type="bullet">
            ///             <see cref="Integer"/>
            ///           </list>
            ///         </list>
            ///       </list>
            ///     </list>
            ///   </list>
            /// </summary>
            public abstract partial record Number : Continuous
            {
                public override Entity Replace(Func<Entity, Entity> func) => func(this);
                protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();
                public abstract bool IsExact { get; }
                public static implicit operator Number(sbyte value) => Integer.Create(value);
                public static implicit operator Number(byte value) => Integer.Create(value);
                public static implicit operator Number(short value) => Integer.Create(value);
                public static implicit operator Number(ushort value) => Integer.Create(value);
                public static implicit operator Number(int value) => Integer.Create(value);
                public static implicit operator Number(uint value) => Integer.Create(value);
                public static implicit operator Number(long value) => Integer.Create(value);
                public static implicit operator Number(ulong value) => Integer.Create(value);
                public static implicit operator Number(EInteger value) => Integer.Create(value);
                public static implicit operator Number(ERational value) => Rational.Create(value);
                public static implicit operator Number(EDecimal value) => Real.Create(value);
                public static implicit operator Number(float value) => Real.Create(EDecimal.FromSingle(value));
                public static implicit operator Number(double value) => Real.Create(EDecimal.FromDouble(value));
                public static implicit operator Number(decimal value) => Real.Create(EDecimal.FromDecimal(value));
                public static implicit operator Number(System.Numerics.Complex value) =>
                    Complex.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
            }

            #region Operators
            public partial record Sumf(Continuous Augend, Continuous Addend) : Continuous
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Sumf New(Continuous augend, Continuous addend) =>
                    ReferenceEquals(Augend, augend) && ReferenceEquals(Addend, addend) ? this : new(augend, addend);
                public override Priority Priority => Priority.Sum;
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Augend.Replace(func), Addend.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Augend, Addend };
                /// <summary>
                /// Gathers linear children of a sum, e.g.
                /// <code>1 + (x - a/2) + b - 4</code>
                /// would return
                /// <code>{ 1, x, (-1) * a / 2, b, (-1) * 4 }</code>
                /// </summary>
                internal static IEnumerable<Entity> LinearChildren(Entity tree) => tree switch
                {
                    Sumf(var augend, var addend) => LinearChildren(augend).Concat(LinearChildren(addend)),
                    Minusf(var subtrahend, var minuend) =>
                        LinearChildren(subtrahend).Concat(LinearChildren(minuend).Select(entity => -1 * entity)),
                    _ => new[] { tree }
                };
            }
            public static Continuous operator +(Continuous a, Continuous b) => new Sumf(a, b);
            public static Entity operator +(Entity a) => a;

            public partial record Minusf(Entity Subtrahend, Entity Minuend) : Continuous
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Minusf New(Entity subtrahend, Entity minuend) =>
                    ReferenceEquals(Subtrahend, subtrahend) && ReferenceEquals(Minuend, minuend) ? this : new(subtrahend, minuend);
                public override Priority Priority => Priority.Minus;
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Subtrahend.Replace(func), Minuend.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Subtrahend, Minuend };
            }
            public static Entity operator -(Entity a, Entity b) => new Minusf(a, b);

            public partial record Mulf(Entity Multiplier, Entity Multiplicand) : Continuous
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Mulf New(Entity multiplier, Entity multiplicand) =>
                    ReferenceEquals(Multiplier, multiplier) && ReferenceEquals(Multiplicand, multiplicand) ? this : new(multiplier, multiplicand);
                public override Priority Priority => Priority.Mul;
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Multiplier.Replace(func), Multiplicand.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Multiplier, Multiplicand };
                /// <summary>
                /// Gathers linear children of a product, e.g.
                /// <code>1 * (x / a^2) * b / 4</code>
                /// would return
                /// <code>{ 1, x, (a^2)^(-1), b, 4^(-1) }</code>
                /// </summary>
                internal static IEnumerable<Entity> LinearChildren(Entity tree) => tree switch
                {
                    Mulf(var multiplier, var multiplicand) => LinearChildren(multiplier).Concat(LinearChildren(multiplicand)),
                    Divf(var dividend, var divisor) =>
                        LinearChildren(dividend).Concat(LinearChildren(divisor).Select(entity => new Powf(entity, -1))),
                    _ => new[] { tree }
                };
            }
            public static Entity operator -(Entity a) => new Mulf(-1, a);
            public static Entity operator *(Entity a, Entity b) => new Mulf(a, b);

            public partial record Divf(Entity Dividend, Entity Divisor) : Continuous
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Divf New(Entity dividend, Entity divisor) =>
                    ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor);
                public override Priority Priority => Priority.Div;
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
            }
            public static Entity operator /(Entity a, Entity b) => new Divf(a, b);
            #endregion

            public abstract record Function : Continuous
            {
                public override Priority Priority => Priority.Func;
            }

            #region Trigonometry
            public partial record Sinf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Sinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Sin() => new Sinf(this);
            public partial record Cosf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Cosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Cos() => new Cosf(this);
            public partial record Tanf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Tanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Tan() => new Tanf(this);
            public partial record Cotanf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Cotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Cotan() => new Cotanf(this);
            #endregion

            #region Exponential
            public partial record Powf(Entity Base, Entity Exponent) : Entity
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Powf New(Entity @base, Entity exponent) =>
                    ReferenceEquals(Base, @base) && ReferenceEquals(Exponent, exponent) ? this : new(@base, exponent);
                public override Priority Priority => Priority.Pow;
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Exponent.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Base, Exponent };
            }
            public Entity Pow(Entity n) => new Powf(this, n);

            public partial record Logf(Entity Base, Entity Antilogarithm) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Logf New(Entity @base, Entity antilogarithm) =>
                    ReferenceEquals(Base, @base) && ReferenceEquals(Antilogarithm, antilogarithm) ? this : new(@base, antilogarithm);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Antilogarithm.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Base, Antilogarithm };
            }
            public Entity Log(Entity x) => new Logf(this, x);

            public partial record Arcsinf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Arcsinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            #endregion

            #region Arc trigonometry
            public Entity Arcsin() => new Arcsinf(this);
            public partial record Arccosf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Arccosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }

            public Entity Arccos() => new Arccosf(this);
            public partial record Arctanf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Arctanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }

            public Entity Arctan() => new Arctanf(this);
            public partial record Arccotanf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Arccotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Arccotan() => new Arccotanf(this);
            #endregion

            #region Factorial
            public partial record Factorialf(Entity Argument) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Factorialf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
                // This is still a function for pattern replacement
                public override Priority Priority => Priority.Factorial;
                public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Factorial() => new Factorialf(this);
            #endregion

            #region Non-numeric nodes
            public partial record Derivativef(Entity Expression, Entity Var, Entity Iterations) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Derivativef New(Entity expression, Entity var, Entity iterations) =>
                    ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var) && ReferenceEquals(Iterations, iterations)
                    ? this : new(expression, var, iterations);
                public override Entity Replace(Func<Entity, Entity> func) =>
                    func(New(Expression.Replace(func), Var.Replace(func), Iterations.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
            }

            public partial record Integralf(Entity Expression, Entity Var, Entity Iterations) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Integralf New(Entity expression, Entity var, Entity iterations) =>
                    ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var) && ReferenceEquals(Iterations, iterations)
                    ? this : new(expression, var, iterations);
                public override Entity Replace(Func<Entity, Entity> func) =>
                    func(New(Expression.Replace(func), Var.Replace(func), Iterations.Replace(func)));
                protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
            }

            public partial record Limitf(Entity Expression, Entity Var, Entity Destination, ApproachFrom ApproachFrom) : Function
            {
                /// <summary>Reuse the cache by returning the same object if possible</summary>
                Limitf New(Entity expression, Entity var, Entity destination, ApproachFrom approachFrom) =>
                    ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var) && ReferenceEquals(Destination, destination)
                    && ApproachFrom == approachFrom ? this : new(expression, var, destination, approachFrom);
                public override Entity Replace(Func<Entity, Entity> func) =>
                    func(New(Expression.Replace(func), Var.Replace(func), Destination.Replace(func), ApproachFrom));
                protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Destination };
            }
            #endregion

            #region Discrete

            public partial record Signumf(Entity Argument) : Function
            {
                public override Entity Replace(Func<Entity, Entity> func) => func(Argument.Replace(func).Signum());
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Signum() => new Signumf(this);

            public partial record Absf(Entity Argument) : Function
            {
                public override Entity Replace(Func<Entity, Entity> func) => func(Argument.Replace(func).Abs());
                protected override Entity[] InitDirectChildren() => new[] { Argument };
            }
            public Entity Abs() => new Absf(this);

            #endregion
        }
    }
}
