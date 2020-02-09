using AngouriMath.Core.Sys.Items.Tensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core
{
    public partial class Tensor : Entity
    {
        protected override bool EqualsTo(Entity obj)
        {
            var t = obj as Tensor;
            if (!TensorFunctional.SameShape(this, t))
                return false;
            for (int i = 0; i < Data.Length; i++)
                if (Data[i] != t.Data[i])
                    return false;
            return true;
        }
    }
}

namespace AngouriMath
{
    public partial class FunctionEntity
    {
        protected override bool EqualsTo(Entity obj)
        {
            if (obj.Name != Name)
                return false;
            if (Children.Count != obj.Children.Count)
                return false;
            for (int i = 0; i < obj.Children.Count; i++)
            {
                if (!(Children[i] == obj.Children[i]))
                    return false;
            }
            return true;
        }
    }

    public partial class OperatorEntity
    {
        protected override bool EqualsTo(Entity obj)
        {
            if (obj.Name != Name)
                return false;
            if (Children.Count != obj.Children.Count)
                return false;
            for (int i = 0; i < obj.Children.Count; i++)
            {
                if (!(Children[i] == obj.Children[i]))
                    return false;
            }
            return true;
        }
    }

    public partial class NumberEntity
    {
        protected override bool EqualsTo(Entity obj)
        {
            return obj.GetValue() == GetValue();
        }
    }

    public partial class VariableEntity
    {
        protected override bool EqualsTo(Entity obj)
        {
            return Name == obj.Name;
        }
    }
}
