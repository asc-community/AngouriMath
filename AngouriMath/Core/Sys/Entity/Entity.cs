
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
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys;
using AngouriMath.Core.Sys.Interfaces;
using GenericTensor.Core;
using PeterO.Numbers;

namespace AngouriMath
{
    /// <summary>
    /// The main class in AngouriMath
    /// Every node, expression, or number is an Entity
    /// However, you cannot create an instance of this class
    /// </summary>
    // Note: When editing record parameter lists on Visual Studio 16.7.x or 16.8 Preview 1,
    // watch out for Visual Studio crash: https://github.com/dotnet/roslyn/issues/46123
    // Workaround is to use Notepad for editing.
    public abstract partial record Entity : IEnumerable<Entity>, ILatexiseable
    {
        Entity[]? _directChildren;
        protected internal IReadOnlyList<Entity> DirectChildren => _directChildren ??= InitDirectChildren();
        protected abstract Entity[] InitDirectChildren();
        /// <remarks>A depth-first enumeration is required by
        /// <see cref="Core.TreeAnalysis.TreeAnalyzer.GetMinimumSubtree(Entity, VariableEntity)"/></remarks>
        public IEnumerator<Entity> GetEnumerator() =>
            Enumerable.Repeat(this, 1).Concat(DirectChildren.SelectMany(c => c)).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        
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
            Replace(e => e is TFrom from && replacements.TryGetValue(from, out var value) ? value : e);

        public abstract Const.Priority Priority { get; }

        public static Entity operator +(Entity a, Entity b) => new Sumf(a, b);
        public static Entity operator +(Entity a) => a;
        public static Entity operator -(Entity a, Entity b) => new Minusf(a, b);
        public static Entity operator -(Entity a) => new Mulf(-1, a);
        public static Entity operator *(Entity a, Entity b) => new Mulf(a, b);
        public static Entity operator /(Entity a, Entity b) => new Divf(a, b);
        public Entity Pow(Entity n) => new Powf(this, n);
        public Entity Sin() => new Sinf(this);
        public Entity Cos() => new Cosf(this);
        public Entity Tan() => new Tanf(this);
        public Entity Cotan() => new Cotanf(this);
        public Entity Arcsin() => new Arcsinf(this);
        public Entity Arccos() => new Arccosf(this);
        public Entity Arctan() => new Arctanf(this);
        public Entity Arccotan() => new Arccotanf(this);
        public Entity Factorial() => new Factorialf(this);
        public Entity Log(Entity n) => new Logf(this, n);
        public bool IsLowerThan(Entity a) => Priority < a.Priority;
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

        /// <summary>
        /// Checks whether both parts of the complex number are finite
        /// meaning that it could be safely used for calculations
        /// </summary>
        public bool IsFinite => _isFinite ??= ThisIsFinite && DirectChildren.All(x => x.IsFinite);
        protected virtual bool ThisIsFinite => true;
        bool? _isFinite;

        /// <returns>
        /// Number of nodes in tree
        /// TODO: improve measurement of Entity complexity, for example
        /// (1 / x ^ 2).Complexity() < (x ^ (-0.5)).Complexity()
        /// </returns>
        public int Complexity => _complexity ??= 1 + DirectChildren.Sum(x => x.Complexity);
        int? _complexity;

        /// <summary>
        /// Returns list of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// <see cref="Set"/> of unique variables excluding mathematical constants
        /// such as <see cref="pi"/>, <see cref="e"/> and <see cref="i"/>
        /// </returns>
        /// <remarks>
        /// Remember that <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/> will
        /// use <see cref="ICollection{T}.Contains(T)"/> which in this case is <see cref="HashSet{T}.Contains(T)"/>
        /// so it is O(1)
        /// </remarks>
        public IReadOnlyCollection<VariableEntity> Vars => _vars ??=
            new HashSet<VariableEntity>(this is VariableEntity v ? Enumerable.Repeat(v, 1) : DirectChildren.SelectMany(x => x.Vars));
        HashSet<VariableEntity>? _vars;

        /// <summary>Checks if <paramref name="x"/> is a subnode inside this <see cref="Entity"/> tree.
        /// Optimized for <see cref="VariableEntity"/>.</summary>
        public bool Contains(Entity x) => x is VariableEntity v ? Vars.Contains(v) : Enumerable.Contains(this, x);

