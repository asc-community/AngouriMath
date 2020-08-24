
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
using System.Numerics;
using System.Runtime.CompilerServices;
using GenericTensor.Core;
using PeterO.Numbers;

namespace AngouriMath.Core
{
    public enum Priority { Sum = 20, Minus = 20, Mul = 40, Div = 40, Pow = 60, Factorial = 70, Func = 80, Variable = 100, Number = 100 }
    public interface ILatexiseable { public string Latexise(); }
}
namespace AngouriMath
{
    using Core;
    using GenTensor = GenTensor<Entity, Entity.Tensor.EntityTensorWrapperOperations>;
    /// <summary>
    /// This is the main class in AngouriMath.
    /// Every node, expression, or number is an <see cref="Entity"/>.
    /// However, you cannot create an instance of this class, look for the nested classes instead.
    /// </summary>
    // Note: When editing record parameter lists on Visual Studio 16.7.x or 16.8 Preview 1,
    // watch out for Visual Studio crash: https://github.com/dotnet/roslyn/issues/46123
    // Workaround is to use Notepad for editing.
    public abstract partial record Entity : ILatexiseable
    {
        private static readonly ConditionalWeakTable<Entity, Entity[]> _directChildren = new();
        protected abstract Entity[] InitDirectChildren();
        public IReadOnlyList<Entity> DirectChildren => _directChildren.GetValue(this, e => e.InitDirectChildren());

        /// <remarks>A depth-first enumeration is required by
        /// <see cref="Core.TreeAnalysis.TreeAnalyzer.GetMinimumSubtree(Entity, Variable)"/></remarks>
        public IEnumerable<Entity> Nodes => DirectChildren.SelectMany(c => c.Nodes).Prepend(this);

        public abstract Entity Replace(Func<Entity, Entity> func);
        public Entity Replace(Func<Entity, Entity> func1, Func<Entity, Entity> func2) =>
            Replace(child => func2(func1(child)));
        public Entity Replace(Func<Entity, Entity> func1, Func<Entity, Entity> func2, Func<Entity, Entity> func3) =>
            Replace(child => func3(func2(func1(child))));

        /// <summary>Replaces all <see cref="x"/> with <see cref="value"/></summary>
        public Entity Substitute(Entity x, Entity value) => Replace(e => e == x ? value : e);
        /// <summary>Replaces all <see cref="replacements"/></summary>
        public Entity Substitute<TFrom, TTo>(IReadOnlyDictionary<TFrom, TTo> replacements)
            where TFrom : Entity where TTo : Entity =>
            replacements.Count == 0
            ? this
            : Replace(e => e is TFrom from && replacements.TryGetValue(from, out var value) ? value : e);

        public abstract Priority Priority { get; }

        /// <summary>Deep but stupid comparison</summary>
        public static bool operator ==(Entity? a, Entity? b)
        {
            // Since C# 7 we can compare objects to null without casting them into object
            if (a is null && b is null)
                return true;
            // We expect the EqualsTo implementation to check if a's type is equal to b's type
            return a?.Equals(b) ?? false;
        }
        public static bool operator !=(Entity? a, Entity? b) => !(a == b);

        /// <value>
        /// Whether both parts of the complex number are finite
        /// meaning that it could be safely used for calculations
        /// </value>
        public bool IsFinite =>
            _isFinite.GetValue(this, e => new(e.ThisIsFinite && e.DirectChildren.All(x => x.IsFinite))).Value;
        protected virtual bool ThisIsFinite => true;
        static readonly ConditionalWeakTable<Entity, Wrapper<bool>> _isFinite = new();
        record Wrapper<T>(T Value) where T : struct { }

        /// <value>Number of nodes in tree</value>
        // TODO: improve measurement of Entity complexity, for example
        // (1 / x ^ 2).Complexity() &lt; (x ^ (-0.5)).Complexity()
        public int Complexity =>
            _complexity.GetValue(this, e => new(1 + e.DirectChildren.Sum(x => x.Complexity))).Value;
        static readonly ConditionalWeakTable<Entity, Wrapper<int>> _complexity = new();

