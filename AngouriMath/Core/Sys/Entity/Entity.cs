
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


using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using AngouriMath.Core.Numerix;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    /// <summary>
    /// The main class in AngouriMath
    /// Every node, expression, or number is an Entity
    /// However, you cannot create an instance of this class
    /// </summary>
    #pragma warning disable CS0660
    #pragma warning disable CS0661
    public abstract partial class Entity : ILatexiseable
    #pragma warning restore CS0661
    #pragma warning restore CS0660
    {
        protected abstract Entity __copy();
        protected abstract bool EqualsTo(Entity obj);
        internal abstract void Check();
        
        public readonly string Name = string.Empty;

        /// <summary>
        /// Usually IsLeaf <=> number, variable, tensor
        /// </summary>
        public bool IsLeaf => Children.Count == 0;
        /* changed from protected to internal due to protection level of EntType */
        internal Entity(string name, EntType type)
        {
            this.entType = type;
            Children = new List<Entity>();
            Name = name;
        }
        /// <summary>
        /// All children nodes of an expression
        /// </summary>
        public List<Entity> Children { get; internal set; }

        /// <summary>
        /// Use this to copy one node (unsafe copy!)
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        internal Entity Copy() => this.__copy();

        /// <summary>
        /// Use this to copy an entity. Recommended to use if you need a safe copy
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public Entity DeepCopy()
        {
            Entity res = Copy();
            foreach (var child in Children)
            {
                res.Children.Add(child.DeepCopy());
            }
            return res;
        }
        
        public static implicit operator Entity(int num)           => new NumberEntity(Number.Create(num));
        public static implicit operator Entity(long num)          => new NumberEntity(Number.Create(num));
        public static implicit operator Entity(ComplexNumber num) => new NumberEntity(num);
        public static implicit operator Entity(decimal num)       => new NumberEntity(Number.Create(num));
        public static implicit operator Entity(float num)         => new NumberEntity(Number.Create(num));
        public static implicit operator Entity(double num)        => new NumberEntity(Number.Create(num));
        public static implicit operator Entity(string expr)       => MathS.FromString(expr);

        /// <summary>
        /// Deep but stupid comparison
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Entity a, Entity b)
        {
            // Since 7.0 we can compare objects to null without casting them into object
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            if (a.entType != b.entType)
                return false;
            return a.EqualsTo(b);
        }
        public static bool operator !=(Entity a, Entity b) => !(a == b);

    }

    /// <summary>
    /// Number node. Maybe converted from Number
    /// </summary>
    public partial class NumberEntity : Entity
    {
        public NumberEntity(ComplexNumber value) : base(value.ToString(), EntType.NUMBER)
        {
            if (value.IsFraction())
                Priority = Const.PRIOR_DIV;
            else if (value.IsImaginary() && value.Real != 0)
                Priority = Const.PRIOR_SUM;
            else if (value.Real < 0 || value.Imaginary < 0)
                Priority = Const.PRIOR_MUL;
            else
                Priority = Const.PRIOR_NUM;
            Value = value;
        }

        /// <summary>
        /// NumberEntity is a node, not a number. To get the number, use either Eval or direct access Value.
        /// </summary>
        public ComplexNumber Value { get; internal set; }
        public new string Name => Value.ToString();
        public static implicit operator NumberEntity((double, double) num) => new NumberEntity(Number.Create(num.Item1, num.Item2));
        public static implicit operator NumberEntity(Complex num) => new NumberEntity(Number.Create(num));
        public static implicit operator NumberEntity(long num) => new NumberEntity(Number.Create(num));
        public static implicit operator NumberEntity(float num) => new NumberEntity(Number.Create(num));
        public static implicit operator NumberEntity(double num) => new NumberEntity(Number.Create(num));
        public static implicit operator NumberEntity(int num) => new NumberEntity(Number.Create(num));
        public static implicit operator NumberEntity(ComplexNumber num) => new NumberEntity(num);

        protected override Entity __copy()
        {
            return new NumberEntity(Value.Copy());
        }

    }

    /// <summary>
    /// Variable node. It only has a name
    /// </summary>
    public partial class VariableEntity : Entity
    {
        public VariableEntity(string name) : base(name, EntType.VARIABLE) => Priority = Const.PRIOR_VAR;
        public static implicit operator VariableEntity(string name) => new VariableEntity(name);
        protected override Entity __copy()
        {
            return new VariableEntity(Name);
        }
    }

    /// <summary>
    /// Operator node. Only binary operators are supported yet
    /// </summary>
    public partial class OperatorEntity : Entity
    {
        public OperatorEntity(string name, int priority) : base(name, EntType.OPERATOR) {
            Priority = priority;
        }
        protected override Entity __copy()
        {
            return new OperatorEntity(Name, Priority);
        }
    }

    /// <summary>
    /// Function entity.
    /// </summary>
    public partial class FunctionEntity : Entity
    {
        public FunctionEntity(string name) : base(name, EntType.FUNCTION) => Priority = Const.PRIOR_FUNC;
        protected override Entity __copy()
        {
            return new FunctionEntity(Name);
        }
    }
}
