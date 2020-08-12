
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


using System.Collections.Generic;
 using System.Linq;
 using System.Text;
 using AngouriMath.Core.Sys.Interfaces;


namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Static hash. It won't recount automatically, to recount it call
        /// <see cref="UpdateHash"/>
        /// </summary>
        internal string Hash => _hash ??= UpdateHash();
        string? _hash;

        /// <summary>
        /// Occurances of this exact subtree.
        /// To recount it, call
        /// <see cref="HashOccurancesUpdate(string)"/>
        /// </summary>
        internal int HashOccurances { get; set; }

        /// <summary>
        /// Recounts <see cref="Hash"/>.
        /// Before using <see cref="Hash"/>, make sure you called this function
        /// </summary>
        /// <returns></returns>
        internal string UpdateHash()
        {
            var ownHash = Name;
            var sb = new StringBuilder();
            foreach (var ch in Children)
                sb.Append(ch.UpdateHash());
            _hash = Const.HashString(sb.ToString() + ownHash);
            return Hash;
        }

        /// <summary>
        /// Recounts <see cref="HashOccurances"/>.
        /// Before using <see cref="HashOccurances"/>, make sure you called this function
        /// </summary>
        /// <param name="expr"></param>
        internal static void HashOccurancesUpdate(Entity expr)
        {
            var unfolded = expr.Unfold();

            // First, we count number of occurances for each hash
            var counts = new Dictionary<string, int>();
            foreach (var node in unfolded)
            {
                if (!counts.ContainsKey(node.Hash))
                    counts[node.Hash] = 0;
                counts[node.Hash]++;
            }

            // Second, we assign those numbers to each node
            foreach (var node in unfolded)
                node.HashOccurances = counts[node.Hash];
        }
    }
}