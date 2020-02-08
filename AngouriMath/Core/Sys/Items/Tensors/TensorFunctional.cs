using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sys.Items.Tensors
{
    internal static class TensorFunctional
    {


        internal static void Apply(Tensor tensor, Func<Entity, Entity> app)
        {
            for (int i = 0; i < tensor.Data.Length; i++)
                tensor.Data[i] = app(tensor.Data[i]);
        }

        internal static bool SameShape(Tensor A, Tensor B)
        {
            if (A.Dimensions != B.Dimensions)
                return false;
            for (int i = 0; i < A.Dimensions; i++)
                if (A.Shape[i] != B.Shape[i])
                    return false;
            return true;
        }

        internal static Tensor DotProduct(Tensor A, Tensor B)
        {
            if (!A.IsMatrix() || !B.IsMatrix())
                throw new MathSException("Dot product is only applicable to matrices");
            if (A.Shape[1] != B.Shape[0])
                throw new MathSException("Matrices are incorrect for dot product");
            var C = new Tensor(A.Shape[0], B.Shape[1]);
            for (var x = 0; x < A.Shape[0]; x++)
                for (var y = 0; y < B.Shape[1]; y++)
                {
                    C[x, y] = 0;
                    for (var k = 0; k < A.Shape[1]; k++)
                        C[x, y] += A[x, k] * B[k, y];
                }
            return C;
        }

        internal static void __EvalTensor(ref Entity expr)
        {
            if (!expr.IsLeaf)
            {
                for (int i = 0; i < expr.Children.Count; i++)
                {
                    var tmp = expr.Children[i];
                    __EvalTensor(ref tmp);
                    expr.Children[i] = tmp;
                }

                Entity Wrap(Entity p, Entity op)
                {
                    var cp = op.Copy();
                    cp.Children.Add(p);
                    return cp;
                }

                switch (expr.Children.Count)
                {
                    case 1:
                        if (expr.Children[0].entType == Entity.EntType.TENSOR)
                        {
                            var ex1 = expr;
                            Apply(expr.Children[0] as Tensor, p => Wrap(p, ex1));
                            expr = expr.Children[0];
                        }
                        break;
                    case 2:
                        break;
                        /*
                        var ch1 = expr.Children[0];
                        var ch2 = expr.Children[1];
                        if (ch1.entType == Entity.EntType.TENSOR && ch2.entType == Entity.EntType.TENSOR)
                        {
                            
                        }*/
                }
            }
        }
    }
}
