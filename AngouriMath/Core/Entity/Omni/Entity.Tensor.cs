
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

using System;
using System.Linq;
using GenericTensor.Core;
using AngouriMath.Core;

namespace AngouriMath
{
    using GenTensor = GenTensor<Entity, Entity.Tensor.EntityTensorWrapperOperations>;
    partial record Entity
    {
        #region Tensor
        /// <summary>Basic tensor implementation: <a href="https://en.wikipedia.org/wiki/Tensor"/></summary>
        public partial record Tensor(GenTensor InnerTensor) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Tensor New(GenTensor innerTensor) =>
                innerTensor.Iterate().All(tup => ReferenceEquals(InnerTensor.GetValueNoCheck(tup.Index), tup.Value))
                ? this
                : new Tensor(innerTensor);
            public override Priority Priority => Priority.Leaf;
            internal Tensor Elementwise(Func<Entity, Entity> operation) =>
                New(GenTensor.CreateTensor(InnerTensor.Shape, indices => operation(InnerTensor.GetValueNoCheck(indices))));
            internal Tensor Elementwise(Tensor other, Func<Entity, Entity, Entity> operation) =>
                Shape != other.Shape
                ? throw new InvalidShapeException("Arguments should be of the same shape to apply elementwise operation")
                : New(GenTensor.CreateTensor(InnerTensor.Shape, indices =>
                        operation(InnerTensor.GetValueNoCheck(indices), other.InnerTensor.GetValueNoCheck(indices))));
            public override Entity Replace(Func<Entity, Entity> func) => Elementwise(element => element.Replace(func));
            protected override Entity[] InitDirectChildren() => InnerTensor.Iterate().Select(tup => tup.Value).ToArray();

            public readonly struct EntityTensorWrapperOperations : IOperations<Entity>
            {
                public Entity Add(Entity a, Entity b) => a + b;
                public Entity Subtract(Entity a, Entity b) => a - b;
                public Entity Multiply(Entity a, Entity b) => a * b;
                public Entity Negate(Entity a) => -a;
                public Entity Divide(Entity a, Entity b) => a / b;
                public Entity CreateOne() => Number.Integer.One;
                public Entity CreateZero() => Number.Integer.Zero;
                public Entity Copy(Entity a) => a;
                public Entity Forward(Entity a) => a;
                public bool AreEqual(Entity a, Entity b) => a == b;
                public bool IsZero(Entity a) => a == 0;
                public string ToString(Entity a) => a.ToString();

                public byte[] Serialize(Entity a)
                {
                    throw new NotImplementedException("Serialization is not planned");
                }

                public Entity Deserialize(byte[] data)
                {
                    throw new NotImplementedException("Deserialization is not planned");
                }
            }
            /// <summary>List of <see cref="int"/>s that stand for dimensions</summary>
            public TensorShape Shape => InnerTensor.Shape;

            /// <summary>Number of dimensions: 2 for matrix, 1 for vector</summary>
            public int Dimensions => Shape.Count;

            /// <summary>
            /// List of dimensions
            /// If you need matrix, list 2 dimensions 
            /// If you need vector, list 1 dimension (length of the vector)
            /// You can't list 0 dimensions
            /// </summary>
            public Tensor(Func<int[], Entity> operation, params int[] dims) : this(GenTensor.CreateTensor(new(dims), operation)) { }

            public Entity this[int i] => InnerTensor[i];
            public Entity this[int x, int y] => InnerTensor[x, y];
            public Entity this[int x, int y, int z] => InnerTensor[x, y, z];
            public Entity this[params int[] dims] => InnerTensor[dims];
            public bool IsVector => InnerTensor.IsVector;
            public bool IsMatrix => InnerTensor.IsMatrix;

            /// <summary>Changes the order of axes</summary>
            public void Transpose(int a, int b) => InnerTensor.Transpose(a, b);

            /// <summary>Changes the order of axes in matrix</summary>
            public void Transpose()
            {
                if (IsMatrix) InnerTensor.TransposeMatrix();
                else throw new Core.Exceptions.MathSException("Specify axes numbers for non-matrices");
            }

            // We do not need to use Gaussian elimination here
            // since we anyway get N! memory use
            public Entity Determinant() => InnerTensor.DeterminantLaplace();

            /// <summary>Inverts all matrices in a tensor</summary>
            public Tensor Inverse()
            {
                var cp = InnerTensor.Copy(false);
                cp.TensorMatrixInvert();
                return new Tensor(cp);
            }
        }
        #endregion
    }
}
