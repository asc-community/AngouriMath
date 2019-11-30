using MathSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathSharp
{
    public abstract partial class Entity
    {
        public string Name { get; set; }
        public bool IsLeaf { get => children.Count == 0; }
        protected Entity(string name)
        {
            children = new List<Entity>();
            Name = name;
        }
        public List<Entity> children;

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
        public Entity DeepCopy()
        {
            Entity res = Copy();
            foreach (var child in children)
            {
                res.children.Add(child.DeepCopy());
            }
            return res;
        }
        
        public static implicit operator Entity(int num) => new NumberEntity(num);
        public static implicit operator Entity(Number num) => new NumberEntity(num);
        public static implicit operator Entity(double num) => new NumberEntity(num);

        public static bool operator ==(Entity a, Entity b)
        {
            if (a.GetType() != b.GetType())
                return false;
            if (a is NumberEntity && b is NumberEntity)
                return (a as NumberEntity).GetValue() == (b as NumberEntity).GetValue();
            if ((a.Name != b.Name) || (a.children.Count() != b.children.Count()))
            {
                return false;
            }
            for (int i = 0; i < a.children.Count; i++)
            {
                if (!(a.children[i] == b.children[i]))
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