        /// <summary>
        /// Set of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// Set of unique variables excluding mathematical constants
        /// such as <see cref="pi"/> and <see cref="e"/>
        /// </returns>
        public IEnumerable<Variable> Vars => VarsAndConsts.Where(x => !x.IsConstant);
        /// <summary>
        /// Set of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// Set of unique variables and mathematical constants
        /// such as <see cref="pi"/> and <see cref="e"/>
        /// </returns>
        public IReadOnlyCollection<Variable> VarsAndConsts => _vars.GetValue(this, e =>
            new(e is Variable v ? new[] { v } : DirectChildren.SelectMany(x => x.VarsAndConsts)));
        static readonly ConditionalWeakTable<Entity, HashSet<Variable>> _vars = new();

        /// <summary>Checks if <paramref name="x"/> is a subnode inside this <see cref="Entity"/> tree.
        /// Optimized for <see cref="Variable"/>.</summary>
        public bool Contains(Entity x) => x is Variable v ? VarsAndConsts.Contains(v) : Nodes.Contains(x);

        public static implicit operator Entity(sbyte value) => Number.Integer.Create(value);
        public static implicit operator Entity(byte value) => Number.Integer.Create(value);
        public static implicit operator Entity(short value) => Number.Integer.Create(value);
        public static implicit operator Entity(ushort value) => Number.Integer.Create(value);
        public static implicit operator Entity(int value) => Number.Integer.Create(value);
        public static implicit operator Entity(uint value) => Number.Integer.Create(value);
        public static implicit operator Entity(long value) => Number.Integer.Create(value);
        public static implicit operator Entity(ulong value) => Number.Integer.Create(value);
        public static implicit operator Entity(EInteger value) => Number.Integer.Create(value);
        public static implicit operator Entity(ERational value) => Number.Rational.Create(value);
        public static implicit operator Entity(EDecimal value) => Number.Real.Create(value);
        public static implicit operator Entity(float value) => Number.Real.Create(EDecimal.FromSingle(value));
        public static implicit operator Entity(double value) => Number.Real.Create(EDecimal.FromDouble(value));
        public static implicit operator Entity(decimal value) => Number.Real.Create(EDecimal.FromDecimal(value));
        public static implicit operator Entity(Complex value) =>
            Number.Complex.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
        public static implicit operator Entity(string expr) => MathS.FromString(expr);

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
        public abstract partial record Number : Entity
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

