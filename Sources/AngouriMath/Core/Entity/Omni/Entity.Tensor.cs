/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;
using System.Linq;
using GenericTensor.Core;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using FieldCacheNamespace;

namespace AngouriMath
{
    using GenTensor = GenTensor<Entity, Entity.Matrix.EntityTensorWrapperOperations>;
    partial record Entity
    {
        #region Tensor

        /// <summary>Basic matrix implementation: <a href="https://en.wikipedia.org/wiki/Matrix_(mathematics)"/></summary>
#pragma warning disable CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters
        public sealed partial record Matrix : Entity
#pragma warning restore CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters
        {
            /// <summary>
            /// The inner matrix of type <see cref="GenTensor"/>.
            /// </summary>
            internal GenTensor InnerMatrix { get; }

            /// <summary>
            /// Creates a <see cref="Matrix"/> from an instance of <see cref="GenTensor"/>.
            /// Since only 2-dimensional matrices are supported, it will throw an exception if the number
            /// of dimensions is too high or too low.
            /// </summary>
            /// <param name="innerMatrix">
            /// The instance of a matrix. It will be copied inside. Make sure,
            /// that it has exactly two dimensions. If a vector needs to be
            /// created, make it a normal matrix with one column.
            /// </param>
            /// <exception cref="InvalidMatrixOperationException">
            /// Is thrown if the number of dimensions of the passed
            /// tensor is not 2.
            /// </exception>
            public Matrix(GenTensor innerMatrix)
            {
                if (innerMatrix.Shape.Length != 2)
                    throw new InvalidMatrixOperationException("Only matrices and vectors are supported");
                InnerMatrix = innerMatrix.Copy(false);
            }

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Matrix New(GenTensor innerMatrix) =>
                innerMatrix.Iterate().All(tup => ReferenceEquals(InnerMatrix.GetValueNoCheck(tup.Index), tup.Value))
                ? this
                : new Matrix(innerMatrix);
            internal override Priority Priority => Priority.Leaf;
            internal Matrix Elementwise(Func<Entity, Entity> operation) =>
                New(GenTensor.CreateTensor(InnerMatrix.Shape, indices => operation(InnerMatrix.GetValueNoCheck(indices))));
            internal Matrix Elementwise(Matrix other, Func<Entity, Entity, Entity> operation) =>
                Shape != other.Shape
                ? throw new InvalidShapeException("Arguments should be of the same shape to apply elementwise operation")
                : New(GenTensor.CreateTensor(InnerMatrix.Shape, indices =>
                        operation(InnerMatrix.GetValueNoCheck(indices), other.InnerMatrix.GetValueNoCheck(indices))));
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => Elementwise(element => element.Replace(func));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => InnerMatrix.Iterate().Select(tup => tup.Value).ToArray();

#pragma warning disable CS1591
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
#pragma warning disable CA1822 // Mark members as static
                public Entity Forward(Entity a) => a;
#pragma warning restore CA1822 // Mark members as static
                public bool AreEqual(Entity a, Entity b) => a == b;
                public bool IsZero(Entity a) => a == 0;
                public string ToString(Entity a) => a.Stringize();

                public byte[] Serialize(Entity a)
                {
                    throw FutureReleaseException.Raised("Serialization");
                }

                public Entity Deserialize(byte[] data)
                {
                    throw FutureReleaseException.Raised("Deserialization");
                }
            }
#pragma warning restore CS1591
            /// <summary>List of <see cref="int"/>s that stand for dimensions</summary>
            public TensorShape Shape => InnerMatrix.Shape;

            /// <summary>
            /// List of dimensions
            /// If you need matrix, list 2 dimensions 
            /// If you need vector, list 1 dimension (length of the vector)
            /// You can't list 0 dimensions
            /// </summary>
            public Matrix(Func<int[], Entity> operation, params int[] dims) : this(GenTensor.CreateTensor(new(dims), operation)) { }

            /// <summary>
            /// Access the tensor if it is a vector
            /// </summary>
            public Entity this[int i] => InnerMatrix[i];

            /// <summary>
            /// Access the tensor if it is a matrix
            /// </summary>
            public Entity this[int x, int y] => InnerMatrix[x, y];

            /// <summary>
            /// Checks whether the matrix only contains one
            /// column
            /// </summary>
            public bool IsVector => InnerMatrix.Shape[1] == 1;

            /// <summary>
            /// Checks whether the matrix is square (has as many rows as columns)
            /// </summary>
            public bool IsSquare => InnerMatrix.Shape[0] == InnerMatrix.Shape[1];

            /// <summary>Changes the order of axes in matrix</summary>
            public Matrix T => t.GetValue(static @this =>
                {
                    var newInner = @this.InnerMatrix.Copy(true);
                    newInner.TransposeMatrix();
                    return new(newInner);
                },
            this);
            private FieldCache<Matrix> t;

