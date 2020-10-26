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
using System;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    using SortLevel = Functions.TreeAnalyzer.SortLevel;
    public abstract partial record Entity
    {
        /// <summary>Hash that is convenient to sort with</summary>
        internal string SortHash(SortLevel level) =>
            SortHashName(level) + string.Join("_", DirectChildren.Select(child => child.SortHash(level)).Where(x => x is not ""));
        private protected abstract string SortHashName(SortLevel level);

        public partial record Number
        {
            private protected override string SortHashName(SortLevel level) => level switch
            {
                SortLevel.HIGH_LEVEL => "",
                SortLevel.MIDDLE_LEVEL => Stringize(),
                _ => Stringize() + " "
            };
        }

        // TODO: reorder all those names to make similar functions appear closer

        public partial record Variable
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

        public partial record Secantf
        {
            private protected override string SortHashName(SortLevel level) => "cosf_secf_";
        }

        public partial record Cosecantf
        {
            private protected override string SortHashName(SortLevel level) => "sinf_cscf_";
        }

        public partial record Tanf
        {
            private protected override string SortHashName(SortLevel level) => "tanf_";
        }

        public partial record Cotanf
        {
            private protected override string SortHashName(SortLevel level) => "tanf_cotanf_";
        }

        public partial record Arcsecantf
        {
            private protected override string SortHashName(SortLevel level) => "asecf_";
        }

        public partial record Arccosecantf
        {
            private protected override string SortHashName(SortLevel level) => "acosf_";
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

        public partial record Signumf
        {
            private protected override string SortHashName(SortLevel level) => "signumf_";
        }

        public partial record Absf
        {
            private protected override string SortHashName(SortLevel level) => "absf_";
        }

        partial record Boolean
        {
            private protected override string SortHashName(SortLevel level)
                => level switch
                {
                    SortLevel.HIGH_LEVEL => "",
                    SortLevel.MIDDLE_LEVEL => Stringize(),
                    _ => Stringize() + " "
                };
        }

        partial record Notf
        {
            private protected override string SortHashName(SortLevel level)
                => level == SortLevel.LOW_LEVEL ? "not_" : "";
        }

        partial record Andf
        {
            private protected override string SortHashName(SortLevel level)
                => level == SortLevel.LOW_LEVEL ? "and_" : "";
        }

        partial record Orf
        {
            private protected override string SortHashName(SortLevel level)
                => level == SortLevel.LOW_LEVEL ? "or_" : "";
        }

        partial record Xorf
        {
            private protected override string SortHashName(SortLevel level)
                => level == SortLevel.LOW_LEVEL ? "xor_" : "";
        }

        partial record Impliesf
        {
            private protected override string SortHashName(SortLevel level)
                => level == SortLevel.LOW_LEVEL ? "impl_" : "";
        }

        partial record Equalsf
        {
            private protected override string SortHashName(SortLevel level)
               => level == SortLevel.LOW_LEVEL ? "equals_" : "";
        }

        partial record Greaterf
        {
            private protected override string SortHashName(SortLevel level)
               => level == SortLevel.LOW_LEVEL ? "greater_" : "";
        }

        partial record GreaterOrEqualf
        {
            private protected override string SortHashName(SortLevel level)
               => level == SortLevel.LOW_LEVEL ? "greaterequal_" : "";
        }

        partial record Lessf
        {
            private protected override string SortHashName(SortLevel level)
               => level == SortLevel.LOW_LEVEL ? "less_" : "";
        }

        partial record LessOrEqualf
        {
            private protected override string SortHashName(SortLevel level)
               => level == SortLevel.LOW_LEVEL ? "lessequal_" : "";
        }

        partial record Set
        {
            partial record FiniteSet
            {
                private protected override string SortHashName(SortLevel level)
                    => level switch
                    {
                        SortLevel.HIGH_LEVEL => "",
                        SortLevel.MIDDLE_LEVEL => Stringize(),
                        _ => Stringize() + " "
                    };
            }

            partial record Interval
            {
                private protected override string SortHashName(SortLevel level)
                    => level switch
                    {
                        SortLevel.HIGH_LEVEL => "",
                        SortLevel.MIDDLE_LEVEL => Stringize(),
                        _ => Stringize() + " "
                    };
            }

            partial record ConditionalSet
            {
                private protected override string SortHashName(SortLevel level)
                    => level switch
                    {
                        SortLevel.HIGH_LEVEL => "",
                        SortLevel.MIDDLE_LEVEL => Stringize(),
                        _ => Stringize() + " "
                    };
            }

            partial record SpecialSet
            {
                private protected override string SortHashName(SortLevel level)
                    => level switch
                    {
                        SortLevel.HIGH_LEVEL => "",
                        SortLevel.MIDDLE_LEVEL => Stringize(),
                        _ => Stringize() + " "
                    };
            }

            partial record Unionf
            {
                private protected override string SortHashName(SortLevel level)
                    => level == SortLevel.LOW_LEVEL ? "union_" : "";
            }

            partial record Intersectionf
            {
                private protected override string SortHashName(SortLevel level)
                    => level == SortLevel.LOW_LEVEL ? "intersection_" : "";
            }

            partial record SetMinusf
            {
                private protected override string SortHashName(SortLevel level)
                    => level == SortLevel.LOW_LEVEL ? "setminus_" : "";
            }

            partial record Inf
            {
                private protected override string SortHashName(SortLevel level)
                    => level == SortLevel.LOW_LEVEL ? "in_" : "";
            }
        }
    }
}

namespace AngouriMath.Functions
{
    internal static partial class TreeAnalyzer
    {
        internal static IEnumerable<Entity> SortRealsAndNonReals(IEnumerable<Entity> entities)
        {
            var reals = entities.OfType<Real>();
            var nonReals = entities.Where(c => c is not Real);
            var all = reals.OrderBy(c => c).Concat(nonReals);
            return all;
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