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
        public Entity(string name)
        {
            children = new List<Entity>();
            Name = name;
        }
        public List<Entity> children;

        public Entity Copy()
        {
            Entity res;
            if (this is NumberEntity)
                res = new NumberEntity(Name);
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
                res.children.Add(child.DeepCopy());
            return res;
        }
        
        public static implicit operator Entity(int num) => new NumberEntity(num);
        public static implicit operator Entity(double num) => new NumberEntity(num);

        public static bool operator ==(Entity a, Entity b)
        {
            if (a.GetType() != b.GetType())
                return false;
            if (a.Name != b.Name)
                return false;
            if (a.children.Count() != b.children.Count())
                return false;
            for (int i = 0; i < a.children.Count; i++)
                if (!(a.children[i] == b.children[i]))
                    return false;
            return true;
        }
        public static bool operator !=(Entity a, Entity b) => !(a == b);
    }
    public class NumberEntity : Entity
    {
        public NumberEntity(string name) : base(name) => Priority = Const.PRIOR_NUM;
        public NumberEntity(double value) : base(value.ToString()) { }
        public override string ToString()
        {
            return this.Name.ToString();
        }
        public double Value { get => Convert.ToDouble(Name); set => Name = value.ToString(); }
        public static implicit operator NumberEntity(int num) => new NumberEntity(num);
        public static implicit operator NumberEntity(double num) => new NumberEntity(num);
    }
    public class VariableEntity : Entity
    {
        public VariableEntity(string name) : base(name) => Priority = Const.PRIOR_VAR;

        public override string ToString()
        {
            return this.Name.ToString();
        }
    }

    

    public class OperatorEntity : Entity
    {
        public OperatorEntity(string name, int priority) : base(name) {
            Priority = priority;
        }
        public override string ToString()
        {
            MathFunctions.AssertArgs(children.Count, 2);
            string op = "?";
            switch(Name)
            {
                case "Sumf": op = "+"; break;
                case "Minusf": op = "-"; break;
                case "Mulf": op = "*"; break;
                case "Divf": op = "/"; break;
                case "Powf": op = "^"; break;
            }

            return "(" + children[0].ToString() + " " + op + " " + children[1].ToString() + ")";
        }
    }
    public class FunctionEntity : Entity
    {
        public FunctionEntity(string name) : base(name) => Priority = Const.PRIOR_FUNC;
        public override string ToString()
        {
            return Name.Substring(0, Name.Length - 1).ToLower() + "(" + string.Join(",", children) + ")";
        }
    }
}