        /// <summary>
        /// Variable node. It only has a name.
        /// Construct a <see cref="Variable"/> with an implicit conversion from <see cref="string"/>.
        /// 
        /// </summary>
        public partial record Variable : Entity
        {
            internal static Variable CreateVariableUnchecked(string name) => new(name);
            private Variable(string name) => Name = name;
            public string Name { get; }
            public override Priority Priority => Priority.Variable;
            public override Entity Replace(Func<Entity, Entity> func) => func(this);
            protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();
            internal static readonly Variable pi = new Variable(nameof(pi));
            internal static readonly Variable e = new Variable(nameof(e));
            internal static readonly IReadOnlyDictionary<Variable, Number.Complex> ConstantList =
                new Dictionary<Variable, Number.Complex>
                {
                    { pi, MathS.DecimalConst.pi },
                    { e, MathS.DecimalConst.e }
                };
            /// <summary>
            /// Determines whether something is a variable or contant, e. g.
            /// <list type="table">
            ///     <listheader><term>Expression</term> <description>Is it a constant?</description></listheader>
            ///     <item><term>e</term> <description>Yes</description></item>
            ///     <item><term>x</term> <description>No</description></item>
            ///     <item><term>x + 3</term> <description>No</description></item>
            ///     <item><term>pi + 4</term> <description>No</description></item>
            ///     <item><term>pi</term> <description>Yes</description></item>
            /// </list>
            /// </summary>
            public bool IsConstant => ConstantList.ContainsKey(this);
            /// <summary>
            /// Extracts this <see cref="Variable"/>'s name and index
            /// from its <see cref="Name"/> (e. g. "qua" or "phi_3" or "qu_q")
            /// </summary>
            /// <returns>
            /// If this contains _ and valid name and index, returns a pair of
            /// (<see cref="string"/> Prefix, <see cref="string"/> Index),
            /// <see langword="null"/> otherwise
            /// </returns>
            internal (string Prefix, string Index)? SplitIndex() =>
                Name.IndexOf('_') is var pos_ && pos_ == -1
                ? null
                : ((string Prefix, string Index)?)(Name.Substring(0, pos_), Name.Substring(pos_ + 1));
            /// <summary>
            /// Finds next var index name that is unused in <paramref name="expr"/> starting with 1, e. g.
            /// x + n_0 + n_a + n_3 + n_1
            /// will find n_2
            /// </summary>
            /// <remarks>
            /// This is intended for variables visible to the user.
            /// For non-visible variables, use <see cref="CreateTemp"/> instead.
            /// </remarks>
            internal static Variable CreateUnique(Entity expr, string prefix)
            {
                var indices = new HashSet<int>();
                foreach (var var in expr.Vars)
                    if (var.SplitIndex() is var (varPrefix, index)
                        && varPrefix == prefix
                        && int.TryParse(index, out var num))
                        indices.Add(num);
                var i = 1;
                while (indices.Contains(i))
                    i++;
                return new Variable(prefix + "_" + i);
            }
            /// <summary>Creates a temporary variable like %1, %2 and %3 that is not in <paramref name="existingVars"/></summary>
            /// <remarks>
            /// This is intended for variables not visible to the user.
            /// For visible variables, use <see cref="CreateUnique"/> instead.
            /// </remarks>
            internal static Variable CreateTemp(IEnumerable<Variable> existingVars)
            {
                var indices = new HashSet<int>();
                foreach (var var in existingVars)
                    if (var.Name.StartsWith("%") && int.TryParse(var.Name.Substring(1), out var num))
                        indices.Add(num);
                var i = 1;
                while (indices.Contains(i))
                    i++;
                return new Variable("%" + i);
            }
            public static implicit operator Variable(string name) => (Variable)Parser.Parse(name);
        }
        /// <summary>Basic tensor implementation: <a href="https://en.wikipedia.org/wiki/Tensor"/></summary>
        public partial record Tensor(GenTensor InnerTensor) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Tensor New(GenTensor innerTensor) =>
                innerTensor.Iterate().All(tup => ReferenceEquals(InnerTensor.GetValueNoCheck(tup.Index), tup.Value))
                ? this
                : new Tensor(innerTensor);
            public override Priority Priority => Priority.Number;
            internal Tensor Elementwise(Func<Entity, Entity> operation) =>
                New(GenTensor.CreateTensor(InnerTensor.Shape, indices => operation(InnerTensor.GetValueNoCheck(indices))));
            internal Tensor Elementwise(Tensor other, Func<Entity, Entity, Entity> operation) =>
                Shape != other.Shape
                ? throw new InvalidShapeException("Arguments should be of the same shape to apply elementwise operation")
                : New(GenTensor.CreateTensor(InnerTensor.Shape, indices =>
                        operation(InnerTensor.GetValueNoCheck(indices), other.InnerTensor.GetValueNoCheck(indices))));
            public override Entity Replace(Func<Entity, Entity> func) => Elementwise(element => element.Replace(func));
            protected override Entity[] InitDirectChildren() => InnerTensor.Iterate().Select(tup => tup.Value).ToArray();

