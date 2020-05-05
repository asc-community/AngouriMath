
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



﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sys.Items.Tensors
{
    internal static class TensorFunctional
    {
        /// <summary>
        /// Performs scalar product operation on two vectors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static Entity ScalarProduct(Tensor a, Tensor b)
        {
            // TODO: Extend to tensors
            if (!a.IsVector() || !b.IsVector())
                throw new MathSException("Scalar product of non-vectors is not supported yet");
            // TODO: allow different shapes
            if (a.Shape[0] != b.Shape[0])
                throw new MathSException("Vectors should be the same size");
            // TODO: to remove "0" from the final sum
            Entity res = 0;
            for (int i = 0; i < a.Shape[0]; i++)
                res += a[i] * b[i];
            return res;
        }

        /// <summary>
        /// Changes each tensor's data item -> app(item)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="app"></param>
        internal static void Apply(Tensor tensor, Func<Entity, Entity> app)
        {
            for (int i = 0; i < tensor.Data.Length; i++)
                tensor.Data[i] = app(tensor.Data[i]);
        }

        /// <summary>
        /// Returns a new matrix, whose each item = app(A.item, B.item)
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        internal static Entity ApplyPointwise(Tensor A, Tensor B, Func<Entity, Entity, Entity> app)
        {
            if (!SameShape(A, B))
                throw new MathSException("You need two same-shape tensors to perform pointwise operations");
            var C = new Tensor(A.Shape.ToArray());
            for (int i = 0; i < A.Data.Length; i++)
                C.Data[i] = app(A.Data[i], B.Data[i]);
            return C;
        }

        /// <summary>
        /// Finds out whether tensors are of the same shape
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        internal static bool SameShape(Tensor A, Tensor B)
        {
            if (A.Dimensions != B.Dimensions)
                return false;
            for (int i = 0; i < A.Dimensions; i++)
                if (A.Shape[i] != B.Shape[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Performs dot product operation on two matrices
        /// TODO: extend to tensors
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        internal static Tensor DotProduct(Tensor A, Tensor B)
        {
            // TODO: Extend to tensors
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

        /// <summary>
        /// Collapses the entire expression into tensor
        /// </summary>
        /// <param name="expr"></param>
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
                        var ch1 = expr.Children[0];
                        var ch2 = expr.Children[1];
                        if (ch1.entType == Entity.EntType.TENSOR && ch2.entType == Entity.EntType.TENSOR)
                        {
                            var t1 = ch1 as Tensor;
                            var t2 = ch2 as Tensor;
                            string name = expr.Name;
                            expr = ApplyPointwise(t1, t2, (a, b) => MathFunctions.evalTable[name](new List<Entity>{ a, b }));
                        }
                        else if (ch1.entType == Entity.EntType.TENSOR || ch2.entType == Entity.EntType.TENSOR)
                        {
                            Tensor t;
                            Entity c;
                            if (ch1.entType == Entity.EntType.TENSOR)
                            {
                                t = ch1 as Tensor;
                                c = ch2;
                            }
                            else
                            {
                                t = ch2 as Tensor;
                                c = ch1;
                            }
                            string name = expr.Name;
                            Apply(t, e => MathFunctions.evalTable[name](new List<Entity> { e, c }));
                            expr = t;
                        }
                        break;
                    default:
                        throw new NotImplementedException("More than 2 arguments is not supported yet");
                }
            }
        }

        internal static Tensor Matrix(int rows, int columns, params Entity[] values)
        {
            if (values.Length != rows * columns)
                throw new MathSException("Axes don't match data");
            var r = new Tensor(rows, columns);
            r.Assign(values);
            return r;
        }

        internal static Tensor Vector(params Entity[] p)
        {
            var r = new Tensor(p.Length);
            r.Assign(p);
            return r;
        }
    }
}