        public static implicit operator Entity(sbyte value) => IntegerNumber.Create(value);
        public static implicit operator Entity(byte value) => IntegerNumber.Create(value);
        public static implicit operator Entity(short value) => IntegerNumber.Create(value);
        public static implicit operator Entity(ushort value) => IntegerNumber.Create(value);
        public static implicit operator Entity(int value) => IntegerNumber.Create(value);
        public static implicit operator Entity(uint value) => IntegerNumber.Create(value);
        public static implicit operator Entity(long value) => IntegerNumber.Create(value);
        public static implicit operator Entity(ulong value) => IntegerNumber.Create(value);
        public static implicit operator Entity(EInteger value) => IntegerNumber.Create(value);
        public static implicit operator Entity(ERational value) => RationalNumber.Create(value);
        public static implicit operator Entity(EDecimal value) => RealNumber.Create(value);
        public static implicit operator Entity(float value) => RealNumber.Create(EDecimal.FromSingle(value));
        public static implicit operator Entity(double value) => RealNumber.Create(EDecimal.FromDouble(value));
        public static implicit operator Entity(decimal value) => RealNumber.Create(EDecimal.FromDecimal(value));
        public static implicit operator Entity(Complex value) =>
            ComplexNumber.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
        public static implicit operator Entity(string expr) => MathS.FromString(expr);
    }

