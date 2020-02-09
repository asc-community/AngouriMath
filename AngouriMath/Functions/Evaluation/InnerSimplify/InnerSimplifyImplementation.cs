using AngouriMath.Core;
using AngouriMath.Core.Sys.Items.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath
{
    public partial class FunctionEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            if (Children.Any(el => el.entType == Entity.EntType.NUMBER && el.GetValue().IsNull))
                return Number.Null;
            return MathFunctions.InvokeEval(Name, Children);
        }
    }
    public partial class OperatorEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            if (Children.Any(el => el.entType == Entity.EntType.NUMBER && el.GetValue().IsNull))
                return Number.Null;
            return MathFunctions.InvokeEval(Name, Children);
        }
    }
    public partial class NumberEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            return this;
        }
    }
    public partial class VariableEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            return this;
        }
    }
}

namespace AngouriMath.Core
{
    public partial class Tensor : Entity
    {
        internal override Entity InnerSimplify()
        {
            var r = DeepCopy() as Tensor;
            TensorFunctional.Apply(r, e => e.InnerSimplify());
            return r;
        }
    }
}