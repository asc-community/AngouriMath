
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



using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath
{
    using SortLevel = TreeAnalyzer.SortLevel;
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>Hash that is convenient to sort with</summary>
        internal string SortHash(SortLevel level) =>
            SortHashName(level) + string.Join("_", DirectChildren.Select(child => child.SortHash(level)).Where(x => x is not ""));
        private protected abstract string SortHashName(SortLevel level);

        public partial record Number : Entity
        {
            private protected override string SortHashName(SortLevel level) => level switch
            {
                SortLevel.HIGH_LEVEL => "",
                SortLevel.MIDDLE_LEVEL => ToString(),
                _ => ToString() + " "
            };
        }
        public partial record Variable : Entity
        {
            private protected override string SortHashName(SortLevel level) => "v_" + Name;
        }
        public partial record Tensor : Entity
        {
            private protected override string SortHashName(SortLevel level) => level == SortLevel.LOW_LEVEL ? "tensort_" : "";
        }
        // Each function and operator processing
        public partial record Sumf
        {
            private protected override string SortHashName(SortLevel level) => level == SortLevel.LOW_LEVEL ? "sumf_" : "";
        }
        public partial record Minusf
        {
            private protected override string SortHashName(SortLevel level) => level == SortLevel.LOW_LEVEL ? "minusf_" : "";
        }
        public partial record Mulf
        {
            private protected override string SortHashName(SortLevel level) => level == SortLevel.LOW_LEVEL ? "mulf_" : "";
        }
        public partial record Divf
        {
            private protected override string SortHashName(SortLevel level) => level == SortLevel.LOW_LEVEL ? "divf_" : "";
        }
        public partial record Powf
        {
            private protected override string SortHashName(SortLevel level) => level == SortLevel.LOW_LEVEL ? "powf_" : "";
        }
        public partial record Sinf
        {
            private protected override string SortHashName(SortLevel level) => "sinf_";
        }
        public partial record Cosf
        {
            private protected override string SortHashName(SortLevel level) => "cosf_";
        }
        public partial record Tanf
        {
            private protected override string SortHashName(SortLevel level) => "tanf_";
        }
        public partial record Cotanf
        {
            private protected override string SortHashName(SortLevel level) => "cotanf_";
        }

        public partial record Logf
        {
            private protected override string SortHashName(SortLevel level) => "logf_";
        }

        public partial record Arcsinf
        {
            private protected override string SortHashName(SortLevel level) => "arcsinf_";
        }
        public partial record Arccosf
        {
            private protected override string SortHashName(SortLevel level) => "arccosf_";
        }
        public partial record Arctanf
        {
            private protected override string SortHashName(SortLevel level) => "arctanf_";
        }
        public partial record Arccotanf
        {
            private protected override string SortHashName(SortLevel level) => "arccotanf_";
        }
        public partial record Factorialf
        {
            private protected override string SortHashName(SortLevel level) => "factorialf_";
        }

        public partial record Derivativef
        {
            private protected override string SortHashName(SortLevel level) => "derivativef_";
        }

        public partial record Integralf
        {
            private protected override string SortHashName(SortLevel level) => "integralf_";
        }

        public partial record Limitf
        {
            private protected override string SortHashName(SortLevel level) => "limitf_";
        }
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>One group - one hash, Different hashes - different groups</summary>
        internal static IReadOnlyCollection<List<Entity>> GroupByHash(IEnumerable<Entity> entities, SortLevel level)
        {
            var dict = new SortedDictionary<string, List<Entity>>();
            foreach (var ent in entities)
            {
                var hash = ent.SortHash(level);
                if (dict.TryGetValue(hash, out var values))
                    values.Add(ent);
                else dict.Add(hash, new List<Entity> { ent });
            }
            return dict.Values;
        }

        /// <summary>Linear multi hanging: (1 + (1 + (1 + 1)))</summary>
        internal static Entity MultiHangLinear(IReadOnlyList<Entity> children, Func<Entity, Entity, Entity> op)
        {
            var entity = children.Count == 0 ? throw new TreeException("At least 1 child required") : children.Last();
            for (int i = children.Count - 2; i >= 0; i--)
                entity = op(children[i], entity);
            return entity;
        }

        /// <summary>Binary multi hanging: ((1 + 1) + (1 + 1))</summary>
        internal static Entity MultiHangBinary(IReadOnlyList<Entity> children, Func<Entity, Entity, Entity> op)
        {
            Entity MultiHangBinary(int start, int length) =>
                length switch
                {
                    0 => throw new TreeException("At least 1 child required"),
                    1 => children[start],
                    2 => op(children[start], children[start + 1]),
                    _ => op(MultiHangBinary(start, length / 2),
                            MultiHangBinary(start + length / 2, length - length / 2))
                };
            return MultiHangBinary(0, children.Count);
        }
        internal enum SortLevel
        {
            HIGH_LEVEL, // Variables, functions. Doesn't pay attention to constants or ops
            MIDDLE_LEVEL, // Contants are now countable
            LOW_LEVEL, // De facto full hash
        }
    }
}