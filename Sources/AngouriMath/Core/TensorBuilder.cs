/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Core
{
    /// <summary>
    /// Use this class for solvers and other places when a matrix needs to be built
    /// </summary>
    internal sealed class TensorBuilder
    {
        private readonly List<List<Entity>> raw = new();
        private readonly int columnCount;

        public TensorBuilder(int columnCount)
        {
            this.columnCount = columnCount;
        }

        public TensorBuilder(List<List<Entity>>? alreadyHas, int columnCount) : this(columnCount)
        {
            if (alreadyHas is not null)
            {
                foreach (var row in alreadyHas)
                    if (row.Count != columnCount)
                        throw new AngouriBugException($"Invalid usage of {nameof(TensorBuilder)}");
                raw = alreadyHas;
            }
        }

        public void Add(List<Entity> row)
        { 
            if (row.Count == columnCount)
                raw.Add(row);
            else
                throw new AngouriBugException($"Incorrect usage of {nameof(TensorBuilder)}"); 
        }

        public void Add(IEnumerable<Entity> row)
            => Add(row.ToList());

        public Tensor? ToTensor()
        {
            if (raw.Count == 0)
                return null;
            return new Tensor(indices => raw[indices[0]][indices[1]], raw.Count, columnCount);
        }
    }
}
