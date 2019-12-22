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
    public abstract partial class Entity
    {
        public string Name { get; set; }
        public bool IsLeaf { get => Children.Count == 0; }
        protected Entity(string name)
        {
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
            Entity res;
            if (this is NumberEntity)
                res = new NumberEntity((this as NumberEntity).Value);
            else if (this is VariableEntity)
                res = new VariableEntity(Name);
            else if (this is OperatorEntity)
                res = new OperatorEntity(Name, Priority);
            else if (this is FunctionEntity)
                res = new FunctionEntity(Name);
            else
                throw new MathSException("Unknown entity");
            return res;
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

        public static bool operator ==(Entity a, Entity b)
        {
            var obj1 = (object)a;
            var obj2 = (object)b;
            if (obj1 == null && obj2 == null)
                return true;
            if (obj1 == null || obj2 == null)
                return false;
            if (a.GetType() != b.GetType())
                return false;
            if (a is NumberEntity && b is NumberEntity)
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
    }
    public class NumberEntity : Entity
    {
        public NumberEntity(Number value) : base(value.ToString()) {
            Priority = Const.PRIOR_NUM;
            Value = value;
        }
        public Number Value { get; internal set; }
        public string Name { get => Value.ToString(); }
        public static implicit operator NumberEntity(int num) => new NumberEntity(num);
        public static implicit operator NumberEntity(Number num) => new NumberEntity(num);
    }
    public class VariableEntity : Entity
    {
        public VariableEntity(string name) : base(name) => Priority = Const.PRIOR_VAR;
    }
    public class OperatorEntity : Entity
    {
        public OperatorEntity(string name, int priority) : base(name) {
            Priority = priority;
        }
    }
    public class FunctionEntity : Entity
    {
        public FunctionEntity(string name) : base(name) => Priority = Const.PRIOR_FUNC;
    }
}