            // We do not need to use Gaussian elimination here
            // since we anyway get N! memory use
            /// <summary>
            /// Finds the symbolical determinant via Laplace's method
            /// </summary>
            public Entity Determinant => determinant.GetValue(
                static @this => @this.InnerMatrix.DeterminantLaplace().InnerSimplified,
                this
                );
            private FieldCache<Entity> determinant;

            // The reason it's not cached is because it throws exceptions.
            /// <summary>Inverts the matrix</summary>
            public Matrix ComputeInverse()
            {
                var cp = InnerMatrix.Copy(false);
                try
                {
                    cp.TensorMatrixInvert();
                }
                catch (InvalidShapeException)
                {
                    throw new InvalidMatrixOperationException("Cannot inverse a non-square matrix!");
                }
                catch (InvalidDeterminantException)
                {
                    throw new InvalidMatrixOperationException("Cannot inverse a singular matrix!");
                }
                return new Matrix(cp);
            }

            /// <summary>Inverts the matrix</summary>
            [Obsolete("Use ComputeInverse() instead")]
            public Matrix Inverse() => ComputeInverse();

            /// <summary>
            /// The Add operator. Performs an active operation
            /// (elementwise addition of two matrices or vectors)
            /// and then applies inner simplification
            /// </summary>
            public static Matrix operator +(Matrix m1, Matrix m2)
                =>
                m1.InnerMatrix.Shape != m2.InnerMatrix.Shape
                ?
                throw new InvalidMatrixOperationException("Cannot apply the operator to matrices or vectors of different shapes")
                :
                (Matrix)new Matrix(GenTensor.PiecewiseAdd(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified;

            /// <summary>
            /// The Subtract operator. Performs an active operation
            /// (elementwise subtraction of two matrices or vectors)
            /// and then applies inner simplification
            /// </summary>
            public static Matrix operator -(Matrix m1, Matrix m2)
                =>
                m1.InnerMatrix.Shape != m2.InnerMatrix.Shape
                ?
                throw new InvalidMatrixOperationException("Cannot apply the operator to matrices or vectors of different shapes")
                :
                (Matrix)new Matrix(GenTensor.PiecewiseSubtract(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified;

            /// <summary>
            /// The Multiply operator. Performs an active operation
            /// (matrix multiplication of two matrices)
            /// and then applies inner simplification
            /// </summary>
            public static Matrix operator *(Matrix m1, Matrix m2)
            {
                try
                {
                    return (Matrix)new Matrix(GenTensor.MatrixMultiply(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified;
                }
                catch (InvalidShapeException)
                {
                    throw new InvalidMatrixOperationException(
                        $"Cannot multiply matrices of shapes {m1.InnerMatrix.Shape} and {m2.InnerMatrix.Shape}"
                        );
                }
            }

            /// <summary>
            /// The Multiply operator. Performs an active operation.
            /// 1. Finds the inverse of the divisor
            /// 2. Multiplies the dividend by the inverse of the divisor
            /// and then applies inner simplification.
            /// The operator only works with square matrices of the same size
            /// </summary>
            /// <exception cref="InvalidMatrixOperationException">
            /// May be thrown if no inverse was found.
            /// </exception>
            public static Matrix operator /(Matrix m1, Matrix m2)
            {
                var inv = m2.ComputeInverse();
                return m1 * inv;
            }

            /// <summary>
            /// Performs a binary power of the matrix.
            /// The matrix must be a square matrix.
            /// </summary>
            /// <returns>
            /// A matrix to the given power with InnerSimplified
            /// applied.
            /// </returns>
            public Matrix Pow(uint exp)
            {
                if (!IsSquare)
                    throw new InvalidMatrixOperationException("Cannot find power of a non-square matrix");
                var size = (uint)Shape[0];
                if (exp == 0)
                    return I(size);
                if (exp == 1)
                    return this;
                var p2 = Pow(exp / 2);
                if (exp % 2 == 0)
                    return p2 * p2;
                else
                    return p2 * p2 * this;
            }

            /// <summary>
            /// Creates a square identity matrix
            /// </summary>
            public static Matrix I(uint size)
                => new(GenTensor.CreateIdentityMatrix((int)size));

            /// <summary>
            /// Creates a rectangular identity matrix
            /// with the given size
            /// </summary>
            public static Matrix I(uint rowCount, uint colCount)
            {
                var inn = GenTensor.CreateMatrix((int)rowCount, (int)colCount);
                for (int i = 0; i < Math.Min(rowCount, colCount); i++)
                    inn[i, i] = 1;
                return new(inn);
            }
        }
        #endregion
    }
}
