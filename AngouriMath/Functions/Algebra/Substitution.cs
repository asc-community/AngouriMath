using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// Substitute a variable with an expression
        /// </summary>
        /// <param name="x">
        /// A variable to substitute
        /// </param>
        /// <param name="value">
        /// The value we replace variable with
        /// </param>
        /// <returns></returns>
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
