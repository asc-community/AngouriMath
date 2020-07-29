
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
using AngouriMath.Core.Sys;
using AngouriMath.Core.Sys.Interfaces;
using PeterO.Numbers;

namespace AngouriMath
{
    /// <summary>
    /// The main class in AngouriMath
    /// Every node, expression, or number is an Entity
    /// However, you cannot create an instance of this class
    /// </summary>
    public abstract partial class Entity : ILatexiseable, System.IEquatable<Entity>
    {
        protected abstract Entity __copy();
        protected abstract bool EqualsTo(Entity obj);
        internal abstract void Check();
        
        public readonly string Name = string.Empty;

        /// <summary>
        /// Usually IsLeaf <=> number, variable, tensor
        /// </summary>
        public bool IsLeaf => Children.Count == 0;
        protected Entity(string name)
        {
            Children = new List<Entity>();
            Name = name;
            PropsReinit();
        }

        /// <summary>
        /// All children nodes of an expression
        /// </summary>
        private List<Entity> Children { get; set; }

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
                res.AddChild(child.DeepCopy());
            }
            return res;
        }
        
        public static implicit operator Entity(int num)           => new NumberEntity(num);
        public static implicit operator Entity(long num)          => new NumberEntity(num);
        public static implicit operator Entity(ComplexNumber num) => new NumberEntity(num);
        public static implicit operator Entity(EDecimal num)      => new NumberEntity(num);
        public static implicit operator Entity(decimal num)       => new NumberEntity(num);
        public static implicit operator Entity(float num)         => new NumberEntity(num);
        public static implicit operator Entity(double num)        => new NumberEntity(num);
        public static implicit operator Entity(string expr)       => MathS.FromString(expr);

        /// <summary>Deep but stupid comparison</summary>
        public static bool operator ==(Entity? a, Entity? b)
        {
            // Since 7.0 we can compare objects to null without casting them into object
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            // We expect the EqualsTo implementation to check if a's type is equal to b's type
            return a.EqualsTo(b);
        }
        public static bool operator !=(Entity? a, Entity? b) => !(a == b);
        public override bool Equals(object obj) => obj is Entity e && EqualsTo(e);
        bool System.IEquatable<Entity>.Equals(Entity other) => EqualsTo(other);

        
        internal List<Entity> ChildrenReadonly => Children;

        public int ChildrenCount => Children.Count;

        internal void PropsReinit()
        {
            properties = null; // will be reinitted by the first addressing to Properties
        }

        public void SetChild(int index, Entity child)
        {
            Children[index] = child;
            PropsReinit();
        }

        public Entity GetChild(int index)
            => Children[index];


        public void AddChildrenRange(IEnumerable<Entity> children)
        {
            Children.AddRange(children);
            PropsReinit();
        }

        public void AddChild(Entity child)
        {
            Children.Add(child);
            PropsReinit();
        }

        private EntityProperties? properties;
        internal EntityProperties Properties
        {
            get
            {
                if (properties is null)
                    properties = new EntityProperties(this);
                return properties;
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + Properties.GetPropHashCode();
        }

        public bool HasVar(string name)
        {
            if (Name == name && this is VariableEntity)
                return true;
            return Properties.GetPropHasVar(name);
        }
    }

    /// <summary>
    /// Number node. Maybe converted from Number
    /// </summary>
    public partial class NumberEntity : Entity
    {
        public NumberEntity(ComplexNumber value) : base(value.ToString())
        {
            if (value is RationalNumber && !(value is IntegerNumber))
                Priority = Const.PRIOR_DIV;
            else if (!value.Real.Value.IsZero && !value.Imaginary.Value.IsZero)
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
        public static implicit operator NumberEntity(sbyte num) => new NumberEntity(num);
        public static implicit operator NumberEntity(byte num) => new NumberEntity(num);
        public static implicit operator NumberEntity(short num) => new NumberEntity(num);
        public static implicit operator NumberEntity(ushort num) => new NumberEntity(num);
        public static implicit operator NumberEntity(int num) => new NumberEntity(num);
        public static implicit operator NumberEntity(uint num) => new NumberEntity(num);
        public static implicit operator NumberEntity(long num) => new NumberEntity(num);
        public static implicit operator NumberEntity(ulong num) => new NumberEntity(num);
        public static implicit operator NumberEntity(float num) => new NumberEntity(num);
        public static implicit operator NumberEntity(double num) => new NumberEntity(num);
        public static implicit operator NumberEntity(decimal num) => new NumberEntity(num);
        public static implicit operator NumberEntity(EInteger num) => new NumberEntity(num);
        public static implicit operator NumberEntity(IntegerNumber num) => new NumberEntity(num);
        public static implicit operator NumberEntity(ERational num) => new NumberEntity(num);
        public static implicit operator NumberEntity(RationalNumber num) => new NumberEntity(num);
        public static implicit operator NumberEntity(EDecimal num) => new NumberEntity(num);
        public static implicit operator NumberEntity(RealNumber num) => new NumberEntity(num);
        public static implicit operator NumberEntity(Complex num) => new NumberEntity(num);
        public static implicit operator NumberEntity(ComplexNumber num) => new NumberEntity(num);

        protected override Entity __copy()
        {
            // Numbers are immutable, no copying needed
            return new NumberEntity(Value);
        }
    }

    /// <summary>
    /// Variable node. It only has a name
    /// </summary>
    public partial class VariableEntity : Entity
    {
        public VariableEntity(string name) : base(name) => Priority = Const.PRIOR_VAR;
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
        public OperatorEntity(string name, int priority) : base(name) {
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
        public FunctionEntity(string name) : base(name) => Priority = Const.PRIOR_FUNC;
        protected override Entity __copy()
        {
            return new FunctionEntity(Name);
        }
    }
}
