using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    // Adding function Derive to Entity
    public abstract partial class Entity
    {
        public Entity Derive(VariableEntity x)
        {
            if(IsLeaf)
            {
                if (this is VariableEntity && this.Name == x.Name)
                    return new NumberEntity(1);
                else
                    return new NumberEntity(0);
            }
            //else
                
        }
    }
}
