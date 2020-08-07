
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
using System.Linq;
 using System.Runtime.CompilerServices;
 using System.Text;
 using AngouriMath.Core.Numerix;
 using AngouriMath.Core.TreeAnalysis;
using GenericTensor.Core;

[assembly: InternalsVisibleTo("AngouriMath.Core.Sys.Items.Tensors")]
namespace AngouriMath.Core
{
    internal struct EntityTensorWrapperOperations : IOperations<Entity>
    {
        public Entity Add(Entity a, Entity b) => a + b;
        public Entity Subtract(Entity a, Entity b) => a - b;
        public Entity Multiply(Entity a, Entity b) => a * b;
        public Entity Negate(Entity a) => -a;
        public Entity Divide(Entity a, Entity b) => a / b;
        public Entity CreateOne() => IntegerNumber.One;
        public Entity CreateZero() => IntegerNumber.Zero;
        public Entity Copy(Entity a) => a.DeepCopy();
        public Entity Forward(Entity a) => a;
        public bool AreEqual(Entity a, Entity b) => a == b;
        public bool IsZero(Entity a) => a == 0;
        public string ToString(Entity a) => a.ToString();
    }

    /// <summary>
    /// Basic tensor implementation
    /// https://en.wikipedia.org/wiki/Tensor
    /// </summary>
    public partial class Tensor : Entity
    {
        /// <summary>
        /// List of ints that stand for dimensions
        /// </summary>
        public TensorShape Shape => innerTensor.Shape;

        /// <summary>
        /// Numbere of dimensions. 2 for matrix, 1 for vector
        /// </summary>
        public int Dimensions => Shape.Count;

        internal GenTensor<Entity, EntityTensorWrapperOperations> innerTensor;

        /// <summary>
        /// List of dimensions
        /// If you need matrix, list 2 dimensions 
        /// If you need vector, list 1 dimension (length of the vector)
        /// You can't list 0 dimensions
        /// </summary>
        /// <param name="dims"></param>
        public Tensor(params int[] dims) : base("tensort") =>
            innerTensor = GenTensor<Entity, EntityTensorWrapperOperations>.CreateTensor(new TensorShape(dims), inds => 0);

        public Entity this[params int[] dims]
        {
            get => innerTensor[dims];
            set => innerTensor[dims] = value;
        }

        public override string ToString() => innerTensor.ToString(); // TODO

        public bool IsVector => innerTensor.IsVector;
        public bool IsMatrix => innerTensor.IsMatrix;

        /// <summary>
        /// Changes the order of axes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Transpose(int a, int b) => innerTensor.Transpose(a, b);

        /// <summary>
        /// Changes the order of axes in matrix
        /// </summary>
        public void Transpose()
        {
            if (IsMatrix)
                innerTensor.TransposeMatrix();
            else
                throw new MathSException("Specify axes numbers for non-matrices");
        }

        internal Tensor(GenTensor<Entity, EntityTensorWrapperOperations> inner) : base("tensort") =>
            innerTensor = inner;

        protected override Entity __copy() => new Tensor(innerTensor.Copy(copyElements: true));

        /// <summary>
        /// Converts into LaTeX format
        /// </summary>
        /// <returns></returns>
        public new string Latexise()
        {
            if (IsMatrix)
            {
                var sb = new StringBuilder();
                sb.Append(@"\begin{pmatrix}");
                var lines = new List<string>();
                for (int x = 0; x < Shape[0]; x++)
                {
                    var items = new List<string>();

                    for (int y = 0; y < Shape[1]; y++)
                        items.Add(this[x, y].Latexise());

                    var line = string.Join(" & ", items);
                    lines.Add(line);
                }
                sb.Append(string.Join(@"\\", lines));
                sb.Append(@"\end{pmatrix}");
                return sb.ToString();
            }
            else if (IsVector)
            {
                var sb = new StringBuilder();
                sb.Append(@"\begin{bmatrix}");
                sb.Append(string.Join(" & ", innerTensor.Iterate().Select(k => k.Value.Latexise())));
                sb.Append(@"\end{bmatrix}");
                return sb.ToString();
            }
            else
            {
                return this.ToString();
            }
        }

        // We do not need to use Gaussian elimination here
        // since we anyway get N! memory use
        public Entity Determinant() => innerTensor.DeterminantLaplace();

        /// <summary>
        /// Inverts all matrices in a tensor
        /// </summary>
        public Tensor Inverse()
        {
            var cp = innerTensor.Copy(copyElements: true);
            cp.TensorMatrixInvert();
            return new Tensor(cp);
        }
    }
}
