
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
using System.Text;
 using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Core
{
    /// <summary>
    /// Basic tensor implementation
    /// https://en.wikipedia.org/wiki/Tensor
    /// </summary>
    public partial class Tensor : Entity
    {
        /// <summary>
        /// List of ints that stand for dimensions
        /// </summary>
        public List<int> Shape { get; }

        /// <summary>
        /// Numbere of dimensions. 2 for matrix, 1 for vector
        /// </summary>
        public int Dimensions => Shape.Count;

        /// <summary>
        /// Used for swapping axes if a tensor is transposed
        /// </summary>
        private readonly int[] AxesOrder;

        /// <summary>
        /// Data in a linear array
        /// </summary>
        internal Entity[] Data;

        private readonly List<int> Volumes; // 10x20x30 in Shape => 600x30x1 in Volumes

        /// <summary>
        /// List of dimensions
        /// If you need matrix, list 2 dimensions 
        /// If you need vector, list 1 dimension (length of the vector)
        /// You can't list 0 dimensions
        /// </summary>
        /// <param name="dims"></param>
        public Tensor(params int[] dims) : base("tensort")
        {
            if (dims.Length == 0)
                throw new TreeException("Tensor must consist of dimensions");
            Shape = new List<int>();
            Shape.AddRange(dims);

            // Volumes are required to get or set an element by id, for example
            // given tensor 3x4x5 volumes=[60, 20, 5, 1] => tensor[1][2][3] <=>
            // Data[1 * 20 + 2 * 5 + 3 * 1] = Data[33]
            {
                Volumes = new List<int>();
                int r = 1;
                Volumes.Add(r);
                for (int i = Dimensions - 1; i >= 0; i--)
                {
                    r *= Shape[i];
                    Volumes.Add(r);
                }
                Volumes.Reverse();
            }

            // The first element of Volumes is the Volume of the entire tensor
            Data = new Entity[Volumes[0]];

            AxesOrder = new int[dims.Length];
            for (int i = 0; i < AxesOrder.Length; i++)
                AxesOrder[i] = i;
        }

        /// <summary>
        /// 1x2x3 => 33
        /// </summary>
        /// <param name="dims"></param>
        /// <returns></returns>
        private int GetDataIdByIndexes(int[] dims)
        {
            int id = 0;
            for (int i = 0; i < dims.Length; i++)
                id += Volumes[i + 1] * dims[AxesOrder[i]];
            return id;
        }

        /// <summary>
        /// 33 => 1x2x3
        /// </summary>
        /// <param name="dataId"></param>
        private void GetIndexesByDataId(int[] dst, int dataId)
        {
            for (int i = 1; i < Volumes.Count; i++)
            {
                var id = dataId / Volumes[i];
                dst[i - 1] = id;
                dataId -= Volumes[i] * id;
            }
        }
        
        private bool CheckBounds(int[] dims)
        {
            for (int i = 0; i < dims.Length; i++)
                if (dims[i] < 0 || dims[i] >= Shape[i])
                    return false;
            return true;
        }

        public Entity this[params int[] dims]
        {
            get
            {
                if (!CheckBounds(dims))
                    throw new MathSException("Index out of range");
                var id = GetDataIdByIndexes(dims);
                return Data[id];
            }
            set
            {
                if (!CheckBounds(dims))
                    throw new MathSException("Index out of range");
                var id = GetDataIdByIndexes(dims);
                Data[id] = value;
            }
        }

        public override string ToString()
            => PrintOut(int.MaxValue);

        internal string PrintOut()
        => PrintOut(55);

        public string PrintOut(int maxElLen)
        {
            var bias = Dimensions switch
            {
                1 => "Vector[" + Shape[0] + "]",
                2 => "Matrix[" + Shape[0] + "x" + Shape[1] + "]",
                _ => "Tensor[" + string.Join<int>("x", Shape) + "]"
            };

            switch (Dimensions)
            {
                case 1: return bias + "< " + string.Join<Entity>(" | ", Data) + " >";
                case 2:
                    var res = bias + "\n";
                    var maxlen = new List<int>();
                    for (int i = 0; i < Shape[1]; i++)
                    {
                        var mxlen = 0;
                        for (int j = 0; j < Shape[0]; j++)
                            mxlen = Math.Max(mxlen, this[j, i]?.ToString().Length ?? 4);
                        mxlen = Math.Min(maxElLen, mxlen);
                        maxlen.Add(mxlen);
                    }
                    for (int x = 0; x < Shape[0]; x++)
                    {
                        for (int y = 0; y < Shape[1]; y++)
                        {
                            var ents = this[x, y]?.ToString() ?? "null";
                            if (ents.Length > maxlen[y])
                                ents = ents.Substring(0, maxlen[y] - 1) + @"\";
                            res += ents + new string(' ', maxlen[y] - ents.Length + 3);
                        }
                        res += "\n";
                    }
                    return res;
                default:
                    var tres = bias + "\n";
                    var dims = new int[Dimensions];
                    for (int i = 0; i < Data.Length; i++)
                    {
                        GetIndexesByDataId(dims, i);
                        tres += "t[" + string.Join(", ", dims) + "]: " + Data[i].ToString();
                        tres += "\n";
                    }
                    return tres;
            }
        }

        /// <summary>
        /// Assignes data to internal tensor's array. Not recommended to use
        /// </summary>
        /// <param name="data"></param>
        public void Assign(Entity[] data)
        {
            if (data.Length == Data.Length)
                Data = data;
            else
                throw new MathSException("Axes don't match data"); 
        }

        public bool IsVector() => Dimensions == 1;
        public bool IsMatrix() => Dimensions == 2;

        /// <summary>
        /// Changes the order of axes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Transpose(int a, int b)
        {
            // TODO: check bounds
            var tmp = Shape[a];
            Shape[a] = Shape[b];
            Shape[b] = tmp;

            var tmp_a = AxesOrder[a];
            AxesOrder[a] = AxesOrder[b];
            AxesOrder[b] = tmp_a;

            //InitVolumes();
        }

        /// <summary>
        /// Changes the order of axes in matrix
        /// </summary>
        public void Transpose()
        {
            if (IsMatrix())
                Transpose(0, 1);
            else
                throw new MathSException("Specify axes numbers for non-matrices");
        }

        protected override Entity __copy()
        {
            var _shape = new int[Dimensions];
            for (int i = 0; i < Dimensions; i++)
                _shape[i] = Shape[AxesOrder[i]];
            var t = new Tensor(_shape);
            for (int i = 0; i < Dimensions; i++)
                t.Shape[i] = Shape[i];
            for (int i = 0; i < Data.Length; i++)
                t.Data[i] = Data[i].DeepCopy();
            AxesOrder.CopyTo(t.AxesOrder, 0);
            return t;
        }

        /// <summary>
        /// Converts into LaTeX format
        /// </summary>
        /// <returns></returns>
        public new string Latexise()
        {
            if (IsMatrix())
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
            else if (IsVector())
            {
                var sb = new StringBuilder();
                sb.Append(@"\begin{bmatrix}");
                sb.Append(string.Join(" & ", Data.Select(c => c.Latexise())));
                sb.Append(@"\end{bmatrix}");
                return sb.ToString();
            }
            else
            {
                return this.ToString();
            }
        }
    }
}
