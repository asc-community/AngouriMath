using AngouriMath.Core;
using System;
using System.Collections.Generic;
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
        public string Name = string.Empty;

        public bool IsLeaf { get => Children.Count == 0; }
        protected Entity(string name, Type type)
        {
            this.type = type;
            Children = new List<Entity>();
            Name = name;
        }
        public List<Entity> Children { get; internal set; }

        /// <summary>
        /// Use this to copy one node (unsafe copy!)
        /// </summary>
        /// <returns></returns>
        public Entity Copy()
        {
            switch (type)
            {
                case Type.NUMBER:
                    return new NumberEntity((this as NumberEntity).Value);
                case Type.VARIABLE:
                    return new VariableEntity(Name);
                case Type.OPERATOR:
                    return new OperatorEntity(Name, Priority);
                case Type.FUNCTION:
                    return new FunctionEntity(Name);
                default:
                    throw new MathSException("Unknowne entity type");
            }
        }

        /// <summary>
        /// Use this to copy an entity. Recommended to use if you need a safe copy
        /// </summary>
        /// <returns></returns>
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
            if (a.type != b.type)
                return false;
            if (a.type == Type.NUMBER && b.type == Type.NUMBER)
                return (a as NumberEntity).GetValue() == (b as NumberEntity).GetValue();
            if ((a.Name != b.Name) || (a.Children.Count() != b.Children.Count()))
            {
                return false;
            }
            for (int i = 0; i < a.Children.Count; i++)
            {
                if (!(a.Children[i] == b.Children[i]))
                    return false;
            }
            return true;
        }
        public static bool operator !=(Entity a, Entity b) => !(a == b);

        // TODO create hash for entity
        internal new string GetHashCode() => ToString();

    }
    public class NumberEntity : Entity
    {
        public NumberEntity(Number value) : base(value.ToString(), Type.NUMBER) {
            Priority = Const.PRIOR_NUM;
            Value = value;
        }
        public Number Value { get; internal set; }
        public new string Name { get => Value.ToString(); }
        public static implicit operator NumberEntity(int num) => new NumberEntity(num);
        public static implicit operator NumberEntity(Number num) => new NumberEntity(num);

    }
    public class VariableEntity : Entity
    {
        public VariableEntity(string name) : base(name, Type.VARIABLE) => Priority = Const.PRIOR_VAR;
        public static implicit operator VariableEntity(string name) => new VariableEntity(name);
    }
    public class OperatorEntity : Entity
    {
        public OperatorEntity(string name, int priority) : base(name, Type.OPERATOR) {
            Priority = priority;
        }
    }
    public class FunctionEntity : Entity
    {
        public FunctionEntity(string name) : base(name, Type.FUNCTION) => Priority = Const.PRIOR_FUNC;
    }
}
