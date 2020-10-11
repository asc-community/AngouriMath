/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using PeterO.Numbers;
using AngouriMath.Core;

namespace AngouriMath
{
    partial record Entity
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
        public abstract partial record Number : NumericNode
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

        public partial record Sumf(Entity Augend, Entity Addend) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Sumf New(Entity augend, Entity addend) =>
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

        public partial record Minusf(Entity Subtrahend, Entity Minuend) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Minusf New(Entity subtrahend, Entity minuend) =>
                ReferenceEquals(Subtrahend, subtrahend) && ReferenceEquals(Minuend, minuend) ? this : new(subtrahend, minuend);
            public override Priority Priority => Priority.Minus;
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Subtrahend.Replace(func), Minuend.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Subtrahend, Minuend };
        }

        public partial record Mulf(Entity Multiplier, Entity Multiplicand) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Mulf New(Entity multiplier, Entity multiplicand) =>
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

        public partial record Divf(Entity Dividend, Entity Divisor) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Divf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor);
            public override Priority Priority => Priority.Div;
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
        }

        #endregion

        public abstract record Function : NumericNode
        {
            public override Priority Priority => Priority.Func;
        }

        #region Trigonometry

        public partial record Sinf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Sinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
        
        public partial record Cosf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Cosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
        
        public partial record Tanf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Tanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
        
        public partial record Cotanf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Cotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
        
        #endregion

        #region Exponential

        public partial record Powf(Entity Base, Entity Exponent) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Powf New(Entity @base, Entity exponent) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Exponent, exponent) ? this : new(@base, exponent);
            public override Priority Priority => Priority.Pow;
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Exponent.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Base, Exponent };
        }

        public partial record Logf(Entity Base, Entity Antilogarithm) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Logf New(Entity @base, Entity antilogarithm) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Antilogarithm, antilogarithm) ? this : new(@base, antilogarithm);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Antilogarithm.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Base, Antilogarithm };
        }

        public partial record Arcsinf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arcsinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        #endregion

        #region Arc trigonometry
        
        public partial record Arccosf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arccosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        public partial record Arctanf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arctanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        public partial record Arccotanf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Arccotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
        
        #endregion

        #region Factorial

        public partial record Factorialf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Factorialf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            // This is still a function for pattern replacement
            public override Priority Priority => Priority.Factorial;
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        #endregion

        #region Non-numeric nodes

        // Iterations should be refactored? to be int instead of Entity
        public partial record Derivativef(Entity Expression, Entity Var, int Iterations) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Derivativef New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations);
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        public partial record Integralf(Entity Expression, Entity Var, int Iterations) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Integralf New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations);
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        public partial record Limitf(Entity Expression, Entity Var, Entity Destination, ApproachFrom ApproachFrom) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Limitf New(Entity expression, Entity var, Entity destination, ApproachFrom approachFrom) =>
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
            private Signumf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        public partial record Absf(Entity Argument) : Function
        {
            private Absf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        #endregion
    }
}
