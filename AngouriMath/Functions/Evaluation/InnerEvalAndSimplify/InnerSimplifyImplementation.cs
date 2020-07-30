
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


using AngouriMath.Core.Sys.Items.Tensors;
using AngouriMath.Core.Numerix;

namespace AngouriMath
{
    public partial class FunctionEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            return MathFunctions.InvokeSimplify(Name, ChildrenReadonly);
        }
    }
    public partial class OperatorEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            return MathFunctions.InvokeSimplify(Name, ChildrenReadonly);
        }
    }
    public partial class NumberEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            var downcasted = ComplexNumber.Create(Value.Real, Value.Imaginary);
            return new NumberEntity(downcasted) { __cachedEvaledValue = downcasted };
        }
    }
    public partial class VariableEntity : Entity
    {
        internal override Entity InnerSimplify()
        {
            if (MathS.ConstantList.ContainsKey(this.Name))
                __cachedEvaledValue = MathS.ConstantList[this.Name];
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
            var r = (Tensor)DeepCopy();
            TensorFunctional.Apply(r, e => e.InnerSimplify());
            return r;
        }
    }
}