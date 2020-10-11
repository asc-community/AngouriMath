/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Core
{
    // TODO: is it useful for the user to have this class?
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
