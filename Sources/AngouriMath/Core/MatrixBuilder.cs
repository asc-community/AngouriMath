//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;

namespace AngouriMath.Core
{
    /// <summary>
    /// Use this class for solvers and other places when a matrix needs to be built without
    /// recreating an instance multiple times. It builds an instance of <see cref="Matrix"/>.
    /// It enables to build a tensor row-by-row.
    /// </summary>
    /// <example>
    /// <code>
    /// using System;
    /// using AngouriMath;
    /// using AngouriMath.Core;
    /// 
    /// var mb = new MatrixBuilder(3);
    /// Console.WriteLine(mb.ToMatrix() is null);
    /// Console.WriteLine("-------------------------");
    /// 
    /// mb.Add(new Entity[] { 1, 2, 3 } );
    /// mb.Add(new Entity[] { "x", "sqrt(y)", 5 } );
    /// Console.WriteLine(mb.ToMatrix().ToString(multilineFormat: true));
    /// Console.WriteLine("-------------------------");
    /// 
    /// mb.Add(new Entity[] { 1 } ); // throws
    /// </code>
    /// Prints
    /// <code>
    /// True
    /// -------------------------
    /// Matrix[2 x 3]
    /// 1         2         3         
    /// x         sqrt(y)   5         
    /// -------------------------
    /// Unhandled exception. AngouriMath.Core.Exceptions.InvalidMatrixOperationException: Incorrect usage of MatrixBuilder
    /// </code>
    /// </example>
    public sealed class MatrixBuilder
    {
        private readonly List<List<Entity>> raw = new();
        private readonly int columnCount;

        /// <summary>
        /// Creates a builder with the given number of column and no rows.
        /// </summary>
        /// <param name="columnCount">
        /// The number of columns the tensor will have (you cannot change it after creation).
        /// </param>
        public MatrixBuilder(int columnCount)
        {
            this.columnCount = columnCount;
        }

        /// <summary>
        /// Creates a builder with the given number of column and no rows.
        /// </summary>
        /// <param name="alreadyHas">
        /// The list of rows to put in the builder. All lists in this list
        /// must have the same length as columnCount.
        /// </param>
        /// <param name="columnCount">
        /// The number of columns the tensor will have (you cannot change it after creation).
        /// </param>
        public MatrixBuilder(List<List<Entity>>? alreadyHas, int columnCount) : this(columnCount)
        {
            if (alreadyHas is not null)
            {
                foreach (var row in alreadyHas)
                    if (row.Count != columnCount)
                        throw new AngouriBugException($"Invalid usage of {nameof(MatrixBuilder)}");
                raw = alreadyHas;
            }
        }

        /// <summary>
        /// Adds a row to the builder.
        /// </summary>
        /// <param name="row">
        /// A row to add. Make sure it has the same length as columnCount.
        /// </param>
        /// <exception cref="InvalidMatrixOperationException">
        /// Is thrown if the given row has a wrong length.
        /// </exception>
        public void Add(List<Entity> row)
        { 
            if (row.Count == columnCount)
                raw.Add(row);
            else
                throw new InvalidMatrixOperationException($"Incorrect usage of {nameof(MatrixBuilder)}"); 
        }

        /// <summary>
        /// Adds a row to the builder.
        /// </summary>
        /// <param name="row">
        /// A row to add. Make sure it has the same length as columnCount.
        /// </param>
        /// <exception cref="InvalidMatrixOperationException">
        /// Is thrown if the given row has a wrong length.
        /// </exception>
        public void Add(IEnumerable<Entity> row)
            => Add(row.ToList());

        /// <summary>
        /// Builds itself into a <see cref="Matrix"/>.
        /// </summary>
        /// <returns>
        /// An immutable <see cref="Matrix"/> if there exists at least one row.
        /// Null otherwise.
        /// </returns>
        public Matrix? ToMatrix()
        {
            if (raw.Count == 0)
                return null;
            return new Matrix(indices => raw[indices[0]][indices[1]], raw.Count, columnCount);
        }
    }
}