            public readonly struct EntityTensorWrapperOperations : IOperations<Entity>
            {
                public Entity Add(Entity a, Entity b) => a + b;
                public Entity Subtract(Entity a, Entity b) => a - b;
                public Entity Multiply(Entity a, Entity b) => a * b;
                public Entity Negate(Entity a) => -a;
                public Entity Divide(Entity a, Entity b) => a / b;
                public Entity CreateOne() => Number.Integer.One;
                public Entity CreateZero() => Number.Integer.Zero;
                public Entity Copy(Entity a) => a;
                public Entity Forward(Entity a) => a;
                public bool AreEqual(Entity a, Entity b) => a == b;
                public bool IsZero(Entity a) => a == 0;
                public string ToString(Entity a) => a.ToString();
            }
            /// <summary>List of <see cref="int"/>s that stand for dimensions</summary>
            public TensorShape Shape => InnerTensor.Shape;

            /// <summary>Number of dimensions: 2 for matrix, 1 for vector</summary>
            public int Dimensions => Shape.Count;

            /// <summary>
            /// List of dimensions
            /// If you need matrix, list 2 dimensions 
            /// If you need vector, list 1 dimension (length of the vector)
            /// You can't list 0 dimensions
            /// </summary>
            public Tensor(Func<int[], Entity> operation, params int[] dims) : this(GenTensor.CreateTensor(new(dims), operation)) { }

            public Entity this[int i] => InnerTensor[i];
            public Entity this[int x, int y] => InnerTensor[x, y];
            public Entity this[int x, int y, int z] => InnerTensor[x, y, z];
            public Entity this[params int[] dims] => InnerTensor[dims];
            public bool IsVector => InnerTensor.IsVector;
            public bool IsMatrix => InnerTensor.IsMatrix;

            /// <summary>Changes the order of axes</summary>
            public void Transpose(int a, int b) => InnerTensor.Transpose(a, b);

            /// <summary>Changes the order of axes in matrix</summary>
            public void Transpose()
            {
                if (IsMatrix) InnerTensor.TransposeMatrix();
                else throw new Core.Exceptions.MathSException("Specify axes numbers for non-matrices");
            }

            // We do not need to use Gaussian elimination here
            // since we anyway get N! memory use
            public Entity Determinant() => InnerTensor.DeterminantLaplace();

            /// <summary>Inverts all matrices in a tensor</summary>
            public Tensor Inverse()
            {
                var cp = InnerTensor.Copy(false);
                cp.TensorMatrixInvert();
                return new Tensor(cp);
            }
        }
        public partial record Sumf(Entity Augend, Entity Addend) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Sumf New(Entity augend, Entity addend) =>
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
        public static Entity operator +(Entity a, Entity b) => new Sumf(a, b);
        public static Entity operator +(Entity a) => a;
        public partial record Minusf(Entity Subtrahend, Entity Minuend) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Minusf New(Entity subtrahend, Entity minuend) =>
                ReferenceEquals(Subtrahend, subtrahend) && ReferenceEquals(Minuend, minuend) ? this : new(subtrahend, minuend);
            public override Priority Priority => Priority.Minus;
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Subtrahend.Replace(func), Minuend.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Subtrahend, Minuend };
        }
        public static Entity operator -(Entity a, Entity b) => new Minusf(a, b);
        public partial record Mulf(Entity Multiplier, Entity Multiplicand) : Entity
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
        public partial record Divf(Entity Dividend, Entity Divisor) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Divf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor);
            public override Priority Priority => Priority.Div;
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
        }
        public static Entity operator /(Entity a, Entity b) => new Divf(a, b);
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
        public abstract record Function : Entity
        {
            public override Priority Priority => Priority.Func;
        }
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
        public partial record Logf(Entity Base, Entity Antilogarithm) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Logf New(Entity @base, Entity antilogarithm) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Antilogarithm, antilogarithm) ? this : new(@base, antilogarithm);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Antilogarithm.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Base, Antilogarithm };
        }
        public Entity Log(Entity n) => new Logf(this, n);
        public partial record Arcsinf(Entity Argument) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Arcsinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
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
    }
}