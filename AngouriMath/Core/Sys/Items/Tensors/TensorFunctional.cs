
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
 using AngouriMath.Core.TreeAnalysis;
 using GenericTensor.Core;
 using GenericTensor.Functions;

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
            => GenTensor<Entity>.VectorDotProduct(a.innerTensor, b.innerTensor);

        /// <summary>
        /// Changes each tensor's data item -> app(item)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="app"></param>
        internal static void Apply(Tensor tensor, Func<Entity, Entity> app)
        {
            foreach (var (index, value) in tensor.innerTensor.Iterate())
                tensor.innerTensor.SetValueNoCheck(app(value), index);
        }

        /// <summary>
        /// Returns a new matrix, whose each item = app(A.item, B.item)
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        internal static Entity ApplyPointwise(Tensor A, Tensor B, Func<Entity, Entity, Entity> app) 
            => new Tensor(GenTensor<Entity>.Zip(A.innerTensor, B.innerTensor, app));


        /// <summary>
        /// Finds out whether tensors are of the same shape
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        internal static bool SameShape(Tensor A, Tensor B)
            => A.Shape == B.Shape;

        /// <summary>
        /// Performs dot product operation on two matrices
        /// TODO: extend to tensors
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        internal static Tensor DotProduct(Tensor A, Tensor B)
            => new Tensor(GenTensor<Entity>.MatrixMultiply(A.innerTensor, B.innerTensor));

        /// <summary>
        /// Collapses the entire expression into tensor
        /// </summary>
        /// <param name="expr"></param>
        internal static void __EvalTensor(ref Entity expr)
        {
            if (!expr.IsLeaf)
            {
                for (int i = 0; i < expr.ChildrenCount; i++)
                {
                    var tmp = expr.GetChild(i);
                    __EvalTensor(ref tmp);
                    expr.SetChild(i, tmp);
                }

                static Entity Wrap(Entity p, Entity op)
                {
                    var cp = op.Copy();
                    cp.AddChild(p);
                    return cp;
                }

                switch (expr.ChildrenCount)
                {
                    case 1:
                        if (expr.GetChild(0) is Tensor t)
                        {
                            var ex1 = expr;
                            Apply(t, p => Wrap(p, ex1));
                            expr = expr.GetChild(0);
                        }
                        break;
                    case 2:
                        var ch1 = expr.GetChild(0);
                        var ch2 = expr.GetChild(1);
                        if (ch1 is Tensor t1 && ch2 is Tensor t2)
                        {
                            string name = expr.Name;
                            expr = ApplyPointwise(t1, t2, (a, b) => MathFunctions.evalTable[name](new List<Entity> { a, b }));
                        }
                        else if (ch1 is Tensor tt1)
                        {
                            string name = expr.Name;
                            Apply(tt1, e => MathFunctions.evalTable[name](new List<Entity> { e, ch2 }));
                            expr = tt1;
                        }
                        else if (ch2 is Tensor tt2)
                        {
                            string name = expr.Name;
                            Apply(tt2, e => MathFunctions.evalTable[name](new List<Entity> { e, ch1 }));
                            expr = tt2;
                        }
                        break;
                    default:
                        throw new NotImplementedException("More than 2 arguments is not supported yet");
                }
            }
        }

        internal static Tensor Matrix(Entity[,] values)
            => new Tensor(GenTensor<Entity>.CreateMatrix(values));

        internal static Tensor Matrix(int rows, int columns, params Entity[] values)
            => new Tensor(GenTensor<Entity>.CreateMatrix(rows, columns,
                (x, y) => values[x * columns + y]));

        internal static Tensor Vector(params Entity[] p) 
            => new Tensor(GenTensor<Entity>.CreateVector(p));

        static TensorFunctional()
        {
            ConstantsAndFunctions<Entity>.Add = (a, b) => a + b;
            ConstantsAndFunctions<Entity>.Subtract = (a, b) => a - b;
            ConstantsAndFunctions<Entity>.Multiply = (a, b) => a * b;
            ConstantsAndFunctions<Entity>.Divide = (a, b) => a / b;
            ConstantsAndFunctions<Entity>.CreateZero = () => 0;
            ConstantsAndFunctions<Entity>.CreateOne = () => 1;
            ConstantsAndFunctions<Entity>.AreEqual = (a, b) => a == b;
            ConstantsAndFunctions<Entity>.Negate = a => -a;
            ConstantsAndFunctions<Entity>.IsZero = a => a == 0;
            ConstantsAndFunctions<Entity>.Copy = a => a.DeepCopy();
            ConstantsAndFunctions<Entity>.Forward = a => a;
            ConstantsAndFunctions<Entity>.ToString = a => a.ToString();
        }
    }
}
