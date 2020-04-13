using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngouriMath
{
    /// <summary>
    /// The main class in AngouriMath
    /// Every node, expression, or number is an Entity
    /// However, you cannot create an instance of this class
    /// </summary>
    #pragma warning disable CS0660
    #pragma warning disable CS0661
    public abstract partial class Entity
    #pragma warning restore CS0661
    #pragma warning restore CS0660
    {
        protected abstract Entity __copy();
        protected abstract bool EqualsTo(Entity obj);
        internal abstract void Check();
        
        public string Name = string.Empty;

        /// <summary>
        /// Usually IsLeaf <=> number, variable, tensor
        /// </summary>
        public bool IsLeaf { get => Children.Count == 0; }
        /* changed from protected to internal due to protection level of EntType */
        internal Entity(string name, EntType type)
        {
            this.entType = type;
            Children = new List<Entity>();
            Name = name;
        }
        public List<Entity> Children { get; internal set; }

        /// <summary>
        /// Use this to copy one node (unsafe copy!)
        /// </summary>
        /// <returns></returns>
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
        
        public static implicit operator Entity(int num) => new NumberEntity(num);
        public static implicit operator Entity(Number num) => new NumberEntity(num);
        public static implicit operator Entity(double num) => new NumberEntity(num);
        public static implicit operator Entity(string expr) => MathS.FromString(expr);

        /// <summary>
        /// Deep but stupid comparison
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Entity a, Entity b)
        {
            // Entity must be casted to object before comparing for null
            // Without casting stack overflow occurs as a == null calls same method
            var obj1 = (object)a;
            var obj2 = (object)b;
            if (obj1 == null || obj2 == null)
            {
                if (obj1 == null && obj2 == null)
                    return true;
                else
                    return false;
            }
            if (a.entType != b.entType)
                return false;
            return a.EqualsTo(b);
        }
        public static bool operator !=(Entity a, Entity b) => !(a == b);

        // TODO create hash for entity
        internal new string GetHashCode() => ToString();

    }
    public partial class NumberEntity : Entity
    {
        public NumberEntity(Number value) : base(value.ToString(), EntType.NUMBER) 
        {
            Priority = Const.PRIOR_NUM;
            Value = value;
        }

        public Number Value { get; internal set; }
        public new string Name { get => Value.ToString(); }
        public static implicit operator NumberEntity(int num) => new NumberEntity(num);
        public static implicit operator NumberEntity(Number num) => new NumberEntity(num);

        protected override Entity __copy()
        {
            return new NumberEntity(Value);
        }

    }
    public partial class VariableEntity : Entity
    {
        public VariableEntity(string name) : base(name, EntType.VARIABLE) => Priority = Const.PRIOR_VAR;
        public static implicit operator VariableEntity(string name) => new VariableEntity(name);
        protected override Entity __copy()
        {
            return new VariableEntity(Name);
        }
    }
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
    public partial class FunctionEntity : Entity
    {
        public FunctionEntity(string name) : base(name, EntType.FUNCTION) => Priority = Const.PRIOR_FUNC;
        protected override Entity __copy()
        {
            return new FunctionEntity(Name);
        }
    }
}
