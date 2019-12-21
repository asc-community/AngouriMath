using System;
using System.Collections.Generic;
using System.Text;

/*
 * Adding substitution of a variable into an expression
 * var x = MathS.Var("x");
 * var n = MathS.Num(3);
 * var c = x ^ n;
 * Console.WriteLine(c.Substitute(x, MathS.Num(5)));
 */

namespace AngouriMath
{
    public abstract partial class Entity
    {
        public Entity Substitute(VariableEntity x, Entity value)
        {
            var res = DeepCopy();
            for (int i = 0; i < Children.Count; i++)
                if (Children[i] is VariableEntity && (Children[i] as VariableEntity).Name == x.Name)
                    res.Children[i] = value;
                else
                    res.Children[i] = Children[i].Substitute(x, value);
            return res;
        }
    }
}
