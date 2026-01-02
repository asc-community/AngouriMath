//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using GenericTensor.Core;
using AngouriMath.Core.Exceptions;
using HonkSharp.Laziness;
using AngouriMath.Extensions;
using System.Collections;

namespace AngouriMath
{
    using GenTensor = GenTensor<Entity, Entity.Matrix.EntityTensorWrapperOperations>;
    partial record Entity
    {
        #region Tensor

        /// <summary>Basic matrix implementation: <a href="https://en.wikipedia.org/wiki/Matrix_(mathematics)"/></summary>
        public sealed partial record Matrix : Entity, IEnumerable<Entity>
        {
            /// <summary>
            /// The inner matrix of type <see cref="GenTensor"/>.
            /// </summary>
            public GenTensor InnerMatrix { internal get; init; }

            /// <summary>
            /// Creates a <see cref="Entity.Matrix"/> from an instance of <see cref="GenTensor"/>.
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
            public Matrix(GenTensor innerMatrix) : this(innerMatrix, toCopy: true) { }

            private Matrix(GenTensor innerMatrix, bool toCopy)
            {
                if (innerMatrix.Shape.Length != 2)
                    throw new InvalidMatrixOperationException("Only matrices and vectors are supported");
                if (toCopy)
                    InnerMatrix = innerMatrix.Copy(false);
                else
                    InnerMatrix = innerMatrix;
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
                InnerMatrix.Shape != other.InnerMatrix.Shape
                ? throw new InvalidMatrixOperationException("Arguments should be of the same shape to apply elementwise operation")
                : New(GenTensor.CreateTensor(InnerMatrix.Shape, indices =>
                        operation(InnerMatrix.GetValueNoCheck(indices), other.InnerMatrix.GetValueNoCheck(indices))));
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => Elementwise(element => element.Replace(func));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => InnerMatrix.Iterate().Select(tup => tup.Value).ToArray();

#pragma warning disable CS1591
            public readonly struct EntityTensorWrapperOperations : IOperations<Entity>
            {
                public Entity Add(Entity a, Entity b) => (a + b).InnerSimplified;
                public Entity Subtract(Entity a, Entity b) => (a - b).InnerSimplified;
                public Entity Multiply(Entity a, Entity b) => (a * b).InnerSimplified;
                public Entity Negate(Entity a) => (-a).InnerSimplified;
                public Entity Divide(Entity a, Entity b) => (a / b).InnerSimplified;
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
            /// <summary>
            /// The number of columns of a matrix. It is 1 for vectors.
            /// </summary>
            public int ColumnCount => InnerMatrix.Shape[1];

            /// <summary>
            /// The number of rows for matrices. The number of elements for vectors.
            /// </summary>
            public int RowCount => InnerMatrix.Shape[0];

            /// <summary>List of <see cref="int"/>s that stand for dimensions</summary>
            [Obsolete("Use ColumnCount and RowCount instead")]
            public TensorShape Shape => InnerMatrix.Shape;

            /// <summary>
            /// List of dimensions
            /// If you need matrix, list 2 dimensions 
            /// If you need vector, list 1 dimension (length of the vector)
            /// You can't list 0 dimensions
            /// </summary>
            public Matrix(Func<int[], Entity> operation, params int[] dims) : this(GenTensor.CreateTensor(new(dims), operation)) { }

            /// <summary>
            /// Returns the i-th element of a vector, or the i-th row of a matrix.
            /// </summary>
            public Entity this[int i] =>
                (IsVector, IsRowVector) switch
                {
                    (true, false) => InnerMatrix[i, 0],
                    (true, true) => AsScalar(),
                    _ => Enumerable
                                .Range(0, ColumnCount)
                                .Select(id => InnerMatrix[i, id])
                                .ToVector().T
                };

            private static Matrix ToMatrix(Entity entity)
                => entity is Matrix m ? m : MathS.Scalar(entity);

            /// <summary>
            /// Access the tensor if it is a matrix
            /// </summary>
            public Entity this[int x, int y] => InnerMatrix[x, y];

            /// <summary>
            /// Checks whether the matrix only contains one
            /// column.
            /// </summary>
            public bool IsVector => InnerMatrix.Shape[1] == 1;

            /// <summary>
            /// Checks whether it is a row vector, that is,
            /// the matrix has only one row
            /// </summary>
            public bool IsRowVector => InnerMatrix.Shape[0] == 1;

            /// <summary>
            /// Checks whether the matrix is square (has as many rows as columns)
            /// </summary>
            public bool IsSquare => InnerMatrix.Shape[0] == InnerMatrix.Shape[1];

            /// <summary>
            /// Determines whether a matrix is a scalar, that is, a one-by-one matrix
            /// </summary>
            public bool IsScalar => RowCount == 1 && ColumnCount == 1;

            /// <summary>
            /// Casts the matrix to a scalar. It is only
            /// possible if the matrix is 1x1 size.
            /// </summary>
            /// <exception cref="InvalidMatrixOperationException">
            /// Thrown if the matrix has size different from 1x1.
            /// </exception>
            public Entity AsScalar()
                => IsScalar ? this[0, 0] : throw new InvalidMatrixOperationException("A 1x1 matrix expected");

            /// <summary>Changes the order of axes in matrix</summary>
            public Matrix T => t.GetValue(static @this =>
                {
                    var newInner = @this.InnerMatrix.Copy(true);
                    newInner.TransposeMatrix();
                    return new(newInner);
                },
            this);
            private LazyPropertyA<Matrix> t;

            // We do not need to use Gaussian elimination here
            // since we anyway get N! memory use
            /// <summary>
            /// Finds the symbolical determinant via Laplace's method
            /// </summary>
            public Entity? Determinant => determinant.GetValue(
                static @this => 
                {
                    if (!@this.IsSquare) 
                        return null;
                    return @this.InnerMatrix.DeterminantGaussianSafeDivision().InnerSimplified;
                },
                this
                );
            private LazyPropertyA<Entity?> determinant;

            /// <summary>Returns an inverse matrix if it exists</summary>
            public Matrix? Inverse => inverse.GetValue(static @this =>
            {
                var cp = @this.InnerMatrix.Copy(false);
                if (!@this.IsSquare)
                    return null;
                if (@this.Determinant is null)
                    return null;
                if (@this.Determinant == 0)
                    return null;
                cp.InvertMatrix();
                return ToMatrix(new Matrix(cp).InnerSimplified);
            }, this);
            private LazyPropertyA<Matrix?> inverse;


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
                ToMatrix(new Matrix(GenTensor.PiecewiseAdd(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified);

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
                ToMatrix(new Matrix(GenTensor.PiecewiseSubtract(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified);

            /// <summary>
            /// The Multiply operator. Performs an active operation
            /// (matrix multiplication of two matrices)
            /// and then applies inner simplification
            /// </summary>
            public static Matrix operator *(Matrix m1, Matrix m2)
            {
                try
                {
                    var simplified = new Matrix(GenTensor.MatrixMultiply(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified;
                    return ToMatrix(simplified);
                }
                catch (InvalidShapeException)
                {
                    throw new InvalidMatrixOperationException(
                        $"Cannot multiply matrices of shapes {m1.InnerMatrix.Shape} and {m2.InnerMatrix.Shape}"
                        );
                }
            }

            /// <summary>
            /// Performs a binary power of the matrix.
            /// The matrix must be a square matrix.
            /// </summary>
            /// <returns>
            /// A matrix to the given power with InnerSimplified
            /// applied.
            /// </returns>
            public Matrix Pow(int exp)
                => IsSquare switch
                {
                    false => throw new InvalidMatrixOperationException("Cannot find power of a non-square matrix"),
                    true => new(InnerMatrix.MatrixPower(exp))
                };

            /// <summary>
            /// Returns the n-th power of the
            /// given vector or matrix. It is
            /// the same as sequential applying 
            /// n-1 <see cref="TensorProduct"/> to this.
            /// </summary>
            /// <param name="exp">
            /// The power, or the number
            /// of operands in the a *** a *** ... *** a expression
            /// </param>
            /// <returns>
            /// Non-simplified tensor
            /// powered vector or matrix. If it
            /// initially had size n x m, it will have
            /// the size of 2^n x 2^m.
            /// </returns>
            public Matrix TensorPower(int exp)
                => exp switch
                {
                    <= 0 => throw new InvalidMatrixOperationException("Tensor power argument must be positive"),
                    1 => this,
                    _ when exp % 2 is 0 => TensorProduct(this, this).TensorPower(exp / 2),
                    _ => TensorProduct(TensorProduct(this, this).TensorPower(exp / 2), this)
                };

            /// <summary>
            /// Creates a square identity matrix
            /// </summary>
            public static Matrix I(int size)
                => new(GenTensor.CreateIdentityMatrix((int)size));

            /// <summary>
            /// Creates a rectangular identity matrix
            /// with the given size
            /// </summary>
            public static Matrix I(int rowCount, int colCount)
            {
                var inn = GenTensor.CreateMatrix(rowCount, colCount);
                for (int x = 0; x < rowCount; x++)
                    for (int y = 0; y < colCount; y++)
                        inn[x, y] = x == y ? 1 : 0;
                return new(inn);
            }

            /// <summary>
            /// Gets the enumerator. It is required by the interface
            /// IEnumerable, which lets us enumerate over the matrix or vector
            /// </summary>
            /// <returns></returns>
            public IEnumerator<Entity> GetEnumerator()
            {
                for (int i = 0; i < RowCount; i++)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Matrix's form, transformed via Gaussian elimination.
            /// </summary>
            public Matrix RowEchelonForm =>
                @ref.GetValue(static @this =>
                    (Matrix)new Matrix(@this.InnerMatrix.RowEchelonFormSafeDivision()).InnerSimplified,
                    this);
            private LazyPropertyA<Matrix> @ref;

            /// <summary>
            /// Reduced row echelon form via Gaussian elimination.
            /// </summary>
            public Matrix ReducedRowEchelonForm =>
                rref.GetValue(static @this =>
                    (Matrix)new Matrix(@this.InnerMatrix.ReducedRowEchelonFormSafeDivision()).InnerSimplified,
                    this);
            private LazyPropertyA<Matrix> rref;

            /// <summary>
            /// The number of linearly independent rows
            /// </summary>
            public int Rank =>
                rank.GetValue(static @this =>
                {
                    for (int i = 0; i < @this.RowCount; i++)
                        if (@this.ReducedRowEchelonForm.InnerMatrix.RowGetLeadingElement(i) is null)
                            return i;
                    return @this.RowCount;
                }
                , this);
            private LazyPropertyA<int> rank;

            /// <summary>
            /// Adjugate form of a matrix
            /// </summary>
            public Matrix? Adjugate =>
                adjugate.GetValue(static @this =>
                    {
                        if (!@this.IsSquare)
                            return null;
                        var innerSimplified = new Matrix(@this.InnerMatrix.Adjoint()).InnerSimplified;
                        return ToMatrix(innerSimplified);
                    },
                    this);
            private LazyPropertyA<Matrix?> adjugate;

            /// <summary>
            /// Returns a vector, where the i-th element
            /// is the [i, i] element of the matrix. The number
            /// of elements in it equals the minimum of <see cref="RowCount"/> and <see cref="ColumnCount"/>
            /// </summary>
            public Matrix MainDiagonal =>
                mainDiagonal.GetValue(static @this =>
                    Enumerable.Range(0, Math.Min(@this.RowCount, @this.ColumnCount))
                        .Select(i => @this[i, i]).ToVector()
                , this);
            private LazyPropertyA<Matrix> mainDiagonal;

            /// <summary>
            /// The trace of a matrix. It is a sum of
            /// all elements on the main diagonal
            /// </summary>
            public Entity Trace => trace.GetValue(static @this =>
                TreeAnalyzer.MultiHangBinary(@this.MainDiagonal.ToList(), (a, b) => a + b),
                this);
            private LazyPropertyA<Entity> trace;

            /// <summary>
            /// Creates a new vector with the specified element being
            /// set to a new value
            /// </summary>
            public Matrix WithElement(int index, Entity newElement)
            {
                if (!IsVector && !IsRowVector)
                    throw new InvalidMatrixOperationException("Should be vector or row vector");
                var newInner = InnerMatrix.Copy(false);
                if (IsVector)
                    newInner[index, 0] = newElement;
                else
                    newInner[0, index] = newElement;
                return new(newInner, toCopy: false);
            }

            /// <summary>
            /// Creates a new matrix with the specified element being
            /// set to a new value
            /// </summary>
            public Matrix WithElement(int x, int y, Entity newElement)
            {
                var newInner = InnerMatrix.Copy(false);
                newInner[x, y] = newElement;
                return new(newInner, toCopy: false);
            }

            /// <summary>
            /// Creates a new matrix with the specified row
            /// set to a new row
            /// </summary>
            public Matrix WithRow(int rowId, Matrix newRow)
            {
                var newInner = InnerMatrix.Copy(false);
                WithRow(newInner, rowId, newRow);
                return new(newInner, toCopy: false);
            }

            /// <summary>
            /// Creates a new matrix with the specified row
            /// set to a new row
            /// </summary>
            private static void WithRow(GenTensor newInner, int rowId, Matrix newRow)
            {
                if (!newRow.IsRowVector)
                    throw new InvalidMatrixOperationException("Must be a row vector");
                for (int i = 0; i < newInner.Shape[1]; i++)
                    newInner[rowId, i] = newRow[0, i];
            }

            /// <summary>
            /// Creates a new matrix with the specified column
            /// set to a new column
            /// </summary>
            public Matrix WithColumn(int colId, Matrix newCol)
            {
                var newInner = InnerMatrix.Copy(false);
                WithColumn(newInner, colId, newCol);
                return new(newInner, toCopy: false);
            }

            /// <summary>
            /// Creates a new matrix with the specified column
            /// set to a new column
            /// </summary>
            private static void WithColumn(GenTensor newInner, int colId, Matrix newCol)
            {
                if (!newCol.IsVector)
                    throw new InvalidMatrixOperationException("Must be a column vector vector");
                for (int i = 0; i < newInner.Shape[0]; i++)
                    newInner[i, colId] = newCol[i];
            }

            /// <summary>
            /// Returns a new matrix/vector of the same shape,
            /// but with each element replaced by the user.
            /// </summary>
            /// <param name="elementByIndex">
            /// Takes 2 ints which are row number and column
            /// number of an element, and the current element
            /// as the third argument.
            /// </param>
            /// <example>
            /// <code>
            /// var re = O_4.With
            ///     ((rowId, colId, element) => (rowId, colId, element) switch
            ///     {
            ///         (0, 1, _) => 1,
            ///         (1, 2, _) => 2,
            ///         (>1, 3, _) => 6,
            ///         (var a, var b, _) when a + b &lt; 3 => 8,
            ///         _ => element
            ///     });
            /// Console.WriteLine(re.ToString(multilineFormat: true));
            /// </code>
            /// The output is going to be:
            /// <code>
            /// Matrix[4 x 4]
            /// 8   1   8   0
            /// 8   8   2   0
            /// 8   0   0   6
            /// 0   0   0   6
            /// </code>
            /// </example>
            /// <returns>
            /// A new matrix or vector with the same shape
            /// (row count and column count).
            /// </returns>
            public Matrix With(Func<int, int, Entity, Entity> elementByIndex)
            {
                var newInner = InnerMatrix.Copy(false);
                for (int x = 0; x < RowCount; x++)
                    for (int y = 0; y < ColumnCount; y++)
                        newInner[x, y] = elementByIndex(x, y, newInner[x, y]);
                return new(newInner, toCopy: false);
            }

            /// <summary>
            /// Finds a tensor product of two matrices
            /// https://en.wikipedia.org/wiki/Tensor_product
            /// </summary>
            public static Matrix TensorProduct(Matrix a, Matrix b)
                => MathS.ZeroMatrix(a.RowCount * b.RowCount, a.ColumnCount * b.ColumnCount)
                    .With((x, y, _) => a[x / b.RowCount, y / b.ColumnCount] * b[x % b.RowCount, y % b.ColumnCount]);

            /// <summary>
            /// Gets a string representation of the
            /// given matrix/vector. If true is
            /// passed, it will format the inner matrix
            /// with new lines and paddings to make it
            /// look like a matrix in plain monospace
            /// text.
            /// </summary>
            /// <param name="multilineFormat">
            /// Whether to format it into a matrix
            /// format. If false is passed, it will
            /// return the same as normal <see cref="ToString()"/>.
            /// </param>
            public string ToString(bool multilineFormat)
                => multilineFormat ? InnerMatrix.ToString() : ToString();
        }
        #endregion
    }
}