    /// <summary>Number node.</summary>
    public abstract partial record NumberEntity : Entity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(this);
        protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();
        public static implicit operator NumberEntity(sbyte value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(byte value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(short value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(ushort value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(int value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(uint value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(long value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(ulong value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(EInteger value) => IntegerNumber.Create(value);
        public static implicit operator NumberEntity(ERational value) => RationalNumber.Create(value);
        public static implicit operator NumberEntity(EDecimal value) => RealNumber.Create(value);
        public static implicit operator NumberEntity(float value) => RealNumber.Create(EDecimal.FromSingle(value));
        public static implicit operator NumberEntity(double value) => RealNumber.Create(EDecimal.FromDouble(value));
        public static implicit operator NumberEntity(decimal value) => RealNumber.Create(EDecimal.FromDecimal(value));
        public static implicit operator NumberEntity(Complex value) =>
            ComplexNumber.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
    }

    /// <summary>Variable node. It only has a name</summary>
    public partial record VariableEntity(string Name) : Entity
    {
        public override Const.Priority Priority => Const.Priority.Var;
        public override Entity Replace(Func<Entity, Entity> func) => func(this);
        protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();
        internal static readonly IReadOnlyDictionary<VariableEntity, ComplexNumber> ConstantList =
            new Dictionary<VariableEntity, ComplexNumber>
            {
                { nameof(MathS.DecimalConst.pi), MathS.DecimalConst.pi },
                { nameof(MathS.DecimalConst.e), MathS.DecimalConst.e }
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
        public static implicit operator VariableEntity(string name) => new(name);
    }
    /// <summary>
    /// Basic tensor implementation
    /// https://en.wikipedia.org/wiki/Tensor
    /// </summary>
    public partial record Tensor(GenTensor<Entity, Tensor.EntityTensorWrapperOperations> InnerTensor) : Entity
    {
        public override Const.Priority Priority => Const.Priority.Num;
        Tensor Elementwise(Func<Entity, Entity> operation) =>
            new Tensor(GenTensor<Entity, EntityTensorWrapperOperations>.CreateTensor(
                InnerTensor.Shape, indices => operation(InnerTensor.GetValueNoCheck(indices))));
        public override Entity Replace(Func<Entity, Entity> func) => Elementwise(element => element.Replace(func));
        protected override Entity[] InitDirectChildren() => InnerTensor.Iterate().Select(tup => tup.Value).ToArray();

        public struct EntityTensorWrapperOperations : IOperations<Entity>
        {
            public Entity Add(Entity a, Entity b) => a + b;
            public Entity Subtract(Entity a, Entity b) => a - b;
            public Entity Multiply(Entity a, Entity b) => a * b;
            public Entity Negate(Entity a) => -a;
            public Entity Divide(Entity a, Entity b) => a / b;
            public Entity CreateOne() => IntegerNumber.One;
            public Entity CreateZero() => IntegerNumber.Zero;
            public Entity Copy(Entity a) => a with { };
            public Entity Forward(Entity a) => a;
            public bool AreEqual(Entity a, Entity b) => a == b;
            public bool IsZero(Entity a) => a == 0;
            public string ToString(Entity a) => a.ToString();
        }
        /// <summary>
        /// List of ints that stand for dimensions
        /// </summary>
        public TensorShape Shape => InnerTensor.Shape;

        /// <summary>
        /// Numbere of dimensions. 2 for matrix, 1 for vector
        /// </summary>
        public int Dimensions => Shape.Count;

        /// <summary>
        /// List of dimensions
        /// If you need matrix, list 2 dimensions 
        /// If you need vector, list 1 dimension (length of the vector)
        /// You can't list 0 dimensions
        /// </summary>
        /// <param name="dims"></param>
        public Tensor(params int[] dims)
            : this(GenTensor<Entity, EntityTensorWrapperOperations>.CreateTensor(new TensorShape(dims), inds => 0)) { }

        public Entity this[params int[] dims]
        {
            get => InnerTensor[dims];
            set => InnerTensor[dims] = value;
        }

        public bool IsVector => InnerTensor.IsVector;
        public bool IsMatrix => InnerTensor.IsMatrix;

        /// <summary>
        /// Changes the order of axes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Transpose(int a, int b) => InnerTensor.Transpose(a, b);

        /// <summary>
        /// Changes the order of axes in matrix
        /// </summary>
        public void Transpose()
        {
            if (IsMatrix)
                InnerTensor.TransposeMatrix();
            else
                throw new Core.Exceptions.MathSException("Specify axes numbers for non-matrices");
        }

        // We do not need to use Gaussian elimination here
        // since we anyway get N! memory use
        public Entity Determinant() => InnerTensor.DeterminantLaplace();

        /// <summary>
        /// Inverts all matrices in a tensor
        /// </summary>
        public Tensor Inverse()
        {
            var cp = InnerTensor.Copy(copyElements: true);
            cp.TensorMatrixInvert();
            return new Tensor(cp);
        }
    }

    /// <summary>Binary operator node</summary>
    public abstract partial record OperatorEntity : Entity
    {

    }
    /// <summary>Function node</summary>
    public abstract partial record FunctionEntity : Entity
    {
        public override Const.Priority Priority => Const.Priority.Func;
    }
    public partial record Sumf(Entity Augend, Entity Addend) : OperatorEntity
    {
        public const byte Id = 1;
        public override Const.Priority Priority => Const.Priority.Sum;
        public override Entity Replace(Func<Entity, Entity> func) => func(new Sumf(Augend.Replace(func), Addend.Replace(func)));
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
            _ => Enumerable.Repeat(tree, 1)
        };
    }
    public partial record Minusf(Entity Subtrahend, Entity Minuend) : OperatorEntity
    {
        public override Const.Priority Priority => Const.Priority.Minus;
        public override Entity Replace(Func<Entity, Entity> func) => func(new Minusf(Subtrahend.Replace(func), Minuend.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Subtrahend, Minuend };
    }
    public partial record Mulf(Entity Multiplier, Entity Multiplicand) : OperatorEntity
    {
        public override Const.Priority Priority => Const.Priority.Mul;
        public override Entity Replace(Func<Entity, Entity> func) => func(new Mulf(Multiplier.Replace(func), Multiplicand.Replace(func)));
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
            _ => Enumerable.Repeat(tree, 1)
        };
    }
    public partial record Divf(Entity Dividend, Entity Divisor) : OperatorEntity
    {
        public override Const.Priority Priority => Const.Priority.Div;
        public override Entity Replace(Func<Entity, Entity> func) => func(new Divf(Dividend.Replace(func), Divisor.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
    }
    public partial record Powf(Entity Base, Entity Exponent) : OperatorEntity
    {
        public override Const.Priority Priority => Const.Priority.Pow;
        public override Entity Replace(Func<Entity, Entity> func) => func(new Powf(Base.Replace(func), Exponent.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Base, Exponent };
    }
    public partial record Sinf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Sinf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Cosf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Cosf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Tanf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Tanf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Cotanf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Cotanf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Logf(Entity Base, Entity Antilogarithm) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Logf(Base.Replace(func), Antilogarithm.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Base, Antilogarithm };
    }
    public partial record Arcsinf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Arcsinf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Arccosf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Arccosf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Arctanf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Arctanf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Arccotanf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Arccotanf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Factorialf(Entity Argument) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) => func(new Factorialf(Argument.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Argument };
    }
    public partial record Derivativef(Entity Expression, Entity Variable, Entity Iterations) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) =>
            func(new Derivativef(Expression.Replace(func), Variable.Replace(func), Iterations.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Expression, Variable, Iterations };
    }
    public partial record Integralf(Entity Expression, Entity Variable, Entity Iterations) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) =>
            func(new Integralf(Expression.Replace(func), Variable.Replace(func), Iterations.Replace(func)));
        protected override Entity[] InitDirectChildren() => new[] { Expression, Variable, Iterations };
    }
    public partial record Limitf(Entity Expression, Entity Variable, Entity Destination, Limits.ApproachFrom ApproachFrom) : FunctionEntity
    {
        public override Entity Replace(Func<Entity, Entity> func) =>
            func(new Limitf(Expression.Replace(func), Variable.Replace(func), Destination.Replace(func), ApproachFrom));
        protected override Entity[] InitDirectChildren() => new[] { Expression, Variable, Destination };
    }
}
