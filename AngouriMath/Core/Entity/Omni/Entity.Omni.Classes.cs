
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{

    partial record Entity
    {
        /// <summary>
        /// A
        /// </summary>
        public partial record Set : Entity
        {
            #region FiniteSet
            /// <summary>
            /// A finite set is a set whose elements can be counted and enumerated
            /// </summary>
            public partial record FiniteSet : Set, IReadOnlyCollection<Entity>
            {
                public IEnumerable<Entity> Elements => elements.Values;

                private readonly Dictionary<Entity, Entity> elements;

                // TODO: refactor
                public override Entity Replace(Func<Entity, Entity> func)
                {
                    var hasAnythingChanged = false;
                    List<Entity> newElements = new();
                    foreach (var el in Elements)
                    {
                        var changed = el.Replace(func);
                        if (ReferenceEquals(changed, el))
                            hasAnythingChanged = true;
                    }
                    if (hasAnythingChanged)
                        return func(new FiniteSet(newElements));
                    else
                        return func(this);
                }

                public override Priority Priority => Priority.Leaf;

                protected override Entity[] InitDirectChildren() => Elements.ToArray();

                private static Dictionary<Entity, Entity> BuildDictionaryFromElements(IEnumerable<Entity> elements, bool noCheck)
                {
                    Dictionary<Entity, Entity> dict = new(elements.Count());
                    foreach (var elem in elements)
                    {
                        if (!noCheck ||                                              // some operations should be done unconditionally
                            !dict.ContainsKey(elem.Evaled) ||                        // if no such element in the dict
                            dict[elem.Evaled].SimplifiedRate > elem.SimplifiedRate)  // if the one in the dict is more complicated
                            dict[elem.Evaled] = elem;                                // then we add it
                    }
                    return dict;
                }

                public FiniteSet(IEnumerable<Entity> elements) : this(elements, noCheck: false) { }

                private FiniteSet(IEnumerable<Entity> elements, bool noCheck)
                {
                    this.elements = BuildDictionaryFromElements(elements, noCheck);
                    Count = this.elements.Count;
                }

                public void Deconstruct(out IEnumerable<Entity> elements)
                    => elements = Elements;


                /// <summary> Represents number of entities in the current set </summary>
                public int Count { get; }

                /// <summary>
                /// Used for enumerating. Use "foreach" for iterating over elements
                /// </summary>
                public IEnumerator<Entity> GetEnumerator()
                    => Elements.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator()
                    => Elements.GetEnumerator();

                internal static FiniteSet Unite(FiniteSet A, FiniteSet B)
                    => new FiniteSet(A.Elements.Concat(B.Elements));

                // It could be written with one chain request, but readability > one line
                internal static FiniteSet Subtract(FiniteSet A, FiniteSet B)
                {
                    var dict = BuildDictionaryFromElements(A.Elements, noCheck: true);
                    foreach (var el in B)
                        dict.Remove(el.Evaled);
                    return new FiniteSet(dict.Values, noCheck: true); // we didn't add anything
                }

                internal static FiniteSet Intersect(FiniteSet A, FiniteSet B)
                {
                    var dict = BuildDictionaryFromElements(A.Elements, noCheck: true);
                    foreach (var el in B)
                        if (!B.ContainsNode(el.Evaled))
                            dict.Remove(el.Evaled);
                    return new FiniteSet(dict.Values, noCheck: true); // we didn't add anything
                }

                public override bool Contains(Entity entity)
                    => elements.ContainsKey(entity.Evaled);
            }
            #endregion

            #region Interval
            /// <summary>
            /// An interval represents all numbres in between two Entities
            /// <see cref="Interval.LeftClosed"/> stands for whether <see cref="Interval.Left"/> is included
            /// <see cref="Interval.RightClosed"/> stands for whether <see cref="Interval.Right"/> is included
            /// </summary>
            public partial record Interval(Entity Left, bool LeftClosed, Entity Right, bool RightClosed) : Set
            {
                public bool IsNumeric => !left.Value.IsNaN && !right.Value.IsNaN;

                // TODO: maybe it's not good to create an object just to lazily initialize and we need to write our own wheel?
                private readonly Lazy<Real> left = new Lazy<Real>(() => Left.EvaluableNumerical && Left.Evaled is Real re ? re : Real.NaN);
                private readonly Lazy<Real> right = new Lazy<Real>(() => Right.EvaluableNumerical && Right.Evaled is Real re ? re : Real.NaN);

                private static bool IsALessThanB(Real A, Real B, bool closed)
                    => A < B || closed && A == B;

                public Interval New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) 
                    && ReferenceEquals(Right, right)
                    ? this : new Interval(left, LeftClosed, right, RightClosed);

                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                public override bool Contains(Entity entity)
                {
                    if (entity is not Real re)
                        throw new CannotEvalException("The argument should be a real number");
                    if (!IsNumeric)
                        throw new CannotEvalException("The inteval's ends should be real numbers");
                    return IsALessThanB(left.Value, re, LeftClosed) && IsALessThanB(right.Value, re, RightClosed);
                }
            }
            #endregion

        }
    }
}
